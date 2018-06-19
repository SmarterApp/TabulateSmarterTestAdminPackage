
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.XPath;
using NLog;
using ProcessSmarterTestPackage.Processors.Combined;
using SmarterTestPackage.Common.Data;
using TabulateSmarterTestPackage.Utilities;
using ValidateSmarterTestPackage.RestrictedValues.Enums;

namespace TabulateSmarterTestPackage.Tabulators
{
    public class CombinedItemTabulator
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private const int MaxBpRefs = 7;
        private readonly Dictionary<string, int> sPoolPropertyMapping;

        public CombinedItemTabulator()
        {
            sPoolPropertyMapping = new Dictionary<string, int>
            {
                {"ASL", (int) ItemFieldNames.ASL},
                {"Braille", (int) ItemFieldNames.Braille},
                {"Depth of Knowledge", (int) ItemFieldNames.DOK},
                {"Grade", (int) ItemFieldNames.Grade},
                //{"Language", (int) ItemFieldNames.Language},
                {"Language", 0},
                {"Scoring Engine", (int) ItemFieldNames.ScoringEngine},
                {"Spanish Translation", (int) ItemFieldNames.Spanish},
                {"Calculator", (int) ItemFieldNames.AllowCalculator},
                {"Glossary", (int) ItemFieldNames.Glossary},
                // Ignore these pool properties
                // Value of zero means suppress
                {"--ITEMTYPE--", 0},
                {"Difficulty Category", 0},
                {"Test Pool", 0},
                {"Rubric Source", 0},
                {"Smarter Balanced Item Response Types", 0},
                {"Answer Key", 0},
                {"Claim2_Category", 0},
                {"Revision Sub-category", 0}
            };
        }

