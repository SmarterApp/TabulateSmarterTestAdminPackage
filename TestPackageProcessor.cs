using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml.XPath;
using System.Diagnostics;
using System.Security.Cryptography;

namespace TabulateSmarterTestAdminPackage
{

    class TestPackageProcessor : ITestResultProcessor
    {
        enum ItemFieldNames : int
        {
            TestName,
            TestSubject,
            TestGrade,
            TestType, // PT or Summative
            ItemId, // Strip the "200-" bankId prefix
            Filename,
            Version,
            ItemType,
            Grade,
            PassageRef,
            PassageLength,
            HearingImpaired,
            ASL,
            Braille,
            LanguageBraille,
            DOK,
            Language,
            Calculator,
            Glossary,
            ScoringEngine,
            Spanish,
            TDSPoolFilter,
            IsFieldTest,
            IsActive,
            ResponseRequired,
            AdminRequired,
            FormPosition,
            MeasurementModel,
            Weight,
            ScorePoints,
            a,
            b0_b,
            b1_c,
            b2,
            b3,
            bpref1,
            bpref2,
            bpref3,
            bpref4,
            bpref5,
            bpref6,
            bpref7
        }
        static readonly int ItemFieldNamesCount = Enum.GetNames(typeof(ItemFieldNames)).Length;
        const int MaxBpRefs = 7;

        enum StimFieldNames : int
        {
            TestName,
            TestSubject,
            TestGrade,
            TestType, // PT or Summative
            StimId, // Strip the "200-" bankId prefix
            Filename,
            Version
        };
        static readonly int StimFieldNamesCount = Enum.GetNames(typeof(StimFieldNames)).Length;

        enum ErrorSeverity : int
        {
            Benign,
            Degraded,
            Severe
        }

        private class GroupItemInfo
        {
            public string IsFieldTest;
            public string IsActive;
            public string ResponseRequired;
            public string AdminRequired;
            public string FormPosition;
        }

        // Test Info
        static XPathExpression sXp_PackagePurpose = XPathExpression.Compile("/testspecification/@purpose");
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

        static Dictionary<string, int> sPoolPropertyMapping;
        static HashSet<string> sKnownMeasurementModels;
        static HashSet<string> sKnownMeasurementParameters;

