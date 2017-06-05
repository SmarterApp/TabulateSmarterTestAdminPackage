using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.XPath;
using ProcessSmarterTestPackage.Processors.Common;
using SmarterTestPackage.Common.Data;
using SmarterTestPackage.Common.Extensions;
using TabulateSmarterTestPackage.Utilities;
using ValidateSmarterTestPackage.RestrictedValues.Enums;

namespace TabulateSmarterTestPackage.Tabulators
{
    public class ItemTabulator
    {
        private const int MaxBpRefs = 7;
        private const string c_MathStdPrefix = "SBAC-MA-v6:";
        private const string c_ElaStdPrefix = "SBAC-ELA-v1:";

        private static readonly int ItemFieldNamesCount = Enum.GetNames(typeof(ItemFieldNames)).Length;

        private static readonly XPathExpression sXp_Parameter1 =
            XPathExpression.Compile("itemscoredimension/itemscoreparameter[@measurementparameter='a']/@value");

        private static readonly XPathExpression sXp_Parameter2 =
            XPathExpression.Compile(
                "itemscoredimension/itemscoreparameter[@measurementparameter='b' or @measurementparameter='b0']/@value");

        private static readonly XPathExpression sXp_Parameter3 =
            XPathExpression.Compile(
                "itemscoredimension/itemscoreparameter[@measurementparameter='c' or @measurementparameter='b1']/@value");

        private static readonly XPathExpression sXp_Parameter4 =
            XPathExpression.Compile("itemscoredimension/itemscoreparameter[@measurementparameter='b2']/@value");

        private static readonly XPathExpression sXp_Parameter5 =
            XPathExpression.Compile("itemscoredimension/itemscoreparameter[@measurementparameter='b3']/@value");

        // GroupItem Selector
        // /testspecification/administration/testform/formpartition/itemgroup/groupitem
        // /testspecification/administration/adminsegment/segmentpool/itemgroup/groupitem
        private static readonly XPathExpression sXp_GroupItem = XPathExpression.Compile("/testspecification//groupitem");

        // Parse bpref entries to standard, claim, target
        private static readonly Regex s_Rx_BprefMath =
            new Regex(@"^SBAC-(\d)\|[A-Z]{1,4}\|[0-9A-Z]{1,4}\|([A-Z]+(?:-\d+)?)$");

        private static readonly Regex s_Rx_BprefEla =
            new Regex(@"^SBAC-(\d-[A-Z]{1,2})\|(\d{1,2}-\d{1,2})(?:\|[A-Za-z0-9\-\.]+)?$");

        private readonly Dictionary<string, int> sPoolPropertyMapping;

