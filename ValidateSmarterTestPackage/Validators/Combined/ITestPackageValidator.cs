using System.Collections.Generic;
using SmarterTestPackage.Common.Data;
using ValidateSmarterTestPackage.Resources;

namespace ValidateSmarterTestPackage.Validators.Combined
{
    public interface ITestPackageValidator
    {
        void Validate(TestPackage testPackage, List<ValidationError> errors);
    }
}
