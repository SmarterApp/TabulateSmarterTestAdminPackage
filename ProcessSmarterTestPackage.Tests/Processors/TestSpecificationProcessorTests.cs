using System.IO;
using System.Xml.XPath;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProcessSmarterTestPackage.Processors.Common;
using ValidateSmarterTestPackage.RestrictedValues.Enums;

namespace ProcessSmarterTestAdminPackage.Tests.Processors
{
    [TestClass]
    public class TestSpecificationProcessorTests
    {
        public TestSpecificationProcessor ItemUnderTest { get; set; }

        [TestMethod]
        public void IsValidVersionTest()
        {
            // Arrange
            var doc = new XPathDocument(new StringReader(Resource._SBAC_PT_SBAC_IRP_ELA_3_COMBINED_Summer_2015_2016));
            var testspecification =
                new TestSpecificationProcessor(doc.CreateNavigator().SelectSingleNode("/testspecification"),
                    PackageType.Administration);

            // Act
            var result = testspecification.Process();
            var errors = testspecification.GenerateErrorMessages();

            // Assert
            Assert.IsTrue(result);
        }
    }
}