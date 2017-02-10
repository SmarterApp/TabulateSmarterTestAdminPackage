using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.XPath;
using System.Text.RegularExpressions;
using TabulateSmarterTestAdminPackage.Common.Enums;
using TabulateSmarterTestAdminPackage.Common.Utilities;
using TabulateSmarterTestAdminPackage.Processors.Specification.TestSpecification;

namespace TabulateSmarterTestAdminPackage
{
    internal class TestPackageProcessor : ITestResultProcessor
    {
       
        static readonly int ItemFieldNamesCount = Enum.GetNames(typeof(ItemFieldNames)).Length;
        const int MaxBpRefs = 7;

        static readonly int StimFieldNamesCount = Enum.GetNames(typeof(StimFieldNames)).Length;

        private class GroupItemInfo
        {
            public string IsFieldTest;
            public string IsActive;
            public string ResponseRequired;
            public string AdminRequired;
            public string FormPosition;
        }

        // Test Info
        static XPathExpression sXp_TestName = XPathExpression.Compile("/testspecification/identifier/@name");
        static XPathExpression sXp_TestSubject = XPathExpression.Compile("/testspecification/property[@name='subject']/@value");
        static XPathExpression sXp_TestGrade = XPathExpression.Compile("/testspecification/property[@name='grade']/@value");
        static XPathExpression sXp_TestType = XPathExpression.Compile("/testspecification/property[@name='type']/@value");
        
        // Stim Selector
        // /testspecification/administration/itempool/passage
        static XPathExpression sXp_Stim = XPathExpression.Compile("/testspecification//passage");

        // Item Selector
        // /testspecification/administration/itempool/testitem
        static XPathExpression sXp_Item = XPathExpression.Compile("/testspecification//testitem");

        // Item Info
        static XPathExpression sXp_ItemId = XPathExpression.Compile("identifier/@uniqueid");
        static XPathExpression sXp_Filename = XPathExpression.Compile("@filename");
        static XPathExpression sXp_Version = XPathExpression.Compile("identifier/@version");
        static XPathExpression sXp_ItemType = XPathExpression.Compile("@itemtype");
        static XPathExpression sXp_PassageRef = XPathExpression.Compile("passageref");
        static XPathExpression sXp_MeasurementModel = XPathExpression.Compile("itemscoredimension/@measurementmodel");
        static XPathExpression sXp_Weight = XPathExpression.Compile("itemscoredimension/@weight");
        static XPathExpression sXp_ScorePoints = XPathExpression.Compile("itemscoredimension/@scorepoints");
        static XPathExpression sXp_ParameterName = XPathExpression.Compile("itemscoredimension/itemscoreparameter/@measurementparameter");
        static XPathExpression sXp_Parameter1 = XPathExpression.Compile("itemscoredimension/itemscoreparameter[@measurementparameter='a']/@value");
        static XPathExpression sXp_Parameter2 = XPathExpression.Compile("itemscoredimension/itemscoreparameter[@measurementparameter='a' or @measurementparameter='b0']/@value");
        static XPathExpression sXp_Parameter3 = XPathExpression.Compile("itemscoredimension/itemscoreparameter[@measurementparameter='a' or @measurementparameter='b1']/@value");
        static XPathExpression sXp_Parameter4 = XPathExpression.Compile("itemscoredimension/itemscoreparameter[@measurementparameter='b2']/@value");
        static XPathExpression sXp_Parameter5 = XPathExpression.Compile("itemscoredimension/itemscoreparameter[@measurementparameter='b3']/@value");
        static XPathExpression sXp_Bpref = XPathExpression.Compile("bpref");

        // Pool Properties
        static XPathExpression sXp_PoolProperty = XPathExpression.Compile("poolproperty");
        static XPathExpression sXp_PPProperty = XPathExpression.Compile("@property");
        static XPathExpression sXp_PPValue = XPathExpression.Compile("@value");

