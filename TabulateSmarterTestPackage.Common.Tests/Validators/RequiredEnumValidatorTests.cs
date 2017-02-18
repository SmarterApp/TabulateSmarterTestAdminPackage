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
        public void ValidInputShouldReturnTrue()
        {
            //Arrange
            const ErrorSeverity errorSeverity = ErrorSeverity.Benign;
            ItemUnderTest = new RequiredEnumValidator(errorSeverity, RestrictedListItems.TestType);
            const string input = "interim";

            //Act
            var result = ItemUnderTest.IsValid(input);

            //Assert
            Assert.IsTrue(result);
            Assert.AreEqual(errorSeverity, ItemUnderTest.ErrorSeverity);
        }

        [TestMethod]
        public void InvalidInputShouldReturnFalse()
        {
            //Arrange
            const ErrorSeverity errorSeverity = ErrorSeverity.Severe;
            ItemUnderTest = new RequiredEnumValidator(errorSeverity, RestrictedListItems.TestType);
            const string input = "exceptional";

            //Act
            var result = ItemUnderTest.IsValid(input);

            //Assert
            Assert.IsFalse(result);
            Assert.AreEqual(errorSeverity, ItemUnderTest.ErrorSeverity);
        }

        [TestMethod]
        public void ValidInputWrongEnumShouldReturnFalse()
        {
            //Arrange
            const ErrorSeverity errorSeverity = ErrorSeverity.Benign;
            ItemUnderTest = new RequiredEnumValidator(errorSeverity, RestrictedListItems.PackageType);
            const string input = "interim";

            //Act
            var result = ItemUnderTest.IsValid(input);

            //Assert
            Assert.IsFalse(result);
            Assert.AreEqual(errorSeverity, ItemUnderTest.ErrorSeverity);
        }

        [TestMethod]
        public void ValidInputDifferentCaseShouldReturnTrue()
        {
            //Arrange
            const ErrorSeverity errorSeverity = ErrorSeverity.Benign;
            ItemUnderTest = new RequiredEnumValidator(errorSeverity, RestrictedListItems.TestType);
            const string input = "INTERIM";

            //Act
            var result = ItemUnderTest.IsValid(input);

            //Assert
            Assert.IsTrue(result);
            Assert.AreEqual(errorSeverity, ItemUnderTest.ErrorSeverity);
        }

        [TestMethod]
        public void InvalidParameterTypeShouldReturnFalse()
        {
            //Arrange
            const ErrorSeverity errorSeverity = ErrorSeverity.Benign;
            ItemUnderTest = new RequiredEnumValidator(errorSeverity, "bad data");
            const string input = "interim";

            //Act
            var result = ItemUnderTest.IsValid(input);

            //Assert
            Assert.IsFalse(result);
            Assert.AreEqual(errorSeverity, ItemUnderTest.ErrorSeverity);
            Assert.AreEqual(ItemUnderTest.GetMessage(), "[IncorrectArgumentProvidedToValidator:bad data]");
        }

        [TestMethod]
        public void InvalidValueTypeShouldReturnFalse()
        {
            //Arrange
            const ErrorSeverity errorSeverity = ErrorSeverity.Benign;
            ItemUnderTest = new RequiredEnumValidator(errorSeverity, RestrictedListItems.TestType);
            const int input = 1;

            //Act
            var result = ItemUnderTest.IsValid(input);

            //Assert
            Assert.IsFalse(result);
            Assert.AreEqual(errorSeverity, ItemUnderTest.ErrorSeverity);
        }
    }
}