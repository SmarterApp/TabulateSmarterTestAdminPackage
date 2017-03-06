using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.XPath;
using ProcessSmarterTestPackage.Processors.Common;
using SmarterTestPackage.Common.Data;
using SmarterTestPackage.Common.Extensions;
using TabulateSmarterTestPackage.Utilities;
using TabulateSmarterTestPackage.Utilities.Data;
using ValidateSmarterTestPackage.RestrictedValues.Enums;
using ValidateSmarterTestPackage.RestrictedValues.RestrictedList;

namespace TabulateSmarterTestPackage.Tabulators
{
    public class ItemTabulator
    {
        private const int MaxBpRefs = 7;
        private const string c_MathStdPrefix = "SBAC-MA-v6:";
        private const string c_ElaStdPrefix = "SBAC-ELA-v1:";

        private static readonly int ItemFieldNamesCount = Enum.GetNames(typeof(ItemFieldNames)).Length;

        // Item Selector
        // /testspecification/administration/itempool/testitem
        private static readonly XPathExpression sXp_Item = XPathExpression.Compile("/testspecification//testitem");

        // Item Info
        private static readonly XPathExpression sXp_ItemId = XPathExpression.Compile("identifier/@uniqueid");
        private static readonly XPathExpression sXp_Filename = XPathExpression.Compile("@filename");
        private static readonly XPathExpression sXp_Version = XPathExpression.Compile("identifier/@version");
        private static readonly XPathExpression sXp_ItemType = XPathExpression.Compile("@itemtype");
        private static readonly XPathExpression sXp_PassageRef = XPathExpression.Compile("passageref");

        private static readonly XPathExpression sXp_MeasurementModel =
            XPathExpression.Compile("itemscoredimension/@measurementmodel");

        private static readonly XPathExpression sXp_Weight = XPathExpression.Compile("itemscoredimension/@weight");

        private static readonly XPathExpression sXp_ScorePoints =
            XPathExpression.Compile("itemscoredimension/@scorepoints");

        private static readonly XPathExpression sXp_ParameterName =
            XPathExpression.Compile("itemscoredimension/itemscoreparameter/@measurementparameter");

        private static readonly XPathExpression sXp_Parameter1 =
            XPathExpression.Compile("itemscoredimension/itemscoreparameter[@measurementparameter='a']/@value");

        private static readonly XPathExpression sXp_Parameter2 =
            XPathExpression.Compile(
                "itemscoredimension/itemscoreparameter[@measurementparameter='a' or @measurementparameter='b0']/@value");

        private static readonly XPathExpression sXp_Parameter3 =
            XPathExpression.Compile(
                "itemscoredimension/itemscoreparameter[@measurementparameter='a' or @measurementparameter='b1']/@value");

        private static readonly XPathExpression sXp_Parameter4 =
            XPathExpression.Compile("itemscoredimension/itemscoreparameter[@measurementparameter='b2']/@value");

        private static readonly XPathExpression sXp_Parameter5 =
            XPathExpression.Compile("itemscoredimension/itemscoreparameter[@measurementparameter='b3']/@value");

        private static readonly XPathExpression sXp_Bpref = XPathExpression.Compile("bpref");