        public IEnumerable<IEnumerable<string>> ProcessResult(XPathNavigator navigator,
            CombinedTestProcessor testSpecificationProcessor, IDictionary<ItemFieldNames, string> testInformation)
        {
            Logger.Info($"Tabulating {testSpecificationProcessor.GetUniqueId()}");
            var resultList = new List<List<string>>();

            var commonTestPackageItems = new SortedDictionary<int, string>
            {
                { (int)ItemFieldNames.AssessmentId, testInformation[ItemFieldNames.AssessmentId] },
                { (int)ItemFieldNames.AssessmentVersion, testInformation[ItemFieldNames.AssessmentVersion] },
                { (int)ItemFieldNames.Version, testInformation[ItemFieldNames.Version] },
                { (int)ItemFieldNames.AssessmentSubject, testInformation[ItemFieldNames.AssessmentSubject] },
                { (int)ItemFieldNames.AssessmentGrade, testInformation[ItemFieldNames.AssessmentGrade] },
                { (int)ItemFieldNames.AssessmentType, testInformation[ItemFieldNames.AssessmentType] },
                { (int)ItemFieldNames.AcademicYear, testInformation[ItemFieldNames.AcademicYear] },
                { (int)ItemFieldNames.BankKey, testInformation[ItemFieldNames.BankKey] }
            };

            commonTestPackageItems[(int)ItemFieldNames.CutPoint1] = testInformation[ItemFieldNames.CutPoint1];
            commonTestPackageItems[(int)ItemFieldNames.ScaledHigh1] = testInformation[ItemFieldNames.ScaledHigh1];
            commonTestPackageItems[(int)ItemFieldNames.ScaledLow1] = testInformation[ItemFieldNames.ScaledLow1];
            commonTestPackageItems[(int)ItemFieldNames.CutPoint2] = testInformation[ItemFieldNames.CutPoint2];
            commonTestPackageItems[(int)ItemFieldNames.ScaledHigh2] = testInformation[ItemFieldNames.ScaledHigh2];
            commonTestPackageItems[(int)ItemFieldNames.ScaledLow2] = testInformation[ItemFieldNames.ScaledLow2];
            commonTestPackageItems[(int)ItemFieldNames.CutPoint3] = testInformation[ItemFieldNames.CutPoint3];
            commonTestPackageItems[(int)ItemFieldNames.ScaledHigh3] = testInformation[ItemFieldNames.ScaledHigh3];
            commonTestPackageItems[(int)ItemFieldNames.ScaledLow3] = testInformation[ItemFieldNames.ScaledLow3];
            commonTestPackageItems[(int)ItemFieldNames.CutPoint4] = testInformation[ItemFieldNames.CutPoint4];
            commonTestPackageItems[(int)ItemFieldNames.ScaledHigh4] = testInformation[ItemFieldNames.ScaledHigh4];
            commonTestPackageItems[(int)ItemFieldNames.ScaledLow4] = testInformation[ItemFieldNames.ScaledLow4];


            var testPackage = testSpecificationProcessor.TestPackage;
            var tests = testPackage.Test;

            foreach (var test in tests)
            {
                foreach (var segment in test.Segments)
                {
                    var segmentForms =
                        segment.Item is TestSegmentSegmentForms forms ? forms.SegmentForm : null;
                    if (segmentForms != null)
                    {
                        if (segment.algorithmType.Equals(Algorithm.FIXEDFORM,
                            StringComparison.CurrentCultureIgnoreCase))
                        {
                            foreach (var segmentForm in segmentForms)
                            {
                                foreach (var itemGroup in segmentForm.ItemGroup)
                                {
                                    GetAssesmentItemList(test, itemGroup.Item.ToList(), commonTestPackageItems, testInformation, resultList, itemGroup, segment, testPackage.publisher, testPackage.academicYear);
                                }
                            }
                        }
                        else
                        {
                            var itemGroups = (segment.Item as TestSegmentPool)?.ItemGroup;
                            if (itemGroups != null)
                                foreach (var itemGroup in itemGroups)
                                {
                                    GetAssesmentItemList(test, itemGroup.Item.ToList(), commonTestPackageItems, testInformation, resultList, itemGroup, segment, testPackage.publisher, testPackage.academicYear);
                                }
                        }
                    }

                }
            }

            return resultList.OrderBy(x => x[(int)ItemFieldNames.ItemId]).ToList();
        }

        private SortedDictionary<int, string> GetPoolProperties(ItemGroupItem item, IDictionary<ItemFieldNames, string> testInformation)
        {
            var poolProperties = new SortedDictionary<int, string>();
            var glossary = new List<string>();
            if (item.PoolProperties != null && item.PoolProperties.Length > 0)
            {
                foreach (var poolProperty in item.PoolProperties)
                {
                    var ppProperty = poolProperty.name;
                    var ppValue = poolProperty.value;
                    if (string.IsNullOrEmpty(ppProperty) || string.IsNullOrEmpty(ppValue))
                    {
                        continue;
                    }
                    // Special case for Braille language
                    int fieldIndex;
                    if (ppProperty.Equals("Language", StringComparison.Ordinal) &&
                        ppValue.Equals("ENU-Braille", StringComparison.Ordinal))
                    {
                        poolProperties[(int)ItemFieldNames.LanguageBraille] = ppValue;
                    }
                    // Special case for Spanish language
                    else if (ppProperty.Equals("Language", StringComparison.Ordinal) &&
                             ppValue.Equals("ESN", StringComparison.Ordinal))
                    {
                        poolProperties[(int)ItemFieldNames.Spanish] = "Y";
                    }
                    // Special case for Spanish language
                    else if (ppProperty.Equals("Spanish Translation", StringComparison.Ordinal))
                    {
                        poolProperties[(int)ItemFieldNames.Spanish] = ppValue;
                    }
                    // Special case for Glossary
                    else if (ppProperty.Equals("Glossary", StringComparison.Ordinal))
                    {
                        glossary.Add(ppValue);
                    }
                    else if (ppProperty.Equals("Allow Calculator", StringComparison.OrdinalIgnoreCase))
                    {
                        poolProperties[(int)ItemFieldNames.AllowCalculator] = ppValue;
                    }
                    else if (sPoolPropertyMapping.TryGetValue(ppProperty, out fieldIndex) && fieldIndex != 0)
                    {
                        if (poolProperties.ContainsKey(fieldIndex))
                        {
                            ReportingUtility.ReportError(testInformation[ItemFieldNames.AssessmentId],
                                PackageType.Combined,
                                "TestPackage/Test/Segments/SegmentForms/SegmentForm/ItemGroup/Item/PoolProperties/PoolProperty",
                                ErrorSeverity.Degraded, item.id, ppValue,
                                "'{0}={1}' Multiple values for pool property", ppProperty, ppValue);
                        }
                        poolProperties[fieldIndex] = ppValue;
                    }
                }
            }
            glossary.Sort();
            poolProperties[(int)ItemFieldNames.Glossary] = string.Join(";", glossary);
            return poolProperties;
        }