        // GroupItem Selector
        // /testspecification/administration/testform/formpartition/itemgroup/groupitem
        // /testspecification/administration/adminsegment/segmentpool/itemgroup/groupitem
        static XPathExpression sXp_GroupItem = XPathExpression.Compile("/testspecification//groupitem");

        // Parse bpref entries to standard, claim, target
        static Regex s_Rx_BprefMath = new Regex(@"^SBAC-(\d)\|[A-Z]{1,4}\|[0-9A-Z]{1,4}\|([A-Z]+(?:-\d+)?)$");
        static Regex s_Rx_BprefEla = new Regex(@"^SBAC-(\d-[A-Z]{1,2})\|(\d{1,2}-\d{1,2})(?:\|[A-Za-z0-9\-\.]+)?$");
        const string c_MathStdPrefix = "SBAC-MA-v6:";
        const string c_ElaStdPrefix = "SBAC-ELA-v1:";

        static Dictionary<string, int> sPoolPropertyMapping;
        static HashSet<string> sKnownMeasurementModels;
        static HashSet<string> sKnownMeasurementParameters;

        static TestPackageProcessor()
        {
            sPoolPropertyMapping = new Dictionary<string, int>();
            sPoolPropertyMapping.Add("Appropriate for Hearing Impaired", (int)ItemFieldNames.HearingImpaired);
            sPoolPropertyMapping.Add("ASL", (int)ItemFieldNames.ASL);
            sPoolPropertyMapping.Add("Braille", (int)ItemFieldNames.Braille);
            sPoolPropertyMapping.Add("Depth of Knowledge", (int)ItemFieldNames.DOK);
            sPoolPropertyMapping.Add("Grade", (int)ItemFieldNames.Grade);
            sPoolPropertyMapping.Add("Language", (int)ItemFieldNames.Language);
            sPoolPropertyMapping.Add("Scoring Engine", (int)ItemFieldNames.ScoringEngine);
            sPoolPropertyMapping.Add("Spanish Translation", (int)ItemFieldNames.Spanish);
            sPoolPropertyMapping.Add("Passage Length", (int)ItemFieldNames.PassageLength);
            sPoolPropertyMapping.Add("TDSPoolFilter", (int)ItemFieldNames.TDSPoolFilter);
            sPoolPropertyMapping.Add("Calculator", (int)ItemFieldNames.Calculator);
            sPoolPropertyMapping.Add("Glossary", (int)ItemFieldNames.Glossary);

            // Ignore these pool properties
            sPoolPropertyMapping.Add("--ITEMTYPE--", 0); // Value of zero means suppress
            sPoolPropertyMapping.Add("Difficulty Category", 0);
            sPoolPropertyMapping.Add("Test Pool", 0);
            sPoolPropertyMapping.Add("Rubric Source", 0);
            sPoolPropertyMapping.Add("Smarter Balanced Item Response Types", 0);
            sPoolPropertyMapping.Add("Answer Key", 0);
            sPoolPropertyMapping.Add("Claim2_Category", 0);
            sPoolPropertyMapping.Add("Revision Sub-category", 0);

            sKnownMeasurementModels = new HashSet<string>();
            sKnownMeasurementModels.Add("RAWSCORE");
            sKnownMeasurementModels.Add("IRT3PLn");
            sKnownMeasurementModels.Add("IRTGPC");

            sKnownMeasurementParameters = new HashSet<string>();
            sKnownMeasurementParameters.Add("a");
            sKnownMeasurementParameters.Add("b");
            sKnownMeasurementParameters.Add("c");
            sKnownMeasurementParameters.Add("b0");
            sKnownMeasurementParameters.Add("b1");
            sKnownMeasurementParameters.Add("b2");
            sKnownMeasurementParameters.Add("b3");
        }

