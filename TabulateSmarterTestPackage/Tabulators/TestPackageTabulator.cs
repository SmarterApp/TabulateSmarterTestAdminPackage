using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.XPath;
using NLog;
using ProcessSmarterTestPackage.Processors.Common;
using ProcessSmarterTestPackage.Processors.Common.ItemPool.Passage;
using SmarterTestPackage.Common.Data;
using SmarterTestPackage.Common.Extensions;
using TabulateSmarterTestPackage.Models;
using TabulateSmarterTestPackage.Utilities;
using ValidateSmarterTestPackage.RestrictedValues.Enums;
using ProcessSmarterTestPackage.Processors.Combined;


namespace TabulateSmarterTestPackage.Tabulators
{
    internal class TestPackageTabulator : IDisposable
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public TestPackageTabulator(string oFilename)
        {
            ReportingUtility.SetFileName(oFilename);
#if DEBUG
            if (File.Exists(ReportingUtility.ErrorFileName))
            {
                File.Delete(ReportingUtility.ErrorFileName);
            }
#else
            if (File.Exists(ReportingUtility.ItemFileName))
            {
                throw new ApplicationException($"Output file, '{ReportingUtility.ItemFileName}' already exists.");
            }
            if (File.Exists(ReportingUtility.StimuliFileName))
            {
                throw new ApplicationException($"Output file, '{ReportingUtility.StimuliFileName}' already exists.");
            }
            if (File.Exists(ReportingUtility.ErrorFileName))
            {
                throw new ApplicationException($"Output file, '{ReportingUtility.ErrorFileName}' already exists.");
            }
#endif
        }

        private PackageType ExpectedPackageType { get; set; }


        public void Dispose()
        {
            Dispose(true);
        }

        public Processor ProcessResult(Stream input)
        {
            try
            {
                var doc = new XPathDocument(input);
                var nav = doc.CreateNavigator();
                var nodeSelector = "";

                if (nav.IsNode && nav.SelectSingleNode("//TestPackage") != null)
                {

                    Logger.Info("Processing new package format.");
                    nodeSelector = "//TestPackage";
                    ExpectedPackageType = PackageType.Combined;
                }
                else if (nav.IsNode && nav.SelectSingleNode("/testspecification") != null)
                {
                    Logger.Info("Processing old package format.");
                    nodeSelector = "/testspecification";
                    var packageType = nav.SelectSingleNode(nodeSelector)
                        .Eval(XPathExpression.Compile("@purpose"));
                    if (packageType.Equals("administration", StringComparison.OrdinalIgnoreCase))
                    {
                        ExpectedPackageType = PackageType.Administration;
                    }
                    else if (packageType.Equals("scoring", StringComparison.OrdinalIgnoreCase))
                    {
                        ExpectedPackageType = PackageType.Scoring;
                    }
                    else
                    {
                        throw new ArgumentException("UnrecognizedPackageType");
                    }
                }
                else
                {
                    throw new ArgumentException("UnrecognizedPackageType");
                }

                if (ExpectedPackageType != PackageType.Combined)
                {
                    var testSpecificationProcessor = new TestSpecificationProcessor(nav.SelectSingleNode(nodeSelector),
                        ExpectedPackageType);
                    testSpecificationProcessor.Process();
                    return testSpecificationProcessor;
                }
                else
                {
                    var combinedTestProcessor = new CombinedTestProcessor(nav.SelectSingleNode(nodeSelector), ExpectedPackageType);
                    combinedTestProcessor.Process();
                    return combinedTestProcessor;
                }
            }
            catch (XmlException e)
            {
                throw new ArgumentException($"XML document parse failure. Error:  \"{e.Message}\" Skipping file.");
            }
            

        }

       
        public void TabulateResults(List<Processor> testSpecificationProcessors,
            List<ProcessingError> crossTabulationErrors)
        {
            foreach (var testSpecificationProcessor in testSpecificationProcessors)
            {
                // Extract the test info
                var testInfo = new TestInformation();
                IDictionary<ItemFieldNames, string> testInformation;
                    
                if (testSpecificationProcessor is CombinedTestProcessor)
                {
                    testInformation = testInfo.RetrieveTestInformation((CombinedTestProcessor)testSpecificationProcessor);
                    var combinedItemTabulator = new CombinedItemTabulator();
                    var combinedItems = combinedItemTabulator.ProcessResult(testSpecificationProcessor.Navigator,
                        (CombinedTestProcessor)testSpecificationProcessor,
                        testInformation);
                    combinedItems.ToList().ForEach(x => ReportingUtility.GetItemWriter().Write(x.ToArray()));
                    var combinedItemsRDW = PrepareFieldsForRDW(combinedItems);
                    combinedItemsRDW.ToList().ForEach(x => ReportingUtility.GetItemWriterRDW().Write(x.ToArray()));

                    var stimuliNodes = testSpecificationProcessor.Navigator.Select("//Stimulus");
                    if (stimuliNodes.Count > 0)
                    {
                        var stimuliTabulator = new StimuliTabulator();
                        var stimuli = stimuliTabulator.ProcessResult(stimuliNodes, testInformation);
                        stimuli.ToList().ForEach(x => ReportingUtility.GetStimuliWriter().Write(x.ToArray()));
                    }
                    

                }
                else
                {
                    testInformation = testInfo.RetrieveTestInformation((TestSpecificationProcessor)testSpecificationProcessor);
                    var itemTabulator = new ItemTabulator();
                    var items = itemTabulator.ProcessResult(testSpecificationProcessor.Navigator, testSpecificationProcessor,
                        testInformation);
                    items.ToList().ForEach(x => ReportingUtility.GetItemWriter().Write(x.ToArray()));
                    var itemsRDW = PrepareFieldsForRDW(items);
                    itemsRDW.ToList().ForEach(x => ReportingUtility.GetItemWriterRDW().Write(x.ToArray()));

                    var assessmentRoot = testSpecificationProcessor.ChildNodeWithName("administration") ??
                                         testSpecificationProcessor.ChildNodeWithName("scoring");
                    var passages = assessmentRoot.ChildNodeWithName("itempool").ChildNodesWithName("passage").ToList();
                    if (passages.Any())
                    {
                        var stimuliTabulator = new StimuliTabulator();
                        var stimuli = stimuliTabulator.ProcessResult(passages.Cast<PassageProcessor>().ToList(),
                            testInformation);
                        stimuli.ToList().ForEach(x => ReportingUtility.GetStimuliWriter().Write(x.ToArray()));
                    }
                }

                ReportingUtility.TestName = testInformation[ItemFieldNames.AssessmentName];

                var errors = testSpecificationProcessor.GenerateErrorMessages().Cast<ProcessingError>().ToList();
                errors.AddRange(crossTabulationErrors);
                errors.AddRange(testInfo.Errors);
                errors.AddRange(ItemTabulator.ItemTabulationErrors);
                ItemTabulator.ItemTabulationErrors.Clear();
                var errorList = new List<List<string>>();
                errorList.AddRange(errors.Select(x => new List<string>
                {
                    testSpecificationProcessor.GetUniqueId(),
                    x.PackageType.ToString(),
                    x.ErrorSeverity.ToString(),
                    x.Location,
                    x.ItemId,
                    x.Value,
                    x.Message
                }));

                errorList.ForEach(x => ReportingUtility.GetErrorWriter().Write(x.ToArray()));
            }
        }