        // Pool Properties
        private static readonly XPathExpression sXp_PoolProperty = XPathExpression.Compile("poolproperty");
        private static readonly XPathExpression sXp_PPProperty = XPathExpression.Compile("@property");
        private static readonly XPathExpression sXp_PPValue = XPathExpression.Compile("@value");

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
                {"Appropriate for Hearing Impaired", (int) ItemFieldNames.HearingImpaired},
                {"ASL", (int) ItemFieldNames.ASL},
                {"Braille", (int) ItemFieldNames.Braille},
                {"Depth of Knowledge", (int) ItemFieldNames.DOK},
                {"Grade", (int) ItemFieldNames.Grade},
                {"Language", (int) ItemFieldNames.Language},
                {"Scoring Engine", (int) ItemFieldNames.ScoringEngine},
                {"Spanish Translation", (int) ItemFieldNames.Spanish},
                {"Passage Length", (int) ItemFieldNames.PassageLength},
                {"TDSPoolFilter", (int) ItemFieldNames.TDSPoolFilter},
                {"Calculator", (int) ItemFieldNames.Calculator},
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
            {
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
                        if (!string.Equals(gii.IsFieldTest, isFieldTest, StringComparison.Ordinal))
                        {
                            ReportingUtility.ReportError(testInformation[ItemFieldNames.AssessmentName],
                                testSpecificationProcessor.PackageType,
                                node.NamespaceURI, ErrorSeverity.Degraded, itemId,
                                "Conflicting isfieldtest info: '{0}' <> '{1}'", isFieldTest, gii.IsFieldTest);
                        }
                        if (!string.Equals(gii.IsActive, isActive, StringComparison.Ordinal))
                        {
                            ReportingUtility.ReportError(testInformation[ItemFieldNames.AssessmentName],
                                testSpecificationProcessor.PackageType,
                                node.NamespaceURI, ErrorSeverity.Degraded, itemId,
                                "Conflicting isactive info: '{0}' <> '{1}'", isActive, gii.IsActive);
                        }
                        if (!string.Equals(gii.ResponseRequired, responseRequired, StringComparison.Ordinal))
                        {
                            ReportingUtility.ReportError(testInformation[ItemFieldNames.AssessmentName],
                                testSpecificationProcessor.PackageType,
                                node.NamespaceURI, ErrorSeverity.Degraded, itemId,
                                "Conflicting responserequired info: '{0}' <> '{1}'", responseRequired,
                                gii.ResponseRequired);
                        }
                        if (!string.Equals(gii.AdminRequired, adminRequired, StringComparison.Ordinal))
                        {
                            ReportingUtility.ReportError(testInformation[ItemFieldNames.AssessmentName],
                                testSpecificationProcessor.PackageType,
                                node.NamespaceURI, ErrorSeverity.Degraded, itemId,
                                "Conflicting adminrequired info: '{0}' <> '{1}'", adminRequired, gii.AdminRequired);
                        }
                        if (!string.Equals(gii.FormPosition, formPosition, StringComparison.Ordinal))
                        {
                            ReportingUtility.ReportError(testInformation[ItemFieldNames.AssessmentName],
                                testSpecificationProcessor.PackageType,
                                node.NamespaceURI, ErrorSeverity.Degraded, itemId,
                                "Conflicting formposition info: '{0} <> '{1}'", formPosition, gii.FormPosition);
                        }
                    }
                    else
                    {
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

            // Report the item fields
            var itemCount = 0;

            var nodes = navigator.Select(sXp_Item);
            while (nodes.MoveNext())
            {
                ++itemCount;
                var node = nodes.Current;

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
                itemFields[(int) ItemFieldNames.AcademicYear] = testInformation[ItemFieldNames.AcademicYear];

                var itemId = FormatHelper.Strip200(node.Eval(sXp_ItemId));
                itemFields[(int) ItemFieldNames.ItemId] = itemId.Split('-').Last();
                itemFields[(int) ItemFieldNames.BankKey] = itemId.Split('-').First();
                itemFields[(int) ItemFieldNames.Filename] = node.Eval(sXp_Filename);
                itemFields[(int) ItemFieldNames.Version] = node.Eval(sXp_Version);
                itemFields[(int) ItemFieldNames.ItemType] = node.Eval(sXp_ItemType);
                itemFields[(int) ItemFieldNames.PassageRef] = FormatHelper.Strip200(node.Eval(sXp_PassageRef));

                // Process PoolProperties
                var glossary = new List<string>();
                var ppNodes = node.Select(sXp_PoolProperty);
                while (ppNodes.MoveNext())
                {
                    var ppNode = ppNodes.Current;
                    var ppProperty = ppNode.Eval(sXp_PPProperty).Trim();
                    var ppValue = ppNode.Eval(sXp_PPValue).Trim();
                    if (!string.IsNullOrEmpty(ppProperty) && !string.IsNullOrEmpty(ppValue))
                    {
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

                        else if (sPoolPropertyMapping.TryGetValue(ppProperty, out fieldIndex))
                        {
                            if (fieldIndex != 0)
                            {
                                if (!string.IsNullOrEmpty(itemFields[fieldIndex]))
                                {
                                    ReportingUtility.ReportError(testInformation[ItemFieldNames.AssessmentId],
                                        testSpecificationProcessor.PackageType,
                                        ppNode.Name,
                                        ErrorSeverity.Degraded, itemId,
                                        "'{0}={1}' Multiple values for pool property", ppProperty, ppValue);
                                }
                                itemFields[fieldIndex] = ppValue;
                            }
                        }
                        else
                        {
                            ReportingUtility.ReportError(testInformation[ItemFieldNames.AssessmentId],
                                testSpecificationProcessor.PackageType,
                                ppNode.Name, ErrorSeverity.Degraded,
                                itemId, "'{0}={1}' Unrecognized Pool Property", ppProperty, ppValue);
                        }
                    }
                }
                glossary.Sort();
                itemFields[(int) ItemFieldNames.Glossary] = string.Join(";", glossary);

                itemFields[(int) ItemFieldNames.MeasurementModel] = node.Eval(sXp_MeasurementModel);
                itemFields[(int) ItemFieldNames.Weight] = FormatHelper.FormatDouble(node.Eval(sXp_Weight));
                itemFields[(int) ItemFieldNames.ScorePoints] = node.Eval(sXp_ScorePoints);
                itemFields[(int) ItemFieldNames.a] = FormatHelper.FormatDouble(node.Eval(sXp_Parameter1));
                itemFields[(int) ItemFieldNames.b0_b] = FormatHelper.FormatDouble(node.Eval(sXp_Parameter2));
                itemFields[(int) ItemFieldNames.b1_c] = FormatHelper.FormatDouble(node.Eval(sXp_Parameter3));
                itemFields[(int) ItemFieldNames.b2] = FormatHelper.FormatDouble(node.Eval(sXp_Parameter4));
                itemFields[(int) ItemFieldNames.b3] = FormatHelper.FormatDouble(node.Eval(sXp_Parameter5));

                // Check known measurement model
                if (
                    !RestrictedList.RestrictedLists[RestrictedListItems.MeasurementModel]
                        .Contains(itemFields[(int) ItemFieldNames.MeasurementModel]))
                {
                    ReportingUtility.ReportError(testInformation[ItemFieldNames.AssessmentId],
                        testSpecificationProcessor.PackageType, node.Name,
                        ErrorSeverity.Benign, itemId,
                        "'{0}' Unrecognized Measurement Model", itemFields[(int) ItemFieldNames.MeasurementModel]);
                }

                // Check known parameters
                var pnNodes = node.Select(sXp_ParameterName);
                while (pnNodes.MoveNext())
                {
                    var name = pnNodes.Current.Value;
                    if (!RestrictedList.RestrictedLists[RestrictedListItems.MeasurementParameter].Contains(name))
                    {
                        ReportingUtility.ReportError(testInformation[ItemFieldNames.AssessmentId],
                            testSpecificationProcessor.PackageType,
                            pnNodes.Current.Name, ErrorSeverity.Benign,
                            itemId, "'{0}' Unrecognized Measurement Parameter", name);
                    }
                }

                // bprefs
                var bpIndex = 0;
                var bpNodes = node.Select(sXp_Bpref);
                while (bpNodes.MoveNext())
                {
                    var bpref = bpNodes.Current.Value;
                    if (bpIndex >= MaxBpRefs)
                    {
                        ReportingUtility.ReportError(testInformation[ItemFieldNames.AssessmentId],
                            testSpecificationProcessor.PackageType,
                            bpNodes.Current.Name, ErrorSeverity.Benign,
                            itemId, "More than {0} bpref nodes", MaxBpRefs);
                    }
                    else
                    {
                        itemFields[(int) ItemFieldNames.bpref1 + bpIndex++] = bpref;
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
                        var match = s_Rx_BprefMath.Match(bpref);
                        if (match.Success)
                        {
                            itemFields[(int) ItemFieldNames.Standard] = string.Concat(c_MathStdPrefix,
                                match.Value.Substring(5));
                            itemFields[(int) ItemFieldNames.Claim] = match.Groups[1].Value;
                            itemFields[(int) ItemFieldNames.Target] = match.Groups[2].Value;
                        }
                    }
                    else if (testInformation[ItemFieldNames.AssessmentSubject].Equals("ELA",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        var match = s_Rx_BprefEla.Match(bpref);
                        if (match.Success)
                        {
                            itemFields[(int) ItemFieldNames.Standard] = string.Concat(c_ElaStdPrefix,
                                match.Value.Substring(5));
                            itemFields[(int) ItemFieldNames.Claim] = match.Groups[1].Value + "\t";
                            // Adding tab character prevents Excel from treating these as dates.
                            itemFields[(int) ItemFieldNames.Target] = match.Groups[2].Value + "\t";
                        }
                    }
                }

                if (ReportingUtility.CrossProcessor != null &&
                    ReportingUtility.CrossProcessor.ItemContentPackage != null)
                {
                    var contentItem = ReportingUtility.CrossProcessor.ItemContentPackage.FirstOrDefault(
                        x => x.ContainsKey("ItemId") &&
                             x["ItemId"].Equals(itemFields[(int) ItemFieldNames.ItemId],
                                 StringComparison.OrdinalIgnoreCase));


                    if (itemFields[(int) ItemFieldNames.Standard] == null)
                    {
                        if (contentItem != null &&
                            !string.IsNullOrEmpty(contentItem["Standard"].Replace("\"", string.Empty).Trim()))
                        {
                            itemFields[(int) ItemFieldNames.Standard] = contentItem["Standard"];
                        }
                        else
                        {
                            ReportingUtility.CrossProcessor.Errors[testSpecificationProcessor.GetUniqueId()].Add(
                                GenerateItemError(
                                    "[Item standard does not exist in both test package and content package]", itemId,
                                    testSpecificationProcessor, testSpecificationProcessor.GetUniqueId()));
                        }
                    }
                    if (itemFields[(int) ItemFieldNames.Claim] == null)
                    {
                        if (contentItem != null &&
                            !string.IsNullOrEmpty(contentItem["Claim"].Replace("\"", string.Empty).Trim()))
                        {
                            itemFields[(int) ItemFieldNames.Claim] = contentItem["Claim"];
                        }
                        else
                        {
                            ReportingUtility.CrossProcessor.Errors[testSpecificationProcessor.GetUniqueId()].Add(
                                GenerateItemError(
                                    "[Item claim does not exist in both test package and content package]", itemId,
                                    testSpecificationProcessor, testSpecificationProcessor.GetUniqueId()));
                        }
                    }
                    if (itemFields[(int) ItemFieldNames.Target] == null)
                    {
                        if (contentItem != null &&
                            !string.IsNullOrEmpty(contentItem["Target"].Replace("\"", string.Empty).Trim()))
                        {
                            itemFields[(int) ItemFieldNames.Target] = contentItem["Target"];
                        }
                        else
                        {
                            ReportingUtility.CrossProcessor.Errors[testSpecificationProcessor.GetUniqueId()].Add(
                                GenerateItemError(
                                    "[Item target does not exist in both test package and content package]", itemId,
                                    testSpecificationProcessor, testSpecificationProcessor.GetUniqueId()));
                        }
                    }
                }

                GroupItemInfo gii;
                if (indexGroupItemInfo.TryGetValue(itemId, out gii))
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
                    itemFields[ItemFieldNamesCount + j++] = p.PerfLevel;
                    itemFields[ItemFieldNamesCount + j++] = p.ScaledLow;
                    itemFields[ItemFieldNamesCount + j++] = p.ScaledHigh;
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
                Location = "Item Cross-Tabulation (Item Content Package)",
                Path = $"testspecification/{processor.PackageType.ToString().ToLower()}/itempool/testitem",
                PrimarySource = $"{key} - {processor.PackageType}",
                SecondarySource = "Item Content Package",
                TestName = key
            };
        }
    }
}