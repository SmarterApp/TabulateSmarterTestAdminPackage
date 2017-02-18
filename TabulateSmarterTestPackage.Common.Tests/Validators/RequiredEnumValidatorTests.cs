using Microsoft.VisualStudio.TestTools.UnitTesting;
using TabulateSmarterTestAdminPackage.Common.RestrictedValues.Enums;
using TabulateSmarterTestAdminPackage.Common.Validators;

namespace TabulateSmarterTestPackage.Common.Tests.Validators
{
    [TestClass]
    public class RequiredEnumValidatorTests
    {
        private RequiredEnumValidator ItemUnderTest { get; set; }

        [TestMethod]
        public void ValidateValidInputShouldReturnTrue()
        {
            //Arrange
            var errorSeverity = ErrorSeverity.Benign;
            ItemUnderTest = new RequiredEnumValidator(errorSeverity, TestType.interim);
            var input = "interim";

            //Act
            var result = ItemUnderTest.IsValid(input);

            //Assert
            Assert.IsTrue(result);
            Assert.AreEqual(errorSeverity, ItemUnderTest.ErrorSeverity);
            Assert.IsTrue(string.IsNullOrEmpty(ItemUnderTest.GetMessage()));
        }
    }
}
