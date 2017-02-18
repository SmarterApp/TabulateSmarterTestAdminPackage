using Microsoft.VisualStudio.TestTools.UnitTesting;
using TabulateSmarterTestAdminPackage.Common.RestrictedValues.Enums;
using TabulateSmarterTestAdminPackage.Common.Validators;

namespace TabulateSmarterTestPackage.Common.Tests.Validators
{
    [TestClass]
    public class RequiredRegularExpressionValidatorTests
    {
        private RequiredRegularExpressionValidator ItemUnderTest { get; set; }

        [TestMethod]
        public void ValidInputShouldReturnTrue()
        {
            //Arrange
            const ErrorSeverity errorSeverity = ErrorSeverity.Benign;
            ItemUnderTest = new RequiredRegularExpressionValidator(errorSeverity, "^[A-Z]$");
            const string input = "A";

            //Act
            var result = ItemUnderTest.IsValid(input);

            //Assert
            Assert.IsTrue(result);
            Assert.AreEqual(errorSeverity, ItemUnderTest.ErrorSeverity);
        }

        [TestMethod]
        public void LowercaseInputShouldReturnFalse()
        {
            //Arrange
            const ErrorSeverity errorSeverity = ErrorSeverity.Severe;
            ItemUnderTest = new RequiredRegularExpressionValidator(errorSeverity, "^[A-Z]$");
            const string input = "a";

            //Act
            var result = ItemUnderTest.IsValid(input);

            //Assert
            Assert.IsFalse(result);
            Assert.AreEqual(errorSeverity, ItemUnderTest.ErrorSeverity);
        }

        [TestMethod]
        public void LongWordInputShouldReturnFalse()
        {
            //Arrange
            const ErrorSeverity errorSeverity = ErrorSeverity.Severe;
            ItemUnderTest = new RequiredRegularExpressionValidator(errorSeverity, "^[A-Z]$");
            const string input = "Anteater";

            //Act
            var result = ItemUnderTest.IsValid(input);

            //Assert
            Assert.IsFalse(result);
            Assert.AreEqual(errorSeverity, ItemUnderTest.ErrorSeverity);
        }

        [TestMethod]
        public void NumberInputShouldReturnFalse()
        {
            //Arrange
            const ErrorSeverity errorSeverity = ErrorSeverity.Severe;
            ItemUnderTest = new RequiredRegularExpressionValidator(errorSeverity, "^[A-Z]$");
            const string input = "12";

            //Act
            var result = ItemUnderTest.IsValid(input);

            //Assert
            Assert.IsFalse(result);
            Assert.AreEqual(errorSeverity, ItemUnderTest.ErrorSeverity);
        }

        [TestMethod]
        public void SpecialCharacterInputShouldReturnFalse()
        {
            //Arrange
            const ErrorSeverity errorSeverity = ErrorSeverity.Severe;
            const string pattern = "^[A-Z]$";
            ItemUnderTest = new RequiredRegularExpressionValidator(errorSeverity, pattern);
            const string input = "@$!";

            //Act
            var result = ItemUnderTest.IsValid(input);

            //Assert
            Assert.IsFalse(result);
            Assert.AreEqual(errorSeverity, ItemUnderTest.ErrorSeverity);
            Assert.AreEqual(ItemUnderTest.GetMessage(), $"[RequiredMatch:{pattern}]");
        }
    }
}