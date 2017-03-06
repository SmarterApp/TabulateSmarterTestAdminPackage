using System.IO;
using System.Xml.XPath;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProcessSmarterTestPackage.Processors.Common;
using TabulateSmarterTestPackage.Utilities;
using ValidateSmarterTestPackage.RestrictedValues.Enums;

namespace ProcessSmarterTestAdminPackage.Tests.Processors
{
    [TestClass]
    public class IdentifierProcessorTests
    {
        public IdentifierProcessorTests IdentifierProcessor;

        [TestInitialize]
        public void Setup()
        {
            ReportingUtility.SetFileName("UnitTests");
            ReportingUtility.TestName = "UnitTests";
            if (File.Exists(ReportingUtility.ErrorFileName))
            {
                File.Delete(ReportingUtility.ErrorFileName);
            }
        }

        [TestMethod]
        public void IsValidVersionTest()
        {
            // Arrange
            var doc = new XPathDocument(new StringReader(Resource._SBAC_PT_SBAC_IRP_CAT_ELA_3_Summer_2015_2016));
            var subject =
                new IdentifierProcessor(doc.CreateNavigator().SelectSingleNode("/testspecification/identifier"),
                    PackageType.Administration);

            // Act
            var result = subject.Process();
            ReportingUtility.Dispose(true);

            // Assert
            Assert.IsTrue(result);
        }
    }
}