using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmarterTestPackage.Common.Data;
using ValidateSmarterTestPackage.Validators;

namespace TabulateSmarterTestPackage.Common.Tests.Validators
{
    [TestClass]
    public class StringMatchValidatorTests
    {
        private StringMatchValidator ItemUnderTest { get; set; }

        [TestMethod]
        public void ValidInputShouldReturnTrue()
        {
            //Arrange
            const ErrorSeverity errorSeverity = ErrorSeverity.Benign;
            ItemUnderTest = new StringMatchValidator(errorSeverity, "interim");
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
            ItemUnderTest = new StringMatchValidator(errorSeverity, "bad input");
            const string input = "exceptional";

            //Act
            var result = ItemUnderTest.IsValid(input);

            //Assert
            Assert.IsFalse(result);
            Assert.AreEqual(errorSeverity, ItemUnderTest.ErrorSeverity);
            Assert.AreEqual(ItemUnderTest.GetMessage(), "[Value!=bad input]");
        }

        [TestMethod]
        public void ValidInputDifferentCaseShouldReturnTrue()
        {
            //Arrange
            const ErrorSeverity errorSeverity = ErrorSeverity.Benign;
            ItemUnderTest = new StringMatchValidator(errorSeverity, "interim");
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
            ItemUnderTest = new StringMatchValidator(errorSeverity, new List<string>());
            const string input = "interim";

            //Act
            var result = ItemUnderTest.IsValid(input);

            //Assert
            Assert.IsFalse(result);
            Assert.AreEqual(errorSeverity, ItemUnderTest.ErrorSeverity);
        }

        [TestMethod]
        public void InvalidValueTypeShouldReturnFalse()
        {
            //Arrange
            const ErrorSeverity errorSeverity = ErrorSeverity.Benign;
            ItemUnderTest = new StringMatchValidator(errorSeverity, "1");
            const int input = 1;

            //Act
            var result = ItemUnderTest.IsValid(input);

            //Assert
            Assert.IsFalse(result);
            Assert.AreEqual(errorSeverity, ItemUnderTest.ErrorSeverity);
        }
    }
}