        public ItemTabulator()
        {
            sPoolPropertyMapping = new Dictionary<string, int>
            {
                {"ASL", (int) ItemFieldNames.ASL},
                {"Braille", (int) ItemFieldNames.Braille},
                {"Depth of Knowledge", (int) ItemFieldNames.DOK},
                {"Grade", (int) ItemFieldNames.Grade},
                {"Language", (int) ItemFieldNames.Language},
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
            TestSpecificationProcessor testSpecificationProcessor, IDictionary<ItemFieldNames, string> testInformation)
        {
            var resultList = new List<List<string>>();
            // Index the group item info
            var indexGroupItemInfo = new Dictionary<string, GroupItemInfo>();

            var groupItemNodes = navigator.Select(sXp_GroupItem);
            while (groupItemNodes.MoveNext())
            {
                var node = groupItemNodes.Current;
                var itemId = FormatHelper.Strip200(node.GetAttribute("itemid", string.Empty));
                var isFieldTest = node.GetAttribute("isfieldtest", string.Empty);
                var isActive = node.GetAttribute("isactive", string.Empty);
                var responseRequired = node.GetAttribute("responserequired", string.Empty);
                var adminRequired = node.GetAttribute("adminrequired", string.Empty);
                var formPosition = node.GetAttribute("formposition", string.Empty);

                GroupItemInfo gii;
                if (indexGroupItemInfo.TryGetValue(itemId, out gii))
                {
                    continue;
                }
                gii = new GroupItemInfo
                {
                    IsFieldTest = isFieldTest,
                    IsActive = isActive,
                    ResponseRequired = responseRequired,
                    AdminRequired = adminRequired,
                    FormPosition = formPosition
                };
                indexGroupItemInfo.Add(itemId, gii);
            }


            var performanceLevels = new List<PerformanceLevel>();
            if (testSpecificationProcessor.ChildNodesWithName("scoring").Any())
            {
                performanceLevels =
                    testSpecificationProcessor
                        .ChildNodeWithName("scoring")
                        .ChildNodeWithName("performancelevels")
                        .ChildNodesWithName("performancelevel").Select(x => new PerformanceLevel
                        {
                            PerfLevel = x.ValueForAttribute("plevel"),
                            ScaledLow = x.ValueForAttribute("scaledlo"),
                            ScaledHigh = x.ValueForAttribute("scaledhi")
                        }).ToList();
            }

            var testItems =
                testSpecificationProcessor.ChildNodeWithName(testSpecificationProcessor.PackageType.ToString().ToLower())
                    .ChildNodeWithName("itempool")
                    .ChildNodesWithName("testitem");
            foreach (var testItem in testItems)
            {
                // Collect the item fields
                var itemFields = new string[ItemFieldNamesCount + performanceLevels.Count * 3];

                itemFields[(int) ItemFieldNames.AssessmentId] = testInformation[ItemFieldNames.AssessmentId];
                itemFields[(int) ItemFieldNames.AssessmentName] = testInformation[ItemFieldNames.AssessmentName];
                itemFields[(int) ItemFieldNames.AssessmentLabel] = testInformation[ItemFieldNames.AssessmentLabel];
                itemFields[(int) ItemFieldNames.AssessmentVersion] = testInformation[ItemFieldNames.AssessmentVersion];
                itemFields[(int) ItemFieldNames.AssessmentSubject] = testInformation[ItemFieldNames.AssessmentSubject];
                itemFields[(int) ItemFieldNames.AssessmentGrade] = testInformation[ItemFieldNames.AssessmentGrade];
                itemFields[(int) ItemFieldNames.AssessmentType] = testInformation[ItemFieldNames.AssessmentType];
                itemFields[(int) ItemFieldNames.AssessmentSubtype] = testInformation[ItemFieldNames.AssessmentSubtype];
                itemFields[(int) ItemFieldNames.AcademicYear] =
                    !string.IsNullOrEmpty(testInformation[ItemFieldNames.AcademicYear])
                        ? testInformation[ItemFieldNames.AcademicYear].Split('-').LastOrDefault()
                        : string.Empty;
                
                var itemId =
                    testItem.ChildNodeWithName("identifier").ValueForAttribute("uniqueid");
                itemFields[(int) ItemFieldNames.FullItemKey] = itemId;
                itemFields[(int) ItemFieldNames.ItemId] = itemId.Split('-').Last();
                itemFields[(int) ItemFieldNames.BankKey] = itemId.Split('-').First();
                itemFields[(int) ItemFieldNames.Filename] = testItem.ValueForAttribute("filename");
                itemFields[(int) ItemFieldNames.Version] =
                    testItem.ChildNodeWithName("identifier").ValueForAttribute("version");
                itemFields[(int) ItemFieldNames.ItemType] = testItem.ValueForAttribute("itemtype");
                itemFields[(int) ItemFieldNames.PassageRef] =
                    FormatHelper.Strip200(testItem.ChildNodeWithName("passageref")?.ValueForAttribute("passageref"));

                // Process PoolProperties
                var glossary = new List<string>();
                foreach (var poolProperty in testItem.ChildNodesWithName("poolproperty"))
                {
                    var ppProperty = poolProperty.ValueForAttribute("property").Trim();
                    var ppValue = poolProperty.ValueForAttribute("value").Trim();
                    if (string.IsNullOrEmpty(ppProperty) || string.IsNullOrEmpty(ppValue))
                    {
                        continue;
                    }
                    // Special case for Braille language
                    int fieldIndex;
                    if (ppProperty.Equals("Language", StringComparison.Ordinal) &&
                        ppValue.Equals("ENU-Braille", StringComparison.Ordinal))
                    {
                        itemFields[(int) ItemFieldNames.LanguageBraille] = ppValue;
                    }
                    // Special case for Spanish language
                    else if (ppProperty.Equals("Language", StringComparison.Ordinal) &&
                             ppValue.Equals("ESN", StringComparison.Ordinal))
                    {
                        itemFields[(int) ItemFieldNames.Spanish] = "Y";
                    }
                    // Special case for Spanish language
                    else if (ppProperty.Equals("Spanish Translation", StringComparison.Ordinal))
                    {
                        itemFields[(int) ItemFieldNames.Spanish] = ppValue;
                    }
                    // Special case for Glossary
                    else if (ppProperty.Equals("Glossary", StringComparison.Ordinal))
                    {
                        glossary.Add(ppValue);
                    }
                    else if (ppProperty.Equals("Allow Calculator", StringComparison.OrdinalIgnoreCase))
                    {
                        itemFields[(int) ItemFieldNames.AllowCalculator] = ppValue;
                    }
                    else if (sPoolPropertyMapping.TryGetValue(ppProperty, out fieldIndex) && fieldIndex != 0)
                    {
                        if (!string.IsNullOrEmpty(itemFields[fieldIndex]))
                        {
                            ReportingUtility.ReportError(testInformation[ItemFieldNames.AssessmentId],
                                testSpecificationProcessor.PackageType,
                                $"testspecification/{testSpecificationProcessor.PackageType.ToString().ToLower()}/itempool/testitem/poolproperty",
                                ErrorSeverity.Degraded, itemId, poolProperty.Navigator.OuterXml,
                                "'{0}={1}' Multiple values for pool property", ppProperty, ppValue);
                        }
                        itemFields[fieldIndex] = ppValue;
                    }
                }
                glossary.Sort();
                itemFields[(int) ItemFieldNames.Glossary] = string.Join(";", glossary);

                var itemScoreDimension = testItem.ChildNodeWithName("itemscoredimension");
                if (itemScoreDimension != null)
                {
                    itemFields[(int) ItemFieldNames.MeasurementModel] =
                        itemScoreDimension.ValueForAttribute("measurementmodel");
                    itemFields[(int) ItemFieldNames.Weight] =
                        FormatHelper.FormatDouble(itemScoreDimension.ValueForAttribute("weight"));
                    itemFields[(int) ItemFieldNames.ScorePoints] = itemScoreDimension.ValueForAttribute("scorepoints");
                    itemFields[(int) ItemFieldNames.a] =
                        FormatHelper.FormatDouble(testItem.Navigator.Eval(sXp_Parameter1));
                    itemFields[(int) ItemFieldNames.b0_b] =
                        FormatHelper.FormatDouble(testItem.Navigator.Eval(sXp_Parameter2));
                    itemFields[(int) ItemFieldNames.b1_c] =
                        FormatHelper.FormatDouble(testItem.Navigator.Eval(sXp_Parameter3));
                    itemFields[(int) ItemFieldNames.b2] =
                        FormatHelper.FormatDouble(testItem.Navigator.Eval(sXp_Parameter4));
                    itemFields[(int) ItemFieldNames.b3] =
                        FormatHelper.FormatDouble(testItem.Navigator.Eval(sXp_Parameter5));
                    var avg_b = MathHelper.CalculateAverageB(itemFields[(int) ItemFieldNames.MeasurementModel],
                        itemFields[(int) ItemFieldNames.a], itemFields[(int) ItemFieldNames.b0_b],
                        itemFields[(int) ItemFieldNames.b1_c], itemFields[(int) ItemFieldNames.b2],
                        itemFields[(int) ItemFieldNames.b3], itemFields[(int) ItemFieldNames.ScorePoints]);
                    if (!avg_b.Errors.Any())
                    {
                        itemFields[(int) ItemFieldNames.avg_b] = avg_b.Value;
                    }
                    else
                    {
                        avg_b.Errors.ToList().ForEach(x =>
                            ReportingUtility.ReportError(testInformation[ItemFieldNames.AssessmentId],
                                testSpecificationProcessor.PackageType,
                                $"testspecification/{testSpecificationProcessor.PackageType.ToString().ToLower()}/itempool/testitem/itemscoredimension",
                                ErrorSeverity.Degraded, itemId, itemScoreDimension.Navigator.OuterXml, x)
                        );
                    }
                }

                // bprefs
                var bpRefProcessors = testItem.ChildNodesWithName("bpref").ToList();
                for (var i = 0; i < bpRefProcessors.Count(); i++)
                {
                    var bpRef = bpRefProcessors[i].ValueForAttribute("bpref");
                    if (i < MaxBpRefs)
                    {
                        itemFields[(int) ItemFieldNames.bpref1 + i++] = bpRef;
                    }

                    // Attempt to parse the bpref as an SBAC standard
                    // See http://www.smarterapp.org/documents/InterpretingSmarterBalancedStandardIDs.html
                    // A proper Math standard ID should be in this format: SBAC-MA-v6:1|P|TS04|D-6
                    // However, the bpref form substitutes "SBAC-" for "SBAC-MA-v6:"
                    // A proper ELA standard ID should be in this format: SBAC-ELA-v1:3-L|4-6|6.SL.2
                    // However, the bpref form substitutes "SBAC-" FOR "SBAC-ELA-v1:" and it drops the
                    // last segment which is the common core state standard.
                    if (testInformation[ItemFieldNames.AssessmentSubject].Equals("Math",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        var match = s_Rx_BprefMath.Match(bpRef);
                        if (!match.Success)
                        {
                            continue;
                        }
                        itemFields[(int) ItemFieldNames.Standard] = string.Concat(c_MathStdPrefix,
                            match.Value.Substring(5));
                        itemFields[(int) ItemFieldNames.Claim] = match.Groups[1].Value;
                        itemFields[(int) ItemFieldNames.Target] = match.Groups[2].Value;
                    }
                    else if (testInformation[ItemFieldNames.AssessmentSubject].Equals("ELA",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        var match = s_Rx_BprefEla.Match(bpRef);
                        if (!match.Success)
                        {
                            continue;
                        }
                        itemFields[(int) ItemFieldNames.Standard] = string.Concat(c_ElaStdPrefix,
                            match.Value.Substring(5));
                        itemFields[(int) ItemFieldNames.Claim] = match.Groups[1].Value + "\t";
                        // Adding tab character prevents Excel from treating these as dates.
                        itemFields[(int) ItemFieldNames.Target] = match.Groups[2].Value + "\t";
                    }
                }

                if (ReportingUtility.CrossProcessor != null &&
                    ReportingUtility.CrossProcessor.ItemContentPackage != null)
                {
                    var contentItem = ReportingUtility.CrossProcessor.ItemContentPackage.FirstOrDefault(
                        x => x.ContainsKey("ItemId") &&
                             x["ItemId"].Equals(itemFields[(int) ItemFieldNames.ItemId],
                                 StringComparison.OrdinalIgnoreCase));

                    itemFields[(int) ItemFieldNames.CommonCore] = contentItem["CommonCore"];
                    itemFields[(int) ItemFieldNames.ClaimContentTarget] = contentItem["ClaimContentTarget"];
                    itemFields[(int) ItemFieldNames.SecondaryCommonCore] = contentItem["SecondaryCommonCore"];
                    itemFields[(int) ItemFieldNames.SecondaryClaimContentTarget] =
                        contentItem["SecondaryClaimContentTarget"];
                }
                GroupItemInfo gii;
                if (indexGroupItemInfo.TryGetValue(itemId.Split('-').Last(), out gii))
                {
                    itemFields[(int) ItemFieldNames.IsFieldTest] = gii.IsFieldTest;
                    itemFields[(int) ItemFieldNames.IsActive] = gii.IsActive;
                    itemFields[(int) ItemFieldNames.ResponseRequired] = gii.ResponseRequired;
                    itemFields[(int) ItemFieldNames.AdminRequired] = gii.AdminRequired;
                    itemFields[(int) ItemFieldNames.FormPosition] = gii.FormPosition;
                }
                else
                {
                    itemFields[(int) ItemFieldNames.IsFieldTest] = string.Empty;
                    itemFields[(int) ItemFieldNames.IsActive] = string.Empty;
                    itemFields[(int) ItemFieldNames.ResponseRequired] = string.Empty;
                    itemFields[(int) ItemFieldNames.AdminRequired] = string.Empty;
                    itemFields[(int) ItemFieldNames.FormPosition] = string.Empty;
                }

                var j = 0;
                foreach (var p in performanceLevels)
                {
                    itemFields[ItemFieldNamesCount - (3 * performanceLevels.Count) + j++] = p.PerfLevel;
                    itemFields[ItemFieldNamesCount - (3 * performanceLevels.Count) + j++] = p.ScaledLow;
                    itemFields[ItemFieldNamesCount - (3 * performanceLevels.Count) + j++] = p.ScaledHigh;
                }

                var item =
                    testSpecificationProcessor.ChildNodeWithName(testSpecificationProcessor.PackageType.ToString())
                        .ChildNodeWithName("itempool")
                        .ChildNodesWithName("testitem")
                        .FirstOrDefault(
                            x =>
                                x.ChildNodeWithName("identifier")
                                    .ValueForAttribute("uniqueid")
                                    .Split('-')
                                    .Last()
                                    .Equals(itemFields[(int) ItemFieldNames.ItemId]));
                itemFields[(int) ItemFieldNames.MathematicalPractice] =
                    string.IsNullOrEmpty(item.ValueForAttribute("MathematicalPractice"))
                        ? string.Empty
                        : item.ValueForAttribute("MathematicalPractice");

                itemFields[(int) ItemFieldNames.MaxPoints] = string.IsNullOrEmpty(item.ValueForAttribute("MaxPoints"))
                    ? string.Empty
                    : item.ValueForAttribute("MaxPoints");

                // We're using the backup property from the content package because the item didn't specify
                if (string.IsNullOrEmpty(itemFields[(int) ItemFieldNames.AllowCalculator]))
                {
                    itemFields[(int) ItemFieldNames.AllowCalculator] =
                        string.IsNullOrEmpty(item.ValueForAttribute("AllowCalculator"))
                            ? string.Empty
                            : item.ValueForAttribute("AllowCalculator");
                }

                if (itemFields[(int) ItemFieldNames.AssessmentSubject].Equals("MATH", StringComparison.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrEmpty(itemFields[(int) ItemFieldNames.AllowCalculator]))
                    {
                        ReportingUtility.ReportError(testInformation[ItemFieldNames.AssessmentId],
                            testSpecificationProcessor.PackageType,
                            $"testspecification/{testSpecificationProcessor.PackageType}/itempool/testitem",
                            ErrorSeverity.Degraded, item.ChildNodeWithName("identifier").ValueForAttribute("uniqueid"),
                            item.Navigator.OuterXml,
                            $"Item {item.ChildNodeWithName("identifier").ValueForAttribute("uniqueid")} is a MATH item, but does not have an AllowCalculator value");
                    }
                }

                // Write one line to the CSV
                var items = itemFields.ToList();
                for (var i = 0; i < items.Count; i++)
                {
                    if (items[i] == null)
                    {
                        items[i] = string.Empty;
                    }
                }
                resultList.Add(items);
            }

            return resultList;
        }

        private static CrossPackageValidationError GenerateItemError(string message, string id, Processor processor,
            string key)
        {
            return new CrossPackageValidationError
            {
                ErrorSeverity = ErrorSeverity.Severe,
                GeneratedMessage = message,
                ItemId = id,
                Key = "ItemId",
                Location = $"testspecification/{processor.PackageType.ToString().ToLower()}/itempool/testitem",
                Value = processor.Navigator.OuterXml,
                PrimarySource = $"{key} - {processor.PackageType}",
                SecondarySource = "Item Content Package",
                AssessmentId = key
            };
        }
    }
}