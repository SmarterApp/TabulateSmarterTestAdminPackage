using System.IO;
using System.Xml.XPath;
using NUnit.Framework;
using TabulateSmarterTestAdminPackage.Common.Utilities;
using TabulateSmarterTestAdminPackage.Processors.Specification.TestSpecification;

namespace TabulateSmarterTestAdminPackage.Tests.Processors
{
    [TestFixture]
    public class IdentifierProcessorTests
    {
        [SetUp]
        public void Setup()
        {
            ReportingUtility.SetFileName("UnitTests");
            ReportingUtility.TestName = "UnitTests";
            if (File.Exists(ReportingUtility.ErrorFileName))
            {
                File.Delete(ReportingUtility.ErrorFileName);
            }
        }

        public IdentifierProcessorTests IdentifierProcessor;

        [Test]
        public void IsValidUniqueIdTest()
        {
            // Arrange
            var doc = new XPathDocument(new StringReader(Resource._SBAC_PT_SBAC_IRP_CAT_ELA_3_Summer_2015_2016));
            var subject =
                new IdentifierProcessor(doc.CreateNavigator().SelectSingleNode("/testspecification/identifier"));

            // Act
            var result = subject.IsValidUniqueId();
            ReportingUtility.Dispose(true);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void IsValidVersionTest()
        {
            // Arrange
            var doc = new XPathDocument(new StringReader(Resource._SBAC_PT_SBAC_IRP_CAT_ELA_3_Summer_2015_2016));
            var subject =
                new IdentifierProcessor(doc.CreateNavigator().SelectSingleNode("/testspecification/identifier"));

            // Act
            var result = subject.IsValidVersion();
            ReportingUtility.Dispose(true);

            // Assert
            Assert.IsTrue(result);
        }
    }
}