        private int GetItemPosition(TestSegment segment, ItemGroup itemGroup, string itemId)
        {
            var segmentForms =
                segment.Item is TestSegmentSegmentForms forms ? forms.SegmentForm : null;
            if (segmentForms != null)
            {
                var position = 0;
                foreach (var segForm in segmentForms)
                {
                   
                    foreach (var ig in segForm.ItemGroup)
                    {
                        int index = -1;
                        int i = 0;
                        foreach (var item in ig.Item)
                        {
                            if (item.id.Equals(itemId))
                            {
                                index = i;
                                break;
                            }

                            i++;
                        }

                        if (index != -1)
                        {
                            position += index + 1;
                            break;
                        }
                        else
                        {
                            position += ig.Item.Length;
                        }

                    }
                   
                }

                return position;
            }
            else
            {
                var i = 1;
                foreach (var item in itemGroup.Item)
                {
                    if (item.id.Equals(itemId))
                    {
                        return i;
                    }
                    i++;
                }

                return i;
            }
        }

        private void GetAssesmentItemList(Test test, List<ItemGroupItem> testItems, SortedDictionary<int, string> commonTestPackageItems, 
            IDictionary<ItemFieldNames, string> testInformation, List<List<string>> resultList, ItemGroup itemGroup, TestSegment segment, String publisher, String academicYear)
        {
            var subType = "summative";
            if (!testInformation[ItemFieldNames.AssessmentType].Equals("summative", StringComparison.OrdinalIgnoreCase))
            {
                if (test.id.Contains("ICA"))
                {
                    subType = "ICA";
                }
                else if (test.id.Contains("IAB"))
                {
                    subType = "IAB";
                }
                else
                {
                    subType = string.Empty;
                    
                }
            }


            
            foreach (var item in testItems)
            {
                var ids = GetStandardIDs(item, testInformation[ItemFieldNames.AssessmentSubject]);
                var langs = GetLanguages(item);
                var poolProperties = GetPoolProperties(item, testInformation);
                var bpRefs = GetBpRefs(item, publisher, academicYear);
                var itemScoreParams = GetItemScoreParameters(item, testInformation);
                var itemPosition = GetItemPosition(segment, itemGroup, item.id);
                var crossTabs = GetCrossTabulationItems(item);

                var newList = new SortedDictionary<int, string>(commonTestPackageItems)
                {
                    { (int)ItemFieldNames.AssessmentName, test.id},
                    { (int)ItemFieldNames.AssessmentLabel, test.label },
                    { (int)ItemFieldNames.ItemId, item.id },
                    { (int)ItemFieldNames.FullItemKey, testInformation[ItemFieldNames.BankKey] + $"-{item.id}"},
                    { (int)ItemFieldNames.Filename,  $"item-{testInformation[ItemFieldNames.BankKey]}-{item.id}.xml"}, // item-200-21818.xml"
                    { (int)ItemFieldNames.ItemType, item.type },
                    { (int)ItemFieldNames.AssessmentSubtype, subType },
                    { (int)ItemFieldNames.Standard, ids["Standard"]},
                    { (int)ItemFieldNames.Claim, ids["Claim"]},
                    { (int)ItemFieldNames.Target, ids["Target"]},
                    { (int)ItemFieldNames.PassageId, $"{testInformation[ItemFieldNames.BankKey]}-{itemGroup.id}" },
                    { (int)ItemFieldNames.ASL, poolProperties.ContainsKey((int)ItemFieldNames.ASL) ? poolProperties[(int)ItemFieldNames.ASL] : String.Empty },
                    { (int)ItemFieldNames.Braille, poolProperties.ContainsKey((int)ItemFieldNames.Braille) ? poolProperties[(int)ItemFieldNames.Braille] : String.Empty },
                    { (int)ItemFieldNames.LanguageBraille, langs[(int)ItemFieldNames.LanguageBraille] }, //TODO does this come from poolproperties or elsewhere?
                    { (int)ItemFieldNames.DOK, poolProperties.ContainsKey((int)ItemFieldNames.DOK) ? poolProperties[(int)ItemFieldNames.DOK] : String.Empty },
                    { (int)ItemFieldNames.Language, langs[(int)ItemFieldNames.Language] },
                    { (int)ItemFieldNames.AllowCalculator, poolProperties.ContainsKey((int)ItemFieldNames.AllowCalculator) ? poolProperties[(int)ItemFieldNames.AllowCalculator] : String.Empty },
                    { (int)ItemFieldNames.MathematicalPractice, String.Empty },
                    { (int)ItemFieldNames.Grade, poolProperties.ContainsKey((int)ItemFieldNames.Grade) ? poolProperties[(int)ItemFieldNames.Grade] : String.Empty },
                    { (int)ItemFieldNames.MaxPoints, item.ItemScoreDimensions[0].scorePoints.ToString() },      //ItemScoreDimension.scorePoints.ToString() },
                    { (int)ItemFieldNames.Glossary, poolProperties.ContainsKey((int)ItemFieldNames.Glossary) ? poolProperties[(int)ItemFieldNames.Glossary] : String.Empty },
                    { (int)ItemFieldNames.ScoringEngine, poolProperties.ContainsKey((int)ItemFieldNames.ScoringEngine) ? poolProperties[(int)ItemFieldNames.ScoringEngine] : String.Empty },
                    { (int)ItemFieldNames.Spanish, poolProperties.ContainsKey((int)ItemFieldNames.Spanish) ? poolProperties[(int)ItemFieldNames.Spanish] : String.Empty },
                    { (int)ItemFieldNames.IsFieldTest, item.fieldTest ? "TRUE" : "FALSE" },
                    { (int)ItemFieldNames.IsActive, item.active ? "TRUE" : "FALSE"  },
                    { (int)ItemFieldNames.ResponseRequired, item.responseRequired ? "TRUE" : "FALSE"  },
                    { (int)ItemFieldNames.AdminRequired, item.administrationRequired ? "TRUE" : "FALSE"  },
                    { (int)ItemFieldNames.ItemPosition, itemPosition.ToString() },
                    { (int)ItemFieldNames.MeasurementModel, item.ItemScoreDimensions[0].measurementModel },
                    { (int)ItemFieldNames.Weight, item.ItemScoreDimensions[0].weight.ToString(CultureInfo.InvariantCulture) },
                    { (int)ItemFieldNames.ScorePoints, item.ItemScoreDimensions[0].scorePoints.ToString() },
                    { (int)ItemFieldNames.a, itemScoreParams[(int)ItemFieldNames.a] },
                    { (int)ItemFieldNames.b0_b, itemScoreParams[(int)ItemFieldNames.b0_b] },
                    { (int)ItemFieldNames.b1_c, itemScoreParams[(int)ItemFieldNames.b1_c] },
                    { (int)ItemFieldNames.b2, itemScoreParams[(int)ItemFieldNames.b2] },
                    { (int)ItemFieldNames.b3, itemScoreParams[(int)ItemFieldNames.b3] },
                    { (int)ItemFieldNames.avg_b, itemScoreParams[(int)ItemFieldNames.avg_b] },
                    { (int)ItemFieldNames.CommonCore, crossTabs.ContainsKey((int)ItemFieldNames.CommonCore) ? crossTabs[(int)ItemFieldNames.CommonCore] : String.Empty },
                    { (int)ItemFieldNames.ClaimContentTarget, crossTabs.ContainsKey((int)ItemFieldNames.ClaimContentTarget) ? crossTabs[(int)ItemFieldNames.ClaimContentTarget] : String.Empty },
                    { (int)ItemFieldNames.SecondaryCommonCore, crossTabs.ContainsKey((int)ItemFieldNames.SecondaryCommonCore) ? crossTabs[(int)ItemFieldNames.SecondaryCommonCore] : String.Empty },
                    { (int)ItemFieldNames.SecondaryClaimContentTarget, crossTabs.ContainsKey((int)ItemFieldNames.SecondaryClaimContentTarget) ? crossTabs[(int)ItemFieldNames.SecondaryClaimContentTarget] : String.Empty },
                    { (int)ItemFieldNames.AnswerKey, crossTabs.ContainsKey((int)ItemFieldNames.AnswerKey) ? crossTabs[(int)ItemFieldNames.AnswerKey] : String.Empty },
                    //{ (int)ItemFieldNames.AnswerKey, poolProperties.ContainsKey((int)ItemFieldNames.AnswerKey) ? poolProperties[(int)ItemFieldNames.AnswerKey] : String.Empty }, //poolproperty or crossTabulation takes precedence?
                    { (int)ItemFieldNames.NumberOfAnswerOptions, crossTabs.ContainsKey((int)ItemFieldNames.NumberOfAnswerOptions) ? crossTabs[(int)ItemFieldNames.NumberOfAnswerOptions] : String.Empty },
                    { (int)ItemFieldNames.HandScored, item.handScored ? "TRUE" : "FALSE"  },
                    { (int)ItemFieldNames.DoNotScore, item.doNotScore ? "TRUE" : "FALSE"  }

                };
                
                foreach (var bpRef in bpRefs)
                {
                    newList.Add(bpRef.Key, bpRef.Value);
                }
                
                resultList.Add(newList.Values.ToList());
            }

           
        }