        static TestPackageProcessor()
        {
            sPoolPropertyMapping = new Dictionary<string, int>();
            sPoolPropertyMapping.Add("--ITEMTYPE--", 0); // Value of zero means suppress
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

        Parse.CsvWriter m_itemWriter;
        Parse.CsvWriter m_stimWriter;
        string m_errFilename;
        Parse.CsvWriter m_errWriter;

        public TestPackageProcessor(string oFilename)
        {
            string itemFilename = oFilename + ".items.csv";
            string stimFilename = oFilename + ".stims.csv";
            m_errFilename = oFilename + ".errors.csv";
#if DEBUG
            if (File.Exists(m_errFilename)) File.Delete(m_errFilename);
#else
            if (File.Exists(itemFilename)) throw new ApplicationException(string.Format("Output file, '{0}' already exists.", itemFilename));
            if (File.Exists(stimFilename)) throw new ApplicationException(string.Format("Output file, '{0}' already exists.", stimFilename));
            if (File.Exists(m_errFilename)) throw new ApplicationException(string.Format("Output file, '{0}' already exists.", m_errFilename));
#endif
            m_itemWriter = new Parse.CsvWriter(itemFilename, false);
            m_itemWriter.Write(Enum.GetNames(typeof(ItemFieldNames)));

            m_stimWriter = new Parse.CsvWriter(stimFilename, false);
            m_stimWriter.Write(Enum.GetNames(typeof(StimFieldNames)));
        }

        public PackageType ExpectedPackageType { get; set; }

        static readonly UTF8Encoding UTF8NoByteOrderMark = new UTF8Encoding(false);

        public void ProcessResult(Stream input)
        {
            XPathDocument doc = new XPathDocument(input);
            XPathNavigator nav = doc.CreateNavigator();

            // Get the package purpose
            string purpose = nav.Eval(sXp_PackagePurpose);
            if (!string.Equals(purpose, ExpectedPackageType.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("  Skipping package. Type is '{0}' but processing '{1}.", purpose, ExpectedPackageType);
            }

            // Get the test info
            string testName = nav.Eval(sXp_TestName);
            string testSubject = nav.Eval(sXp_TestSubject);
            string testGrade = nav.Eval(sXp_TestGrade);
            string testType = nav.Eval(sXp_TestType);

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
                            ReportError(testName, ErrorSeverity.Degraded, itemId, "Conflicting isfieldtest info: '{0}' <> '{1}'", isFieldTest, gii.IsFieldTest);
                        }
                        if (!string.Equals(gii.IsActive, isActive, StringComparison.Ordinal))
                        {
                            ReportError(testName, ErrorSeverity.Degraded, itemId, "Conflicting isactive info: '{0}' <> '{1}'", isActive, gii.IsActive);
                        }
                        if (!string.Equals(gii.ResponseRequired, responseRequired, StringComparison.Ordinal))
                        {
                            ReportError(testName, ErrorSeverity.Degraded, itemId, "Conflicting responserequired info: '{0}' <> '{1}'", responseRequired, gii.ResponseRequired);
                        }
                        if (!string.Equals(gii.AdminRequired, adminRequired, StringComparison.Ordinal))
                        {
                            ReportError(testName, ErrorSeverity.Degraded, itemId, "Conflicting adminrequired info: '{0}' <> '{1}'", adminRequired, gii.AdminRequired);
                        }
                        if (!string.Equals(gii.FormPosition, formPosition, StringComparison.Ordinal))
                        {
                            ReportError(testName, ErrorSeverity.Degraded, itemId, "Conflicting formposition info: '{0} <> '{1}'", formPosition, gii.FormPosition);
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
            if (m_itemWriter != null)
            {
                XPathNodeIterator nodes = nav.Select(sXp_Item);                
                while (nodes.MoveNext())
                {
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
                                        ReportError(testName, ErrorSeverity.Degraded, itemId, "'{0}={1}' Multiple values for pool property", ppProperty, ppValue);
                                    itemFields[fieldIndex] = ppValue;
                                }
                            }
                            else
                            {
                                ReportError(testName, ErrorSeverity.Degraded, itemId, "'{0}={1}' Unrecognized Pool Property", ppProperty, ppValue);
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
                        ReportError(testName, ErrorSeverity.Benign, itemId, "'{0}' Unrecognized Measurement Model", itemFields[(int)ItemFieldNames.MeasurementModel]);

                    // Check known parameters
                    XPathNodeIterator pnNodes = node.Select(sXp_ParameterName);
                    while (pnNodes.MoveNext())
                    {
                        string name = pnNodes.Current.Value;
                        if (!sKnownMeasurementParameters.Contains(name))
                            ReportError(testName, ErrorSeverity.Benign, itemId, "'{0}' Unrecognized Measurement Parameter", name);
                    }

                    // bprefs
                    int bpIndex = 0;
                    XPathNodeIterator bpNodes = node.Select(sXp_Bpref);
                    while (bpNodes.MoveNext())
                    {
                        string bpref = bpNodes.Current.Value;
                        if (bpIndex >= MaxBpRefs)
                            ReportError(testName, ErrorSeverity.Benign, itemId, "More than {0} bpref nodes", MaxBpRefs);
                        else
                            itemFields[(int)ItemFieldNames.bpref1 + bpIndex++] = bpref;
                    }

                    GroupItemInfo gii;
                    if (indexGroupItemInfo.TryGetValue(itemId, out gii))
                    {
                        itemFields[(int)ItemFieldNames.IsFieldTest] = gii.IsFieldTest;
                        itemFields[(int)ItemFieldNames.IsActive] = gii.IsActive;
                        itemFields[(int)ItemFieldNames.ResponseRequired] = gii.ResponseRequired;
                        itemFields[(int)ItemFieldNames.AdminRequired] = gii.AdminRequired;
                        itemFields[(int)ItemFieldNames.FormPosition] = gii.FormPosition;
                    }

                    // Write one line to the CSV
                    m_itemWriter.Write(itemFields);
                }
            }

            // Report the stimuli fields
            if (m_stimWriter != null)
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
                    m_stimWriter.Write(stimFields);
                }
            }

        } // ProcessPackage

        void ReportError(string testName, ErrorSeverity severity, string itemId, string message, params object[] args)
        {
            if (m_errWriter == null)
            {
                m_errWriter = new Parse.CsvWriter(m_errFilename, false);
                m_errWriter.Write(new string[] { "TestName", "Severity", "ItemId", "Message" });
            }

            string outMessage = string.Format(message, args);
            m_errWriter.Write(new string[] { testName, severity.ToString(), itemId, outMessage });
            Console.WriteLine("    " + outMessage);
#if DEBUG1
            Debug.Fail(outMessage);
#endif
        }

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
            if (m_itemWriter != null)
            {
#if DEBUG
                if (!disposing) Debug.Fail("Failed to dispose TestPackageProcessor");
#endif
                m_itemWriter.Dispose();
                m_itemWriter = null;
            }
            if (m_stimWriter != null)
            {
                m_stimWriter.Dispose();
                m_stimWriter = null;
            }
            if (m_errWriter != null)
            {
                m_errWriter.Dispose();
                m_errWriter = null;
            }
            if (disposing)
            {
                GC.SuppressFinalize(this);
            }
        }
    }

    static class XPathNavitagorHelper
    {
        public static string Eval(this XPathNavigator nav, XPathExpression expression)
        {
            if (expression.ReturnType == XPathResultType.NodeSet)
            {
                XPathNodeIterator nodes = nav.Select(expression);
                if (nodes.MoveNext())
                {
                    return nodes.Current.ToString();
                }
                else
                {
                    return string.Empty;
                }
            }
            else
            {
                return nav.Evaluate(expression).ToString();
            }
        }
    }
}