        public TestPackageProcessor(string oFilename)
        {
            ReportingUtility.SetFileName(oFilename);
#if DEBUG 
            if (File.Exists(ReportingUtility.ErrorFileName)) File.Delete(ReportingUtility.ErrorFileName);
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
            var testSpecificationProcessor = new TestSpecificationProcessor(nav.SelectSingleNode("/testspecification"), ExpectedPackageType);

            if(testSpecificationProcessor.Attributes.ValidateAttribute(testSpecificationProcessor.Navigator, "package").Any(x => !x.Value.IsValid)) { 
                Console.WriteLine("Incorrect package type assigned");
                return; // If the test package is not what we expect, we should short circuit and return without processing any further
            }

            // Get the test info
            var testName = nav.Eval(sXp_TestName);
            var testSubject = nav.Eval(sXp_TestSubject);
            var testGrade = nav.Eval(sXp_TestGrade);
            var testType = nav.Eval(sXp_TestType);

            ReportingUtility.TestName = testName;
            testSpecificationProcessor.Process();

            // Index the group item info
            Dictionary<string, GroupItemInfo> indexGroupItemInfo = new Dictionary<string,GroupItemInfo>();
            {
                XPathNodeIterator nodes = nav.Select(sXp_GroupItem);
                while (nodes.MoveNext())
                {
                    XPathNavigator node = nodes.Current;
                    string itemId = Strip200(node.GetAttribute("itemid", string.Empty));
                    string isFieldTest = node.GetAttribute("isfieldtest", string.Empty);
                    string isActive = node.GetAttribute("isactive", string.Empty);
                    string responseRequired = node.GetAttribute("responserequired", string.Empty);
                    string adminRequired = node.GetAttribute("adminrequired", string.Empty);
                    string formPosition = node.GetAttribute("formposition", string.Empty);

                    GroupItemInfo gii;
                    if (indexGroupItemInfo.TryGetValue(itemId, out gii))
                    {
                        if (!string.Equals(gii.IsFieldTest, isFieldTest, StringComparison.Ordinal))
                        {
                            ReportingUtility.ReportError(testName, ErrorSeverity.Degraded, itemId, "Conflicting isfieldtest info: '{0}' <> '{1}'", isFieldTest, gii.IsFieldTest);
                        }
                        if (!string.Equals(gii.IsActive, isActive, StringComparison.Ordinal))
                        {
                            ReportingUtility.ReportError(testName, ErrorSeverity.Degraded, itemId, "Conflicting isactive info: '{0}' <> '{1}'", isActive, gii.IsActive);
                        }
                        if (!string.Equals(gii.ResponseRequired, responseRequired, StringComparison.Ordinal))
                        {
                            ReportingUtility.ReportError(testName, ErrorSeverity.Degraded, itemId, "Conflicting responserequired info: '{0}' <> '{1}'", responseRequired, gii.ResponseRequired);
                        }
                        if (!string.Equals(gii.AdminRequired, adminRequired, StringComparison.Ordinal))
                        {
                            ReportingUtility.ReportError(testName, ErrorSeverity.Degraded, itemId, "Conflicting adminrequired info: '{0}' <> '{1}'", adminRequired, gii.AdminRequired);
                        }
                        if (!string.Equals(gii.FormPosition, formPosition, StringComparison.Ordinal))
                        {
                            ReportingUtility.ReportError(testName, ErrorSeverity.Degraded, itemId, "Conflicting formposition info: '{0} <> '{1}'", formPosition, gii.FormPosition);
                        }
                    }
                    else
                    {
                        gii = new GroupItemInfo();
                        gii.IsFieldTest = isFieldTest;
                        gii.IsActive = isActive;
                        gii.ResponseRequired = responseRequired;
                        gii.AdminRequired = adminRequired;
                        gii.FormPosition = formPosition;
                        indexGroupItemInfo.Add(itemId, gii);
                        //Console.WriteLine(itemId);
                    }

                }
            }

            // Report the item fields
            int itemCount = 0;
            if (ReportingUtility.GetItemWriter() != null)
            {
                XPathNodeIterator nodes = nav.Select(sXp_Item);                
                while (nodes.MoveNext())
                {
                    ++itemCount;
                    XPathNavigator node = nodes.Current;

                    // Collect the item fields
                    string[] itemFields = new string[ItemFieldNamesCount];
                    itemFields[(int)ItemFieldNames.TestName] = testName;
                    itemFields[(int)ItemFieldNames.TestSubject] = testSubject;
                    itemFields[(int)ItemFieldNames.TestGrade] = testGrade;
                    itemFields[(int)ItemFieldNames.TestType] = testType;
                    string itemId = Strip200(node.Eval(sXp_ItemId));
                    itemFields[(int)ItemFieldNames.ItemId] = itemId;
                    itemFields[(int)ItemFieldNames.Filename] = node.Eval(sXp_Filename);
                    itemFields[(int)ItemFieldNames.Version] = node.Eval(sXp_Version);
                    itemFields[(int)ItemFieldNames.ItemType] = node.Eval(sXp_ItemType);
                    itemFields[(int)ItemFieldNames.PassageRef] = Strip200(node.Eval(sXp_PassageRef));

                    // Process PoolProperties
                    List<string> glossary = new List<string>();
                    XPathNodeIterator ppNodes = node.Select(sXp_PoolProperty);
                    while (ppNodes.MoveNext())
                    {
                        XPathNavigator ppNode = ppNodes.Current;
                        string ppProperty = ppNode.Eval(sXp_PPProperty).Trim();
                        string ppValue = ppNode.Eval(sXp_PPValue).Trim();
                        if (!string.IsNullOrEmpty(ppProperty) && !string.IsNullOrEmpty(ppValue))
                        {
                            // Special case for Braille language
                            int fieldIndex;
                            if (ppProperty.Equals("Language", StringComparison.Ordinal) && ppValue.Equals("ENU-Braille", StringComparison.Ordinal))
                            {
                                itemFields[(int)ItemFieldNames.LanguageBraille] = ppValue;
                            }

                            // Special case for Spanish language
                            else if (ppProperty.Equals("Language", StringComparison.Ordinal) && ppValue.Equals("ESN", StringComparison.Ordinal))
                            {
                                itemFields[(int)ItemFieldNames.Spanish] = "Y";
                            }

                            // Special case for Spanish language
                            else if (ppProperty.Equals("Spanish Translation", StringComparison.Ordinal))
                            {
                                itemFields[(int)ItemFieldNames.Spanish] = ppValue;
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
                                        ReportingUtility.ReportError(testName, ErrorSeverity.Degraded, itemId, "'{0}={1}' Multiple values for pool property", ppProperty, ppValue);
                                    itemFields[fieldIndex] = ppValue;
                                }
                            }
                            else
                            {
                                ReportingUtility.ReportError(testName, ErrorSeverity.Degraded, itemId, "'{0}={1}' Unrecognized Pool Property", ppProperty, ppValue);
                            }
                        }
                    }
                    glossary.Sort();
                    itemFields[(int)ItemFieldNames.Glossary] = string.Join(";", glossary);

                    itemFields[(int)ItemFieldNames.MeasurementModel] = node.Eval(sXp_MeasurementModel);
                    itemFields[(int)ItemFieldNames.Weight] = FormatDouble(node.Eval(sXp_Weight));
                    itemFields[(int)ItemFieldNames.ScorePoints] = node.Eval(sXp_ScorePoints);
                    itemFields[(int)ItemFieldNames.a] = FormatDouble(node.Eval(sXp_Parameter1));
                    itemFields[(int)ItemFieldNames.b0_b] = FormatDouble(node.Eval(sXp_Parameter2));
                    itemFields[(int)ItemFieldNames.b1_c] = FormatDouble(node.Eval(sXp_Parameter3));
                    itemFields[(int)ItemFieldNames.b2] = FormatDouble(node.Eval(sXp_Parameter4));
                    itemFields[(int)ItemFieldNames.b3] = FormatDouble(node.Eval(sXp_Parameter5));

                    // Check known measurement model
                    if (!sKnownMeasurementModels.Contains(itemFields[(int)ItemFieldNames.MeasurementModel]))
                        ReportingUtility.ReportError(testName, ErrorSeverity.Benign, itemId, "'{0}' Unrecognized Measurement Model", itemFields[(int)ItemFieldNames.MeasurementModel]);

                    // Check known parameters
                    XPathNodeIterator pnNodes = node.Select(sXp_ParameterName);
                    while (pnNodes.MoveNext())
                    {
                        string name = pnNodes.Current.Value;
                        if (!sKnownMeasurementParameters.Contains(name))
                            ReportingUtility.ReportError(testName, ErrorSeverity.Benign, itemId, "'{0}' Unrecognized Measurement Parameter", name);
                    }

                    // bprefs
                    int bpIndex = 0;
                    XPathNodeIterator bpNodes = node.Select(sXp_Bpref);
                    while (bpNodes.MoveNext())
                    {
                        string bpref = bpNodes.Current.Value;
                        if (bpIndex >= MaxBpRefs)
                            ReportingUtility.ReportError(testName, ErrorSeverity.Benign, itemId, "More than {0} bpref nodes", MaxBpRefs);
                        else
                            itemFields[(int)ItemFieldNames.bpref1 + bpIndex++] = bpref;

                        // Attempt to parse the bpref as an SBAC standard
                        // See http://www.smarterapp.org/documents/InterpretingSmarterBalancedStandardIDs.html
                        // A proper Math standard ID should be in this format: SBAC-MA-v6:1|P|TS04|D-6
                        // However, the bpref form substitutes "SBAC-" for "SBAC-MA-v6:"
                        // A proper ELA standard ID should be in this format: SBAC-ELA-v1:3-L|4-6|6.SL.2
                        // However, the bpref form substitutes "SBAC-" FOR "SBAC-ELA-v1:" and it drops the
                        // last segment which is the common core state standard.
                        if (testSubject.Equals("Math", StringComparison.OrdinalIgnoreCase))
                        {
                            Match match = s_Rx_BprefMath.Match(bpref);
                            if (match.Success)
                            {
                                itemFields[(int)ItemFieldNames.Standard] = string.Concat(c_MathStdPrefix, match.Value.Substring(5));
                                itemFields[(int)ItemFieldNames.Claim] = match.Groups[1].Value;
                                itemFields[(int)ItemFieldNames.Target] = match.Groups[2].Value;
                            }
                        }
                        else if (testSubject.Equals("ELA", StringComparison.OrdinalIgnoreCase))
                        {
                            Match match = s_Rx_BprefEla.Match(bpref);
                            if (match.Success)
                            {
                                itemFields[(int)ItemFieldNames.Standard] = string.Concat(c_ElaStdPrefix, match.Value.Substring(5));
                                itemFields[(int)ItemFieldNames.Claim] = match.Groups[1].Value + "\t";   // Adding tab character prevents Excel from treating these as dates.
                                itemFields[(int)ItemFieldNames.Target] = match.Groups[2].Value + "\t";
                            }
                        }
                    }
                    if (itemFields[(int)ItemFieldNames.Standard] == null) itemFields[(int)ItemFieldNames.Standard] = string.Empty;
                    if (itemFields[(int)ItemFieldNames.Claim] == null) itemFields[(int)ItemFieldNames.Claim] = string.Empty;
                    if (itemFields[(int)ItemFieldNames.Target] == null) itemFields[(int)ItemFieldNames.Target] = string.Empty;

                    GroupItemInfo gii;
                    if (indexGroupItemInfo.TryGetValue(itemId, out gii))
                    {
                        itemFields[(int)ItemFieldNames.IsFieldTest] = gii.IsFieldTest;
                        itemFields[(int)ItemFieldNames.IsActive] = gii.IsActive;
                        itemFields[(int)ItemFieldNames.ResponseRequired] = gii.ResponseRequired;
                        itemFields[(int)ItemFieldNames.AdminRequired] = gii.AdminRequired;
                        itemFields[(int)ItemFieldNames.FormPosition] = gii.FormPosition;
                    }
                    else
                    {
                        itemFields[(int)ItemFieldNames.IsFieldTest] = string.Empty;
                        itemFields[(int)ItemFieldNames.IsActive] = string.Empty;
                        itemFields[(int)ItemFieldNames.ResponseRequired] = string.Empty;
                        itemFields[(int)ItemFieldNames.AdminRequired] = string.Empty;
                        itemFields[(int)ItemFieldNames.FormPosition] = string.Empty;
                    }

                    // Write one line to the CSV
                    ReportingUtility.GetItemWriter().Write(itemFields);
                }
            }