        private SortedDictionary<int, string> GetCrossTabulationItems(ItemGroupItem item)
        {
            var itemFields = new SortedDictionary<int, string>();
            if (ReportingUtility.CrossProcessor != null &&
                ReportingUtility.CrossProcessor.ItemContentPackage != null)
            {
                var contentItem = ReportingUtility.CrossProcessor.ItemContentPackage.FirstOrDefault(
                    x => x.ItemId.Equals(item.id, StringComparison.OrdinalIgnoreCase));

                itemFields[(int)ItemFieldNames.CommonCore] = contentItem?.CommonCore ?? string.Empty;
                itemFields[(int)ItemFieldNames.ClaimContentTarget] = contentItem?.ClaimContentTarget ??
                                                                     string.Empty;
                itemFields[(int)ItemFieldNames.SecondaryCommonCore] = contentItem?.SecondaryCommonCore ??
                                                                      string.Empty;
                itemFields[(int)ItemFieldNames.SecondaryClaimContentTarget] =
                    contentItem?.SecondaryClaimContentTarget ?? string.Empty;

                itemFields[(int)ItemFieldNames.AnswerKey] = contentItem?.AnswerKey ?? string.Empty;

                itemFields[(int)ItemFieldNames.NumberOfAnswerOptions] = contentItem?.NumberOfAnswerOptions ?? string.Empty;
            }
            return itemFields;
        }

