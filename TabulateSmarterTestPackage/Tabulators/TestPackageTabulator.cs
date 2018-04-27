using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Schema;
using NLog;
using ProcessSmarterTestPackage.Processors.Common;
using ProcessSmarterTestPackage.Processors.Common.ItemPool.Passage;
using SmarterTestPackage.Common.Data;
using SmarterTestPackage.Common.Extensions;
using TabulateSmarterTestPackage.Models;
using TabulateSmarterTestPackage.Utilities;
using ValidateSmarterTestPackage.RestrictedValues.Enums;

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

        public PackageType ExpectedPackageType { get; set; }


        public void Dispose()
        {
            Dispose(true);
        }

        public TestSpecificationProcessor ProcessResult(Stream input)
        {
            var doc = new XPathDocument(input);
            var nav = doc.CreateNavigator();
            var nodeSelector = "";

            if (nav.IsNode && nav.SelectSingleNode("//TestPackage") != null)
            {
                
                Logger.Debug("****************************New Type");
                nodeSelector = "//TestPackage";
                ExpectedPackageType = PackageType.Combined;

                //load and validate with XML schema
                try
                {
                    XmlDocument validateDocument = new XmlDocument();
                    validateDocument.LoadXml(nav.OuterXml);
                    validateDocument.Schemas.Add(null, "Resources/v4-test-package.xsd"); //TODO can I put this in a setting or something?
                    ValidationEventHandler validation = new ValidationEventHandler(SchemaValidationHandler);
                    validateDocument.Validate(validation);
                }
                catch (XmlException e)
                {
                    Logger.Error("Schema Validation Error: {0}", e.Message);
                    throw new ArgumentException("XML Validation Failure");
                }
                
            }
            else if (nav.IsNode && nav.SelectSingleNode("/testspecification") != null)
            {
                Logger.Debug("^^^^^^^^^^^^^^^^^^^^^^^^^^^^^OLD Type");
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
            }
            else
            {
                throw new ArgumentException("UnrecognizedPackageType");
            }
            var testSpecificationProcessor = new TestSpecificationProcessor(nav.SelectSingleNode(nodeSelector),
                ExpectedPackageType);
            testSpecificationProcessor.Process();
            return testSpecificationProcessor;


        }

        static void SchemaValidationHandler(object sender, ValidationEventArgs e)
        {
            Logger.Debug("VALIDATION ERROR!!");
            switch (e.Severity)
            {
                case XmlSeverityType.Error:
                    Logger.Error("Schema Validation Error: {0}", e.Message);
                    throw new ArgumentException("XML Validation Failure");
                case XmlSeverityType.Warning:
                    Logger.Error("Schema Validation Warning: {0}", e.Message);
                    throw new ArgumentException("XML Validation Warning");
                default:
                    throw new ArgumentException("XML Validation Failure");
            }
        }


        public void TabulateResults(List<TestSpecificationProcessor> testSpecificationProcessors,
            List<ProcessingError> crossTabulationErrors)
        {
            foreach (var testSpecificationProcessor in testSpecificationProcessors)
            {
                // Extract the test info
                var testInfo = new TestInformation();
                var testInformation = testInfo.RetrieveTestInformation(testSpecificationProcessor);

                ReportingUtility.TestName = testInformation[ItemFieldNames.AssessmentName];

                var itemTabulator = new ItemTabulator();
                var items = itemTabulator.ProcessResult(testSpecificationProcessor.Navigator, testSpecificationProcessor,
                    testInformation);
                items.ToList().ForEach(x => ReportingUtility.GetItemWriter().Write(x.ToArray()));

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

                var errors = testSpecificationProcessor.GenerateErrorMessages().Cast<ProcessingError>().ToList();
                errors.AddRange(crossTabulationErrors);
                errors.AddRange(testInfo.Errors);
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
    }
}