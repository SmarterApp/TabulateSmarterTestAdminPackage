using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.RestrictedValues.Enums;
using TabulateSmarterTestAdminPackage.Common.Utilities;
using TabulateSmarterTestPackage.Processors.TestSpecification;

namespace TabulateSmarterTestPackage
{
    internal class TestPackageProcessor : ITestResultProcessor
    {
        private const int MaxBpRefs = 7;
        private const string c_MathStdPrefix = "SBAC-MA-v6:";
        private const string c_ElaStdPrefix = "SBAC-ELA-v1:";

        private static readonly int ItemFieldNamesCount = Enum.GetNames(typeof(ItemFieldNames)).Length;

        private static readonly int StimFieldNamesCount = Enum.GetNames(typeof(StimFieldNames)).Length;

        // Test Info
        private static readonly XPathExpression sXp_TestName =
            XPathExpression.Compile("/testspecification/identifier/@name");

        private static readonly XPathExpression sXp_TestSubject =
            XPathExpression.Compile("/testspecification/property[@name='subject']/@value");

        private static readonly XPathExpression sXp_TestGrade =
            XPathExpression.Compile("/testspecification/property[@name='grade']/@value");

        private static readonly XPathExpression sXp_TestType =
            XPathExpression.Compile("/testspecification/property[@name='type']/@value");

        // Stim Selector
        // /testspecification/administration/itempool/passage
        private static readonly XPathExpression sXp_Stim = XPathExpression.Compile("/testspecification//passage");

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

        private static readonly Dictionary<string, int> sPoolPropertyMapping;

        static TestPackageProcessor()
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

        public TestPackageProcessor(string oFilename)
        {
            ReportingUtility.SetFileName(oFilename);
#if DEBUG
            if (File.Exists(ReportingUtility.ErrorFileName))
            {
                File.Delete(ReportingUtility.ErrorFileName);
            }
#else
            if (File.Exists(itemFilename)) throw new ApplicationException(string.Format("Output file, '{0}' already exists.", itemFilename));
            if (File.Exists(stimFilename)) throw new ApplicationException(string.Format("Output file, '{0}' already exists.", stimFilename));
            if (File.Exists(m_errFilename)) throw new ApplicationException(string.Format("Output file, '{0}' already exists.", m_errFilename));
#endif
            ReportingUtility.GetItemWriter().Write(Enum.GetNames(typeof(ItemFieldNames)));
            ReportingUtility.GetStimuliWriter().Write(Enum.GetNames(typeof(StimFieldNames)));
        }

        public PackageType ExpectedPackageType { get; set; }