        private SortedDictionary<int, string> GetItemScoreParameters(ItemGroupItem item, IDictionary<ItemFieldNames, string> testInformation)
        {
            var scoreParams = new SortedDictionary<int, string>();
            if (item.ItemScoreDimensions != null && !item.ItemScoreDimensions[0].measurementModel.Equals("RAWSCORE"))
            {
                foreach (var isp in item.ItemScoreDimensions[0].ItemScoreParameter)
                {
                    if (isp.measurementParameter.Equals("a", StringComparison.Ordinal))
                    {
                        scoreParams.Add((int)ItemFieldNames.a, isp.value.ToString());
                    }
                    else if (isp.measurementParameter.Equals("b0", StringComparison.Ordinal) || isp.measurementParameter.Equals("b", StringComparison.Ordinal))
                    {
                        scoreParams.Add((int)ItemFieldNames.b0_b, isp.value.ToString());
                    }
                    else if (isp.measurementParameter.Equals("b1", StringComparison.Ordinal) || isp.measurementParameter.Equals("c", StringComparison.Ordinal))
                    {
                        scoreParams.Add((int)ItemFieldNames.b1_c, isp.value.ToString());
                    }
                    else if (isp.measurementParameter.Equals("b2", StringComparison.Ordinal))
                    {
                        scoreParams.Add((int)ItemFieldNames.b2, isp.value.ToString());
                    }
                    else if (isp.measurementParameter.Equals("b3", StringComparison.Ordinal))
                    {
                        scoreParams.Add((int)ItemFieldNames.b3, isp.value.ToString());
                    }
                }
            }

            if (!scoreParams.ContainsKey((int)ItemFieldNames.a))
            {
                scoreParams.Add((int)ItemFieldNames.a, String.Empty);
            }
            if (!scoreParams.ContainsKey((int)ItemFieldNames.b0_b))
            {
                scoreParams.Add((int)ItemFieldNames.b0_b, String.Empty);
            }
            if (!scoreParams.ContainsKey((int)ItemFieldNames.b1_c))
            {
                scoreParams.Add((int)ItemFieldNames.b1_c, String.Empty);
            }
            if (!scoreParams.ContainsKey((int)ItemFieldNames.b2))
            {
                scoreParams.Add((int)ItemFieldNames.b2, String.Empty);
            }
            if (!scoreParams.ContainsKey((int)ItemFieldNames.b3))
            {
                scoreParams.Add((int)ItemFieldNames.b3, String.Empty);
            }

            if (item.ItemScoreDimensions != null && !item.ItemScoreDimensions[0].measurementModel.Equals("RAWSCORE"))
            {
                var avg_b = MathHelper.CalculateAverageB(item.ItemScoreDimensions[0].measurementModel,
                    scoreParams[(int)ItemFieldNames.a], scoreParams[(int)ItemFieldNames.b0_b],
                    scoreParams[(int)ItemFieldNames.b1_c], scoreParams[(int)ItemFieldNames.b2],
                    scoreParams[(int)ItemFieldNames.b3], item.ItemScoreDimensions[0].scorePoints.ToString());
                if (!avg_b.Errors.Any())
                {
                    scoreParams[(int)ItemFieldNames.avg_b] = avg_b.Value;
                }
                else
                {
                    scoreParams[(int)ItemFieldNames.avg_b] = String.Empty;
                    avg_b.Errors.ToList().ForEach(x =>
                        ReportingUtility.ReportError(testInformation[ItemFieldNames.AssessmentId],
                            PackageType.Combined,
                            "TestPackage/Test/Segments/SegmentForms/SegmentForm/ItemGroup/Item/ItemScoreDimension",
                            ErrorSeverity.Degraded, item.id, String.Empty, x)
                    );
                    Logger.Error($"There was an error calulating the avg_b for Item id {item.id}");
                }
            }
            else
            {
                scoreParams[(int)ItemFieldNames.avg_b] = String.Empty;
            }
            

            return scoreParams;
        }