        public void AddTabulationHeaders(int performancelevels = 0)
        {
            ReportingUtility.GetErrorWriter()
                .Write(new[]
                    {"AssessmentId", "PackageType", "ErrorSeverity", "Location", "UniqueId", "Value", "Message"});
            ReportingUtility.GetStimuliWriter().Write(Enum.GetNames(typeof(StimFieldNames)));

            var itemHeaders = new List<string>();
            itemHeaders.AddRange(Enum.GetNames(typeof(ItemFieldNames)).ToList());
            for (var i = 0; i < performancelevels; i++)
            {
                itemHeaders.AddRange(new List<string> {"PerformanceLevel", "ScaledLow", "ScaledHigh"});
            }
            ReportingUtility.GetItemWriter().Write(itemHeaders.ToArray());

            // Specific RDW headers
            var itemHeadersRDW = new List<string>();
            itemHeadersRDW.AddRange(Enum.GetNames(typeof(ItemFieldNamesRDW)).ToList());
            for (var i = 0; i < performancelevels; i++)
            {
                itemHeadersRDW.AddRange(new List<string> { "PerformanceLevel", "ScaledLow", "ScaledHigh" });
            }
            ReportingUtility.GetItemWriterRDW().Write(itemHeadersRDW.ToArray());
        }

        ~TestPackageTabulator()
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

        private IEnumerable<IEnumerable<string>> PrepareFieldsForRDW(IEnumerable<IEnumerable<string>> itemsFullList)
        {
            var rdwList = new List<List<string>>();
            

            foreach(var item in itemsFullList)
            {
                int fieldCounter = 0;
                var rdwFieldList = new List<string>();
                foreach(var field in item)
                {
                    if (fieldCounter != 35 && fieldCounter != 39 && fieldCounter != 47 &&
                        fieldCounter != 48 && fieldCounter != 49 && fieldCounter != 50 && 
                        fieldCounter != 51 && fieldCounter != 52 && fieldCounter != 53 &&
                        fieldCounter != 54 && fieldCounter != 55 && fieldCounter != 82 && 
                        fieldCounter != 83)
                    {
                        rdwFieldList.Add(field);
                    }
                    if (fieldCounter == 68)
                    {
                        rdwFieldList.Add("");
                        //fieldCounter++;
                    }
                    fieldCounter++;
                }
                rdwList.Add(rdwFieldList);
            }

            return rdwList;
        }
    }
}