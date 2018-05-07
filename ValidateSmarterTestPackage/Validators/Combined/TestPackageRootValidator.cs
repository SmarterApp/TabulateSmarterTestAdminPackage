using System;
using System.Collections.Generic;
using NLog;
using SmarterTestPackage.Common.Data;
using ValidateSmarterTestPackage.Resources;

namespace ValidateSmarterTestPackage.Validators.Combined
{
    public class TestPackageRootValidator : ITestPackageValidator
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public void Validate(TestPackage testPackage, List<ValidationError> errors)
        {
            ValidateTestPackageVersionIsLong(testPackage, errors);
        }

        private void ValidateTestPackageVersionIsLong(TestPackage testPackage, List<ValidationError> errors)
        {
            var hasFractionalPart = testPackage.version - Math.Round(testPackage.version) != 0;
            var castFail = false;
            try
            {
                var unused = (long) testPackage.version;
            }
            catch (Exception)
            {
                castFail = true;
            }

            if (hasFractionalPart || castFail)
            {
                var errStr =
                    $"The test package version must be a long or integer value.";
                Logger.Debug(errStr);
                errors.Add(new ValidationError
                {
                    ErrorSeverity = ErrorSeverity.Severe,
                    Location = "TestPackage",
                    GeneratedMessage = errStr,
                    ItemId = testPackage.version.ToString(),
                    Key = "TestPackage",
                    PackageType = PackageType.Combined,
                    Value = testPackage.version.ToString()
                });
            }
        }
    }
}