        private SortedDictionary<int, string> GetLanguages(ItemGroupItem item)
        {
            var langs = new SortedDictionary<int, string>();
            foreach (var pres in item.Presentations)
            {
                if (pres.code.Equals("ENU-Braille", StringComparison.Ordinal))
                {
                    langs.Add((int)ItemFieldNames.LanguageBraille, pres.code);
                } else if (pres.code.Equals("ESN"))
                {
                    langs.Add((int)ItemFieldNames.Spanish, "Y");
                }
                else if (pres.code.Equals("Spanish Translation"))
                {
                    langs.Add((int)ItemFieldNames.Spanish, pres.code);
                }

                else if (!langs.ContainsKey((int) ItemFieldNames.Language))
                {
                    langs.Add((int)ItemFieldNames.Language, pres.code);
                }
            }

            if (!langs.ContainsKey((int) ItemFieldNames.LanguageBraille))
            {
                langs.Add((int)ItemFieldNames.LanguageBraille, String.Empty);
            }
            if (!langs.ContainsKey((int)ItemFieldNames.Spanish))
            {
                langs.Add((int)ItemFieldNames.Spanish, "N");
            }
            if (!langs.ContainsKey((int)ItemFieldNames.Language))
            {
                langs.Add((int)ItemFieldNames.Language, String.Empty);
            }

            return langs;
        }

