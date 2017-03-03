using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.XPath;
using ProcessSmarterTestPackage.Processors.Common;
using ProcessSmarterTestPackage.Processors.Common.ItemPool.Passage;
using SmarterTestPackage.Common.Data;
using SmarterTestPackage.Common.Extensions;
using TabulateSmarterTestPackage.Utilities;
using TabulateSmarterTestPackage.Utilities.Data;
using ValidateSmarterTestPackage.RestrictedValues.Enums;

namespace TabulateSmarterTestPackage.Tabulators
{
    internal class TestPackageTabulator : IDisposable
    {
        public TestPackageTabulator(string oFilename)
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

            // /testspecification
            ExpectedPackageType = nav.SelectSingleNode("/testspecification")
                .Eval(XPathExpression.Compile("@purpose"))
                .Equals("administration", StringComparison.InvariantCultureIgnoreCase)
                ? PackageType.Administration
                : PackageType.Scoring;
            var testSpecificationProcessor = new TestSpecificationProcessor(nav.SelectSingleNode("/testspecification"),
                ExpectedPackageType);
            testSpecificationProcessor.Process();

            return testSpecificationProcessor;
        }

        public void TabulateResults(IList<TestSpecificationProcessor> testSpecificationProcessors,
            IList<ProcessingError> crossTabulationErrors)
        {
            foreach (var testSpecificationProcessor in testSpecificationProcessors)
            {
                var errors = testSpecificationProcessor.GenerateErrorMessages().Cast<ProcessingError>();
                var errorList = new List<List<string>>();
                errorList.AddRange(errors.Select(x => new List<string>
                {
                    testSpecificationProcessor.ChildNodeWithName("identifier").ValueForAttribute("uniqueid"),
                    x.ErrorSeverity.ToString(),
                    x.Path,
                    x.ItemId,
                    x.Message
                }));
                errorList.ForEach(x => ReportingUtility.GetErrorWriter().Write(x.ToArray()));

                // Extract the test info
                var testInformation = TestInformation.RetrieveTestInformation(testSpecificationProcessor);

                ReportingUtility.TestName = testInformation[ItemFieldNames.AssessmentName];

                var itemTabulator = new ItemTabulator();
                var items = itemTabulator.ProcessResult(testSpecificationProcessor.Navigator, testSpecificationProcessor,
                    testInformation);
                items.ToList().ForEach(x => ReportingUtility.GetItemWriter().Write(x.ToArray()));

                var assessmentRoot = ExpectedPackageType == PackageType.Administration
                    ? testSpecificationProcessor.ChildNodeWithName("administration")
                    : testSpecificationProcessor.ChildNodeWithName("scoring");
                var passages = assessmentRoot.ChildNodeWithName("itempool").ChildNodesWithName("passage");
                var stimuliTabulator = new StimuliTabulator();
                var stimuli = stimuliTabulator.ProcessResult(passages.Cast<PassageProcessor>().ToList(), testInformation);
                stimuli.ToList().ForEach(x => ReportingUtility.GetStimuliWriter().Write(x.ToArray()));
            }
        }

        public void AddTabulationHeaders(int performancelevels = 0)
        {
            ReportingUtility.GetErrorWriter()
                .Write(new[] {"AssessmentName", "ErrorSeverity", "Path", "UniqueId", "Message"});
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