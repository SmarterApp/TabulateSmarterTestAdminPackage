using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.XPath;
using NLog;
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
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

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
        // /testspecification/administration/testform/formpartition/itemgroup/groupitem <- now handled by absolute positioning
        // /testspecification/administration/adminsegment/segmentpool/itemgroup/groupitem
        private static readonly XPathExpression sXp_GroupItem =
            XPathExpression.Compile("/testspecification/administration/adminsegment/segmentpool/itemgroup/groupitem");

        // Parse bpref entries to standard, claim, target
        private static readonly Regex s_Rx_BprefMath =
            new Regex(@"^SBAC(_PT)?-(\d)\|[A-Z]{1,4}\|[0-9A-Z]{1,4}\|([A-Z]+(?:-\d+)?)$");

        private static readonly Regex s_Rx_BprefEla =
            new Regex(@"^SBAC(_PT)?-(\d-[A-Z]{1,2})\|(\d{1,2}-\d{1,2})(?:\|[A-Za-z0-9\-\.]+)?$");

        private readonly Dictionary<string, int> sPoolPropertyMapping;

        public static IList<ProcessingError> ItemTabulationErrors { get; set; } = new List<ProcessingError>();

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

            Logger.Info($"Tabulating {testSpecificationProcessor.GetUniqueId()}");
            var indexGroupItemInfo = new Dictionary<string, GroupItemInfo>();

            var adminSegments = navigator.Select("//adminsegment");
            // If there are no admin segments, this must be a fixed form assessment
            // 2018-08-08: fixed form assessments do have admin segments. Not sure why the original comment was placed here, nor the check for an adminSegment element
            //if (adminSegments == null || !adminSegments.OfType<XPathNavigator>().Any())
            if (adminSegments != null || adminSegments.OfType<XPathNavigator>().Any())
            {
                var testFormPartitions = new List<XPathNavigator>();
                // Convoluted way of checking whether the test form identifier ends with Default-ENU
                testFormPartitions =
                    navigator.Select(
                            $"/testspecification/{testSpecificationProcessor.PackageType.ToString().ToLower()}/testform[./identifier[substring(@uniqueid, string-length(@uniqueid) - string-length(':Default-ENU') +1) = ':Default-ENU']]/formpartition")
                        .OfType<XPathNavigator>()
                        .ToList();

                // We want a Default-ENU, failing to find that form, we'll take the first one (This is an error condition)
                if (!testFormPartitions.Any())
                {
                    Logger.Error($"{testSpecificationProcessor.GetUniqueId()} - Missing Default-ENU test form for fixed form assessment!");
                    ItemTabulationErrors.Add(new ValidationError
                    {
                        AssessmentId = testSpecificationProcessor.GetUniqueId(),
                        ErrorSeverity = ErrorSeverity.Severe,
                        GeneratedMessage = "Missing Default-ENU test form for fixed form assessment",
                        Key = "testform",
                        Location = $"testspecification/{testSpecificationProcessor.PackageType.ToString().ToLower()}/testform",
                        PackageType = testSpecificationProcessor.PackageType
                    });
                    var testForms =
                        navigator.Select(
                                $"/testspecification/{testSpecificationProcessor.PackageType.ToString().ToLower()}/testform")
                            .OfType<XPathNavigator>()
                            .FirstOrDefault();
                    testFormPartitions = testForms?.Select("./formpartition")
                        .OfType<XPathNavigator>()
                        .ToList();
                }

                // We will do this section only for fixed-form assessments. 
                // Non -fixed-forms won't have a test form - they will have admin segments instead
                if (testFormPartitions.Any())
                {
                    // Get all the partition IDs from within the selected form
                    var partitionIds =
                        testFormPartitions.SelectMany(x => x.Select("./identifier/@uniqueid").OfType<XPathNavigator>())
                            .Select(x => x.InnerXml);
                    // Get the group items (with relative positions)
                    var groupItems =
                        partitionIds.SelectMany(
                            x =>
                                navigator.Select(
                                        $"/testspecification/{testSpecificationProcessor.PackageType.ToString().ToLower()}/testform/formpartition[./identifier[@uniqueid='{x}']]/itemgroup/groupitem")
                                    .OfType<XPathNavigator>()
                                    .ToList().Select(y => new GroupItemInfo
                                    {
                                        ItemId = FormatHelper.StripItemBankPrefix(y.GetAttribute("itemid", string.Empty)),
                                        IsFieldTest = y.GetAttribute("isfieldtest", string.Empty),
                                        IsActive = y.GetAttribute("isactive", string.Empty),
                                        ResponseRequired = y.GetAttribute("responserequired", string.Empty),
                                        AdminRequired = y.GetAttribute("adminrequired", string.Empty),
                                        FormPosition = y.GetAttribute("formposition", string.Empty)
                                    })).ToList();
                    // Zip the group items against an autonumbering enumerable to get the absolute form position (required for RDW)
                    indexGroupItemInfo =
                        groupItems.Zip(Enumerable.Range(1, groupItems.Count()),
                            (groupItem, absolutePosition) => new GroupItemInfo
                            {
                                ItemId = groupItem.ItemId,
                                IsFieldTest = groupItem.IsFieldTest,
                                IsActive = groupItem.IsActive,
                                ResponseRequired = groupItem.ResponseRequired,
                                AdminRequired = groupItem.AdminRequired,
                                FormPosition = absolutePosition.ToString()
                            }).ToDictionary(x => x.ItemId, x => x);
                }
                else
                {
                    Logger.Error($"{testSpecificationProcessor.GetUniqueId()} - " +
                                "Unable to determine whether this is a fixed form or adaptive assessment. " +
                                "Missing both admin segments and test forms");
                    ItemTabulationErrors.Add(new ValidationError
                    {
                        AssessmentId = testSpecificationProcessor.GetUniqueId(),
                        ErrorSeverity = ErrorSeverity.Severe,
                        GeneratedMessage = "Unable to determine whether this is a fixed form or adaptive assessment. " +
                                "Missing both admin segments and test forms",
                        Key = "testform",
                        Location = $"testspecification/{testSpecificationProcessor.PackageType.ToString().ToLower()}/testform",
                        PackageType = testSpecificationProcessor.PackageType
                    });
                }

                // This is to deal with any GII that may be present in the admin segment nodes 
                var groupItemNodes = navigator.Select(sXp_GroupItem);
                while (groupItemNodes.MoveNext())
                {
                    var node = groupItemNodes.Current;
                    var itemId = FormatHelper.StripItemBankPrefix(node.GetAttribute("itemid", string.Empty));
                    var isFieldTest = node.GetAttribute("isfieldtest", string.Empty);
                    var isActive = node.GetAttribute("isactive", string.Empty);
                    var responseRequired = node.GetAttribute("responserequired", string.Empty);
                    var adminRequired = node.GetAttribute("adminrequired", string.Empty);

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
                        FormPosition = string.Empty
                        // This information should be provided by the test form and will cause confusion if it is pulled from here
                    };
                    indexGroupItemInfo.Add(itemId, gii);
                }
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

                var itemId = testItem.ChildNodeWithName("identifier").ValueForAttribute("uniqueid");
                itemFields[(int) ItemFieldNames.FullItemKey] = itemId;
                itemFields[(int) ItemFieldNames.ItemId] = itemId.Split('-').Last();
                itemFields[(int) ItemFieldNames.BankKey] = itemId.Split('-').First();
                itemFields[(int) ItemFieldNames.Filename] = testItem.ValueForAttribute("filename");
                itemFields[(int) ItemFieldNames.Version] = testItem.ChildNodeWithName("identifier").ValueForAttribute("version");
                itemFields[(int) ItemFieldNames.ItemType] = testItem.ValueForAttribute("itemtype");
                itemFields[(int) ItemFieldNames.PassageId] = testItem.ChildNodeWithName("passageref")?.ValueForAttribute("passageref") ?? string.Empty;

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

                // For WER items there are two itemscoredimension elements where dimension="C" and dimension="D". If a WER item, handle the two itemscoredimension elements
                
                if (itemScoreDimension != null)
                {
                    if (itemFields[(int)ItemFieldNames.ItemType].Equals("WER"))
                    {
                        // find the multiples of the itemscoredimension
                        var itemScoreDimensions = testItem.ChildNodesWithName("itemscoredimension");

                        foreach(var currentScoreDimension in itemScoreDimensions)
                        {
                            if (currentScoreDimension.ValidatedAttributes["dimension"].Value.Equals("C"))
                            {
                                itemFields[(int)ItemFieldNames.MeasurementModel_1] = currentScoreDimension.ValidatedAttributes["measurementmodel"].Value.ToString();
                                itemFields[(int)ItemFieldNames.Weight_1] = FormatHelper.FormatDouble(currentScoreDimension.ValidatedAttributes["weight"].Value.ToString());
                                itemFields[(int)ItemFieldNames.ScorePoints_1] = currentScoreDimension.ValidatedAttributes["scorepoints"].Value.ToString();
                                itemFields[(int)ItemFieldNames.dimension_1] = "C";

                                var itemScoreParameters = currentScoreDimension.ChildNodesWithName("itemscoreparameter");
                                foreach(var currentItemScoreParameter in itemScoreParameters)
                                {
                                    if (currentItemScoreParameter.ValidatedAttributes["measurementparameter"].Value.Equals("a"))
                                    {
                                        itemFields[(int)ItemFieldNames.a] = FormatHelper.FormatDouble(currentItemScoreParameter.ValidatedAttributes["value"].Value.ToString()); // measureparameter = "a"
                                    }
                                    if (currentItemScoreParameter.ValidatedAttributes["measurementparameter"].Value.Equals("b") ||
                                        currentItemScoreParameter.ValidatedAttributes["measurementparameter"].Value.Equals("b0"))
                                    {
                                        itemFields[(int)ItemFieldNames.b0_b] = FormatHelper.FormatDouble(currentItemScoreParameter.ValidatedAttributes["value"].Value.ToString());
                                    }
                                    if (currentItemScoreParameter.ValidatedAttributes["measurementparameter"].Value.Equals("b1") ||
                                        currentItemScoreParameter.ValidatedAttributes["measurementparameter"].Value.Equals("c"))
                                    {
                                        itemFields[(int)ItemFieldNames.b1_c] = FormatHelper.FormatDouble(currentItemScoreParameter.ValidatedAttributes["value"].Value.ToString());
                                    }
                                    if (currentItemScoreParameter.ValidatedAttributes["measurementparameter"].Value.Equals("b2"))
                                    {
                                        itemFields[(int)ItemFieldNames.b2] = FormatHelper.FormatDouble(currentItemScoreParameter.ValidatedAttributes["value"].Value.ToString());
                                    }
                                    if (currentItemScoreParameter.ValidatedAttributes["measurementparameter"].Value.Equals("b3"))
                                    {
                                        itemFields[(int)ItemFieldNames.b3] = FormatHelper.FormatDouble(currentItemScoreParameter.ValidatedAttributes["value"].Value.ToString());
                                    }                                    
                                }
                                
                            }
                            if (currentScoreDimension.ValidatedAttributes["dimension"].Value.Equals("D"))
                            {
                                itemFields[(int)ItemFieldNames.MeasurementModel_2] = currentScoreDimension.ValidatedAttributes["measurementmodel"].Value.ToString();
                                itemFields[(int)ItemFieldNames.Weight_2] = FormatHelper.FormatDouble(currentScoreDimension.ValidatedAttributes["weight"].Value.ToString());
                                itemFields[(int)ItemFieldNames.ScorePoints_2] = currentScoreDimension.ValidatedAttributes["scorepoints"].Value.ToString();
                                itemFields[(int)ItemFieldNames.dimension_2] = "D";

                                var itemScoreParameters = currentScoreDimension.ChildNodesWithName("itemscoreparameter");
                                foreach (var currentItemScoreParameter in itemScoreParameters)
                                {
                                    if (currentItemScoreParameter.ValidatedAttributes["measurementparameter"].Value.Equals("a"))
                                    {
                                        itemFields[(int)ItemFieldNames.a_d2] = FormatHelper.FormatDouble(currentItemScoreParameter.ValidatedAttributes["value"].Value.ToString()); // measureparameter = "a"
                                    }
                                    if (currentItemScoreParameter.ValidatedAttributes["measurementparameter"].Value.Equals("b") ||
                                        currentItemScoreParameter.ValidatedAttributes["measurementparameter"].Value.Equals("b0"))
                                    {
                                        itemFields[(int)ItemFieldNames.b0_d2] = FormatHelper.FormatDouble(currentItemScoreParameter.ValidatedAttributes["value"].Value.ToString());
                                    }
                                    if (currentItemScoreParameter.ValidatedAttributes["measurementparameter"].Value.Equals("b1") ||
                                        currentItemScoreParameter.ValidatedAttributes["measurementparameter"].Value.Equals("c"))
                                    {
                                        itemFields[(int)ItemFieldNames.b1_d2] = FormatHelper.FormatDouble(currentItemScoreParameter.ValidatedAttributes["value"].Value.ToString());
                                    }
                                    if (currentItemScoreParameter.ValidatedAttributes["measurementparameter"].Value.Equals("b2"))
                                    {
                                        itemFields[(int)ItemFieldNames.b2_d2] = FormatHelper.FormatDouble(currentItemScoreParameter.ValidatedAttributes["value"].Value.ToString());
                                    }
                                    if (currentItemScoreParameter.ValidatedAttributes["measurementparameter"].Value.Equals("b3"))
                                    {
                                        itemFields[(int)ItemFieldNames.b3_d2] = FormatHelper.FormatDouble(currentItemScoreParameter.ValidatedAttributes["value"].Value.ToString());
                                    }
                                }
                            }                            
                        }
                        
                    }
                    else
                    {
                        itemFields[(int)ItemFieldNames.MeasurementModel_1] = itemScoreDimension.ValueForAttribute("measurementmodel");
                        itemFields[(int)ItemFieldNames.Weight_1] = FormatHelper.FormatDouble(itemScoreDimension.ValueForAttribute("weight"));
                        itemFields[(int)ItemFieldNames.ScorePoints_1] = itemScoreDimension.ValueForAttribute("scorepoints");
                        itemFields[(int)ItemFieldNames.a] = FormatHelper.FormatDouble(testItem.Navigator.Eval(sXp_Parameter1)); // measureparameter = "a"
                        itemFields[(int)ItemFieldNames.b0_b] = FormatHelper.FormatDouble(testItem.Navigator.Eval(sXp_Parameter2)); // measureparameter = "b0"
                        itemFields[(int)ItemFieldNames.b1_c] = FormatHelper.FormatDouble(testItem.Navigator.Eval(sXp_Parameter3)); // measureparameter = "b1"
                        itemFields[(int)ItemFieldNames.b2] = FormatHelper.FormatDouble(testItem.Navigator.Eval(sXp_Parameter4)); // measureparameter = "b2"
                        itemFields[(int)ItemFieldNames.b3] = FormatHelper.FormatDouble(testItem.Navigator.Eval(sXp_Parameter5)); // measureparameter = "b3"

                        var avg_b = MathHelper.CalculateAverageB(itemFields[(int)ItemFieldNames.MeasurementModel_1],
                            itemFields[(int)ItemFieldNames.a], itemFields[(int)ItemFieldNames.b0_b],
                            itemFields[(int)ItemFieldNames.b1_c], itemFields[(int)ItemFieldNames.b2],
                            itemFields[(int)ItemFieldNames.b3], itemFields[(int)ItemFieldNames.ScorePoints_1]);

                        if (!avg_b.Errors.Any())
                        {
                            itemFields[(int)ItemFieldNames.avg_b] = avg_b.Value;
                        }
                        else
                        {
                            avg_b.Errors.ToList().ForEach(x =>
                                ReportingUtility.ReportError(
                                    testInformation[ItemFieldNames.AssessmentId],
                                    testSpecificationProcessor.PackageType,
                                    $"testspecification/{testSpecificationProcessor.PackageType.ToString().ToLower()}/itempool/testitem/itemscoredimension",
                                    ErrorSeverity.Degraded,
                                    itemId,
                                    itemScoreDimension.Navigator.OuterXml,
                                    x)
                            );
                        }
                    }
                }

                // bprefs
                var bpRefProcessors = testItem.ChildNodesWithName("bpref").ToList();
                for (var i = 0; i < bpRefProcessors.Count(); i++)
                {
                    var bpRef = bpRefProcessors[i].ValueForAttribute("bpref");
                    if (i < MaxBpRefs)
                    {
                        itemFields[(int) ItemFieldNames.bpref1 + i] = bpRef;
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
                            match.Value.Substring(match.Value.IndexOf("-")+1)); // since the regex can handle SBAC and SBAC_PT
                        itemFields[(int) ItemFieldNames.Claim] = match.Groups[2].Value;
                        itemFields[(int) ItemFieldNames.Target] = match.Groups[3].Value;
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
                            match.Value.Substring(match.Value.IndexOf("-")+1)); // since the regex can handle SBAC and SBAC_PT
                        itemFields[(int) ItemFieldNames.Claim] = match.Groups[2].Value + "\t";
                        // Adding tab character prevents Excel from treating these as dates.
                        itemFields[(int) ItemFieldNames.Target] = match.Groups[3].Value + "\t";
                    }
                }

                if (ReportingUtility.CrossProcessor != null &&
                    ReportingUtility.CrossProcessor.ItemContentPackage != null)
                {
                    var contentItem = ReportingUtility.CrossProcessor.ItemContentPackage.FirstOrDefault(
                        x => x.ItemId.Equals(itemFields[(int) ItemFieldNames.ItemId],
                            StringComparison.OrdinalIgnoreCase));

                    itemFields[(int) ItemFieldNames.CommonCore] = contentItem?.CCSS ?? string.Empty;
                    itemFields[(int) ItemFieldNames.ClaimContentTarget] = contentItem?.ClaimContentTarget ??
                                                                          string.Empty;
                    itemFields[(int) ItemFieldNames.SecondaryCommonCore] = contentItem?.SecondaryCCSS ??
                                                                           string.Empty;
                    itemFields[(int) ItemFieldNames.SecondaryClaimContentTarget] =
                        contentItem?.SecondaryClaimContentTarget ?? string.Empty;

                    itemFields[(int)ItemFieldNames.AnswerKey] = contentItem?.AnswerKey ?? string.Empty;

                    itemFields[(int)ItemFieldNames.NumberOfAnswerOptions] = contentItem?.AnswerOptions ?? string.Empty;

                    //itemFields[(int)ItemFieldNames.PerformanceTask] = contentItem?.PerformanceTask ?? string.Empty;
                    itemFields[(int)ItemFieldNames.PtWritingType] = contentItem?.PtWritingType ?? string.Empty;
                }

                GroupItemInfo gii;
                if (indexGroupItemInfo.TryGetValue(itemId.Split('-').Last(), out gii))
                {
                    itemFields[(int) ItemFieldNames.IsFieldTest] = gii.IsFieldTest;
                    itemFields[(int) ItemFieldNames.IsActive] = gii.IsActive;
                    itemFields[(int) ItemFieldNames.ResponseRequired] = gii.ResponseRequired;
                    itemFields[(int) ItemFieldNames.AdminRequired] = gii.AdminRequired;
                    itemFields[(int) ItemFieldNames.ItemPosition] = gii.FormPosition;
                }
                else
                {
                    itemFields[(int) ItemFieldNames.IsFieldTest] = string.Empty;
                    itemFields[(int) ItemFieldNames.IsActive] = string.Empty;
                    itemFields[(int) ItemFieldNames.ResponseRequired] = string.Empty;
                    itemFields[(int) ItemFieldNames.AdminRequired] = string.Empty;
                    itemFields[(int) ItemFieldNames.ItemPosition] = string.Empty;
                }

                var j = 0;
                foreach (var p in performanceLevels)
                {
                    itemFields[ItemFieldNamesCount - 3 * performanceLevels.Count + j++] = p.PerfLevel;
                    itemFields[ItemFieldNamesCount - 3 * performanceLevels.Count + j++] = p.ScaledLow;
                    itemFields[ItemFieldNamesCount - 3 * performanceLevels.Count + j++] = p.ScaledHigh;
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