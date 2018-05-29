
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.XPath;
using NLog;
using ProcessSmarterTestPackage.Processors.Combined;
using TabulateSmarterTestPackage.Utilities;
using ValidateSmarterTestPackage.Resources;
using ValidateSmarterTestPackage.RestrictedValues.Enums;

namespace TabulateSmarterTestPackage.Tabulators
{
    public class CombinedItemTabulator
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private const int MaxBpRefs = 7;

        public IEnumerable<IEnumerable<string>> ProcessResult(XPathNavigator navigator,
            CombinedTestProcessor testSpecificationProcessor, IDictionary<ItemFieldNames, string> testInformation)
        {
            var resultList = new List<List<string>>();

            var commonTestPackageItems = new SortedDictionary<int,string>();
            
            
            commonTestPackageItems.Add((int)ItemFieldNames.AssessmentId, testInformation[ItemFieldNames.AssessmentId]);
            commonTestPackageItems.Add((int)ItemFieldNames.AssessmentVersion, testInformation[ItemFieldNames.AssessmentVersion]);
            commonTestPackageItems.Add((int)ItemFieldNames.Version, testInformation[ItemFieldNames.Version]);
            commonTestPackageItems.Add((int)ItemFieldNames.AssessmentSubject, testInformation[ItemFieldNames.AssessmentSubject]);
            commonTestPackageItems.Add((int)ItemFieldNames.AssessmentGrade, testInformation[ItemFieldNames.AssessmentGrade]);
            commonTestPackageItems.Add((int)ItemFieldNames.AssessmentType, testInformation[ItemFieldNames.AssessmentType]);
            commonTestPackageItems.Add((int)ItemFieldNames.AcademicYear, testInformation[ItemFieldNames.AcademicYear]);
            commonTestPackageItems.Add((int)ItemFieldNames.BankKey, testInformation[ItemFieldNames.BankKey]);

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
            //List<ItemGroupItem> testItems = new List<ItemGroupItem>();

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
                                    GetAssesmentItemList(test, itemGroup.Item.ToList(), commonTestPackageItems, testInformation, resultList, itemGroup.id, segment);
                                }
                            }
                        }
                        else
                        {
                            var itemGroups = (segment.Item as TestSegmentPool)?.ItemGroup;
                            if (itemGroups != null)
                                foreach (var itemGroup in itemGroups)
                                {
                                    GetAssesmentItemList(test, itemGroup.Item.ToList(), commonTestPackageItems, testInformation, resultList, itemGroup.id, segment);
                                }
                        }
                    }

                }
            }

            /*
           var keys = Enum.GetValues(typeof(ItemFieldNames));

           foreach (var listItem in resultList)
           {
               foreach (var key in keys)
               {

                   var intKey = (int) key;

                   Logger.Debug($"type is {key.GetType()} intKey is {intKey} otherthing is");

               }
           }
           */
            var ordered = resultList.OrderBy(x => x[(int)ItemFieldNames.ItemId]).ToList();
            return ordered;
        }


        private void GetAssesmentItemList(Test test, List<ItemGroupItem> testItems, SortedDictionary<int, string> commonTestPackageItems, 
            IDictionary<ItemFieldNames, string> testInformation, List<List<string>> resultList, String itemGroupId, TestSegment segment)
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
                var bpRefs = GetBpRefs(item);
                string scoringEngine = item.handScored ? "HandScored" : "??NotHandScored??";
                var itemScoreParams = GetItemScoreParameters(item);

                var newList = new SortedDictionary<int, string>(commonTestPackageItems)
                {
                    { (int)ItemFieldNames.AssessmentName, test.id},
                    { (int)ItemFieldNames.AssessmentLabel, test.label },
                    { (int)ItemFieldNames.ItemId, item.id },
                    { (int)ItemFieldNames.FullItemKey, testInformation[ItemFieldNames.BankKey] + $"-{item.id}"},
                    { (int)ItemFieldNames.Filename,  $"item-{testInformation[ItemFieldNames.BankKey]}-{item.id}.xml"}, // item-200-21818.xml"
                    { (int)ItemFieldNames.ItemType, item.type },
                    { (int)ItemFieldNames.AssessmentSubtype, subType },
                    { (int)ItemFieldNames.Grade, test.Grades[0].value },
                    { (int)ItemFieldNames.Standard, ids["Standard"]},
                    { (int)ItemFieldNames.Claim, ids["Claim"]},
                    { (int)ItemFieldNames.Target, ids["Target"]},
                    { (int)ItemFieldNames.PassageId, $"{testInformation[ItemFieldNames.BankKey]}-{itemGroupId}" },
                    { (int)ItemFieldNames.ASL, String.Empty }, //wut?
                    { (int)ItemFieldNames.Braille,  String.Empty},
                    { (int)ItemFieldNames.LanguageBraille, langs[(int)ItemFieldNames.LanguageBraille] },
                    { (int)ItemFieldNames.DOK, String.Empty },
                    { (int)ItemFieldNames.Language, langs[(int)ItemFieldNames.Language] },
                    { (int)ItemFieldNames.AllowCalculator, String.Empty },
                    { (int)ItemFieldNames.MathematicalPractice, String.Empty },
                    { (int)ItemFieldNames.MaxPoints, item.ItemScoreDimension.scorePoints.ToString() },
                    { (int)ItemFieldNames.Glossary, String.Empty },
                    { (int)ItemFieldNames.ScoringEngine, scoringEngine },
                    { (int)ItemFieldNames.Spanish, langs[(int)ItemFieldNames.Spanish] },
                    { (int)ItemFieldNames.IsFieldTest, item.fieldTest ? "TRUE" : "FALSE" },
                    { (int)ItemFieldNames.IsActive, item.active ? "TRUE" : "FALSE"  },
                    { (int)ItemFieldNames.ResponseRequired, item.responseRequired ? "TRUE" : "FALSE"  },
                    { (int)ItemFieldNames.AdminRequired, item.administrationRequired ? "TRUE" : "FALSE"  },
                    { (int)ItemFieldNames.ItemPosition, segment.position.ToString() },
                    { (int)ItemFieldNames.MeasurementModel, item.ItemScoreDimension.measurementModel },
                    { (int)ItemFieldNames.Weight, item.ItemScoreDimension.weight.ToString(CultureInfo.InvariantCulture) },
                    { (int)ItemFieldNames.ScorePoints, item.ItemScoreDimension.scorePoints.ToString() },
                    { (int)ItemFieldNames.a, itemScoreParams[(int)ItemFieldNames.a] },
                    { (int)ItemFieldNames.b0_b, itemScoreParams[(int)ItemFieldNames.b0_b] },
                    { (int)ItemFieldNames.b1_c, itemScoreParams[(int)ItemFieldNames.b1_c] },
                    { (int)ItemFieldNames.b2, itemScoreParams[(int)ItemFieldNames.b2] },
                    { (int)ItemFieldNames.b3, itemScoreParams[(int)ItemFieldNames.b3] },
                    { (int)ItemFieldNames.avg_b, itemScoreParams[(int)ItemFieldNames.avg_b] },
                    { (int)ItemFieldNames.CommonCore, String.Empty },
                    { (int)ItemFieldNames.ClaimContentTarget, String.Empty },
                    { (int)ItemFieldNames.SecondaryCommonCore, String.Empty },
                    { (int)ItemFieldNames.SecondaryClaimContentTarget, String.Empty },
                    { (int)ItemFieldNames.AnswerKey, String.Empty },
                    { (int)ItemFieldNames.NumberOfAnswerOptions, String.Empty },
                    { (int)ItemFieldNames.HandScored, item.handScored ? "TRUE" : "FALSE"  },
                    { (int)ItemFieldNames.DoNotScore, item.doNotScore ? "TRUE" : "FALSE"  },

                };
                
                foreach (var bpRef in bpRefs)
                {
                    newList.Add(bpRef.Key, bpRef.Value);
                }
                
                resultList.Add(newList.Values.ToList());
            }
        }

        private SortedDictionary<int, string> GetItemScoreParameters(ItemGroupItem item)
        {
            var scoreParams = new SortedDictionary<int, string>();
            foreach (var isp in item.ItemScoreDimension.ItemScoreParameter)
            {
                if (isp.measurementParameter.Equals("a", StringComparison.Ordinal))
                {
                    scoreParams.Add((int)ItemFieldNames.a, isp.value.ToString());
                } else if (isp.measurementParameter.Equals("b0", StringComparison.Ordinal) || isp.measurementParameter.Equals("b", StringComparison.Ordinal))
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

            var avg_b = MathHelper.CalculateAverageB(item.ItemScoreDimension.measurementModel,
                scoreParams[(int)ItemFieldNames.a], scoreParams[(int)ItemFieldNames.b0_b],
                scoreParams[(int)ItemFieldNames.b1_c], scoreParams[(int)ItemFieldNames.b2],
                scoreParams[(int)ItemFieldNames.b3], item.ItemScoreDimension.scorePoints.ToString());
            if (!avg_b.Errors.Any())
            {
                scoreParams[(int)ItemFieldNames.avg_b] = avg_b.Value;
            }
            else
            {
                scoreParams[(int)ItemFieldNames.avg_b] = String.Empty;
                Logger.Error("There was an error calulating the avg_b ");
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

                if (!langs.ContainsKey((int) ItemFieldNames.Language))
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

        private Dictionary<int, string> GetBpRefs(ItemGroupItem item)
        {
            var bpRefs = new Dictionary<int, string>();
            var i = 0;
            var bps = new List<ItemFieldNames>{ItemFieldNames.bpref1, ItemFieldNames.bpref2, ItemFieldNames.bpref3, ItemFieldNames.bpref4, ItemFieldNames.bpref5, ItemFieldNames.bpref6, ItemFieldNames.bpref7};


            
            foreach(var bpRef in item.BlueprintReferences)
            {
                if (i < MaxBpRefs)
                {
                    bpRefs.Add((int) ItemFieldNames.bpref1 + i, "SBAC-" + bpRef.idRef);
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
                            ids["Claim"] = parts[0] + "\t";
                            ids["Target"] = parts[1] + "\t";
                        }
                        
                    }
                } else if (subject.Equals("MATH", StringComparison.OrdinalIgnoreCase))
                {
                    var parts = bpRef.idRef.Split('|');
                    if (parts.Length == 4)
                    {
                        ids["Standard"] = $"SBAC-MA-v6:{bpRef.idRef}";
                        ids["Claim"] = parts[0] + "\t";
                        ids["Target"] = parts[1] + "\t";
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