        public void ProcessResult(Stream input)
        {
            var doc = new XPathDocument(input);
            var nav = doc.CreateNavigator();

            // /testspecification
            var testSpecificationProcessor = new TestSpecificationProcessor(nav.SelectSingleNode("/testspecification"),
                ExpectedPackageType);

            if (
                testSpecificationProcessor.Attributes.ValidateAttribute(testSpecificationProcessor.Navigator, "package")
                    .Any(x => !x.Value.IsValid))
            {
                Console.WriteLine("Incorrect package type assigned");
                return;
                    // If the test package is not what we expect, we should short circuit and return without processing any further
            }

            // Get the test info
            var testName = nav.Eval(sXp_TestName);
            var testSubject = nav.Eval(sXp_TestSubject);
            var testGrade = nav.Eval(sXp_TestGrade);
            var testType = nav.Eval(sXp_TestType);

            ReportingUtility.TestName = testName;
            testSpecificationProcessor.Process();

            // Index the group item info
            var indexGroupItemInfo = new Dictionary<string, GroupItemInfo>();
            {
                var nodes = nav.Select(sXp_GroupItem);
                while (nodes.MoveNext())
                {
                    var node = nodes.Current;
                    var itemId = Strip200(node.GetAttribute("itemid", string.Empty));
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
                            ReportingUtility.ReportError(testName, node.NamespaceURI, ErrorSeverity.Degraded, itemId,
                                "Conflicting isfieldtest info: '{0}' <> '{1}'", isFieldTest, gii.IsFieldTest);
                        }
                        if (!string.Equals(gii.IsActive, isActive, StringComparison.Ordinal))
                        {
                            ReportingUtility.ReportError(testName, node.NamespaceURI, ErrorSeverity.Degraded, itemId,
                                "Conflicting isactive info: '{0}' <> '{1}'", isActive, gii.IsActive);
                        }
                        if (!string.Equals(gii.ResponseRequired, responseRequired, StringComparison.Ordinal))
                        {
                            ReportingUtility.ReportError(testName, node.NamespaceURI, ErrorSeverity.Degraded, itemId,
                                "Conflicting responserequired info: '{0}' <> '{1}'", responseRequired,
                                gii.ResponseRequired);
                        }
                        if (!string.Equals(gii.AdminRequired, adminRequired, StringComparison.Ordinal))
                        {
                            ReportingUtility.ReportError(testName, node.NamespaceURI, ErrorSeverity.Degraded, itemId,
                                "Conflicting adminrequired info: '{0}' <> '{1}'", adminRequired, gii.AdminRequired);
                        }
                        if (!string.Equals(gii.FormPosition, formPosition, StringComparison.Ordinal))
                        {
                            ReportingUtility.ReportError(testName, node.NamespaceURI, ErrorSeverity.Degraded, itemId,
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
                        //Console.WriteLine(itemId);
                    }
                }
            }

            // Report the item fields
            var itemCount = 0;
            if (ReportingUtility.GetItemWriter() != null)
            {
                var nodes = nav.Select(sXp_Item);
                while (nodes.MoveNext())
                {
                    ++itemCount;
                    var node = nodes.Current;

                    // Collect the item fields
                    var itemFields = new string[ItemFieldNamesCount];
                    itemFields[(int) ItemFieldNames.TestName] = testName;
                    itemFields[(int) ItemFieldNames.TestSubject] = testSubject;
                    itemFields[(int) ItemFieldNames.TestGrade] = testGrade;
                    itemFields[(int) ItemFieldNames.TestType] = testType;
                    var itemId = Strip200(node.Eval(sXp_ItemId));
                    itemFields[(int) ItemFieldNames.ItemId] = itemId;
                    itemFields[(int) ItemFieldNames.Filename] = node.Eval(sXp_Filename);
                    itemFields[(int) ItemFieldNames.Version] = node.Eval(sXp_Version);
                    itemFields[(int) ItemFieldNames.ItemType] = node.Eval(sXp_ItemType);
                    itemFields[(int) ItemFieldNames.PassageRef] = Strip200(node.Eval(sXp_PassageRef));

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
                                        ReportingUtility.ReportError(testName, ppNode.NamespaceURI,
                                            ErrorSeverity.Degraded, itemId,
                                            "'{0}={1}' Multiple values for pool property", ppProperty, ppValue);
                                    }
                                    itemFields[fieldIndex] = ppValue;
                                }
                            }
                            else
                            {
                                ReportingUtility.ReportError(testName, ppNode.NamespaceURI, ErrorSeverity.Degraded,
                                    itemId, "'{0}={1}' Unrecognized Pool Property", ppProperty, ppValue);
                            }
                        }
                    }
                    glossary.Sort();
                    itemFields[(int) ItemFieldNames.Glossary] = string.Join(";", glossary);

                    itemFields[(int) ItemFieldNames.MeasurementModel] = node.Eval(sXp_MeasurementModel);
                    itemFields[(int) ItemFieldNames.Weight] = FormatDouble(node.Eval(sXp_Weight));
                    itemFields[(int) ItemFieldNames.ScorePoints] = node.Eval(sXp_ScorePoints);
                    itemFields[(int) ItemFieldNames.a] = FormatDouble(node.Eval(sXp_Parameter1));
                    itemFields[(int) ItemFieldNames.b0_b] = FormatDouble(node.Eval(sXp_Parameter2));
                    itemFields[(int) ItemFieldNames.b1_c] = FormatDouble(node.Eval(sXp_Parameter3));
                    itemFields[(int) ItemFieldNames.b2] = FormatDouble(node.Eval(sXp_Parameter4));
                    itemFields[(int) ItemFieldNames.b3] = FormatDouble(node.Eval(sXp_Parameter5));

                    // Check known measurement model
                    if (
                        !Enum.GetNames(typeof(MeasurementModel))
                            .Contains(itemFields[(int) ItemFieldNames.MeasurementModel]))
                    {
                        ReportingUtility.ReportError(testName, node.NamespaceURI, ErrorSeverity.Benign, itemId,
                            "'{0}' Unrecognized Measurement Model", itemFields[(int) ItemFieldNames.MeasurementModel]);
                    }

                    // Check known parameters
                    var pnNodes = node.Select(sXp_ParameterName);
                    while (pnNodes.MoveNext())
                    {
                        var name = pnNodes.Current.Value;
                        if (!Enum.GetNames(typeof(MeasurementParameter)).Contains(name))
                        {
                            ReportingUtility.ReportError(testName, pnNodes.Current.NamespaceURI, ErrorSeverity.Benign,
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
                            ReportingUtility.ReportError(testName, bpNodes.Current.NamespaceURI, ErrorSeverity.Benign,
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
                        if (testSubject.Equals("Math", StringComparison.OrdinalIgnoreCase))
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
                        else if (testSubject.Equals("ELA", StringComparison.OrdinalIgnoreCase))
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
                    if (itemFields[(int) ItemFieldNames.Standard] == null)
                    {
                        itemFields[(int) ItemFieldNames.Standard] = string.Empty;
                    }
                    if (itemFields[(int) ItemFieldNames.Claim] == null)
                    {
                        itemFields[(int) ItemFieldNames.Claim] = string.Empty;
                    }
                    if (itemFields[(int) ItemFieldNames.Target] == null)
                    {
                        itemFields[(int) ItemFieldNames.Target] = string.Empty;
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

                    // Write one line to the CSV
                    ReportingUtility.GetItemWriter().Write(itemFields);
                }
            }

            // Report the stimuli fields
            if (ReportingUtility.GetStimuliWriter() != null)
            {
                var nodes = nav.Select(sXp_Stim);
                while (nodes.MoveNext())
                {
                    var node = nodes.Current;

                    // Collect the stim fields
                    var stimFields = new string[StimFieldNamesCount];
                    stimFields[(int) StimFieldNames.TestName] = testName;
                    stimFields[(int) StimFieldNames.TestSubject] = testSubject;
                    stimFields[(int) StimFieldNames.TestGrade] = testGrade;
                    stimFields[(int) StimFieldNames.TestType] = testType;
                    stimFields[(int) StimFieldNames.StimId] = Strip200(node.Eval(sXp_ItemId));
                    stimFields[(int) StimFieldNames.Filename] = node.Eval(sXp_Filename);
                    stimFields[(int) StimFieldNames.Version] = node.Eval(sXp_Version);

                    // Write one line to the CSV
                    ReportingUtility.GetStimuliWriter().Write(stimFields);
                }
            }

            // Get the item counts
            var opitemcount =
                int.Parse(
                    nav.Eval(XPathExpression.Compile("/testspecification//bpelement[@elementtype='test']/@opitemcount")));
            var ftitemcount =
                int.Parse(
                    nav.Eval(XPathExpression.Compile("/testspecification//bpelement[@elementtype='test']/@ftitemcount")));
            var maxopitems =
                int.Parse(
                    nav.Eval(XPathExpression.Compile("/testspecification//bpelement[@elementtype='test']/@maxopitems")));
            //Console.WriteLine("  opitemcount = {0}", opitemcount);
            //Console.WriteLine("  ftitemcount = {0}", ftitemcount);
            //Console.WriteLine("  totalcount  = {0}", opitemcount + ftitemcount);
            //Console.WriteLine("  maxopitems  = {0}", maxopitems);
            //Console.WriteLine("  opitemcount/maxopitems = {0}/{1} = {2}", opitemcount, maxopitems, ((double)opitemcount) / ((double)maxopitems));
            //Debug.WriteLine("{0},{1},{2}", testName, opitemcount, maxopitems);
        } // ProcessPackage


        public void Dispose()
        {
            Dispose(true);
        }

        private static string Strip200(string val)
        {
            return val.StartsWith("200-", StringComparison.Ordinal) ? val.Substring(4) : val;
        }

        private static string FormatDouble(string val)
        {
            if (string.IsNullOrEmpty(val))
            {
                return string.Empty;
            }
            double v;
            return double.TryParse(val, out v) ? v.ToString("G", CultureInfo.InvariantCulture) : val;
        }

        ~TestPackageProcessor()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            ReportingUtility.Dispose(disposing);
            if (disposing)
            {
                GC.SuppressFinalize(this);
            }
        }

        private class GroupItemInfo
        {
            public string AdminRequired;
            public string FormPosition;
            public string IsActive;
            public string IsFieldTest;
            public string ResponseRequired;
        }
    }
}