            // Report the stimuli fields
            if (ReportingUtility.GetStimuliWriter() != null)
            {
                XPathNodeIterator nodes = nav.Select(sXp_Stim);
                while (nodes.MoveNext())
                {
                    XPathNavigator node = nodes.Current;

                    // Collect the stim fields
                    string[] stimFields = new string[StimFieldNamesCount];
                    stimFields[(int)StimFieldNames.TestName] = testName;
                    stimFields[(int)StimFieldNames.TestSubject] = testSubject;
                    stimFields[(int)StimFieldNames.TestGrade] = testGrade;
                    stimFields[(int)StimFieldNames.TestType] = testType;
                    stimFields[(int)StimFieldNames.StimId] = Strip200(node.Eval(sXp_ItemId));
                    stimFields[(int)StimFieldNames.Filename] = node.Eval(sXp_Filename);
                    stimFields[(int)StimFieldNames.Version] = node.Eval(sXp_Version);

                    // Write one line to the CSV
                    ReportingUtility.GetStimuliWriter().Write(stimFields);
                }
            }

            // Get the item counts
            int opitemcount = int.Parse(nav.Eval(XPathExpression.Compile("/testspecification//bpelement[@elementtype='test']/@opitemcount")));
            int ftitemcount = int.Parse(nav.Eval(XPathExpression.Compile("/testspecification//bpelement[@elementtype='test']/@ftitemcount")));
            int maxopitems = int.Parse(nav.Eval(XPathExpression.Compile("/testspecification//bpelement[@elementtype='test']/@maxopitems")));
            //Console.WriteLine("  opitemcount = {0}", opitemcount);
            //Console.WriteLine("  ftitemcount = {0}", ftitemcount);
            //Console.WriteLine("  totalcount  = {0}", opitemcount + ftitemcount);
            //Console.WriteLine("  maxopitems  = {0}", maxopitems);
            //Console.WriteLine("  opitemcount/maxopitems = {0}/{1} = {2}", opitemcount, maxopitems, ((double)opitemcount) / ((double)maxopitems));
            //Debug.WriteLine("{0},{1},{2}", testName, opitemcount, maxopitems);
        } // ProcessPackage

        static string Strip200(string val)
        {
            return val.StartsWith("200-", StringComparison.Ordinal) ? val.Substring(4) : val;
        }

        static string FormatDouble(string val)
        {
            if (string.IsNullOrEmpty(val)) return string.Empty;
            double v;
            return (double.TryParse(val, out v)) ? v.ToString("G", System.Globalization.CultureInfo.InvariantCulture) : val;
        }

        ~TestPackageProcessor()
        {
            Dispose(false);
        }


        public void Dispose()
        {
            Dispose(true);
        }

        void Dispose(bool disposing)
        {
            ReportingUtility.Dispose(disposing);
            if (disposing)
            {
                GC.SuppressFinalize(this);
            }
        }
    }
}