        private Dictionary<int, string> GetBpRefs(ItemGroupItem item, String publisher, String academicYear)
        {
            var bpRefs = new Dictionary<int, string>();
            var i = 0;
            var bps = new List<ItemFieldNames>{ItemFieldNames.bpref1, ItemFieldNames.bpref2, ItemFieldNames.bpref3, ItemFieldNames.bpref4, ItemFieldNames.bpref5, ItemFieldNames.bpref6, ItemFieldNames.bpref7};


            
            foreach(var bpRef in item.BlueprintReferences)
            {
                if (i == 0)
                {
                    bpRefs.Add((int)ItemFieldNames.bpref1 + i, $"({publisher}){bpRef.idRef}-{academicYear}");
                }
                else if (i < MaxBpRefs)
                {
                    bpRefs.Add((int) ItemFieldNames.bpref1 + i, $"{publisher}-{bpRef.idRef}");
                }

                i++;
            }
            
            foreach (var bp in bps)
            {
                if (!bpRefs.ContainsKey((int)bp))
                {
                    bpRefs[(int)bp] = String.Empty;
                }
            }
            return bpRefs;
        }

        private Dictionary<string, string> GetStandardIDs(ItemGroupItem item, String subject)
        {
            var ids = new Dictionary<string, string>();
            foreach (var bpRef in item.BlueprintReferences)
            {
                if (subject.Equals("ELA", StringComparison.OrdinalIgnoreCase))
                {
                    if (bpRef.idRef.Contains("|"))
                    {
                        var parts = bpRef.idRef.Split('|');
                        if (parts.Length <= 2)
                        {
                            ids["Standard"] = $"SBAC-ELA-v1:{bpRef.idRef}";
                            ids["Claim"] = parts[0];
                            ids["Target"] = parts[1] + "\t";
                        }
                        
                    }
                } else if (subject.Equals("MATH", StringComparison.OrdinalIgnoreCase))
                {
                    var parts = bpRef.idRef.Split('|');
                    if (parts.Length == 4)
                    {
                        ids["Standard"] = $"SBAC-MA-v6:{bpRef.idRef}";
                        ids["Claim"] = parts[0];
                        ids["Target"] = parts[parts.Length-1] + "\t";
                    }
                }

            }

            if (ids.Count == 0)
            {
                ids.Add("Standard", String.Empty);
                ids.Add("Claim", String.Empty);
                ids.Add("Target", String.Empty);
            }

            return ids;
        }
    }
}