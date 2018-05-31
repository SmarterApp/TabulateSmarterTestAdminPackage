using System.Collections.Generic;
using SmarterTestPackage.Common.Data;

namespace ValidateSmarterTestPackage.Validators.Combined
{
    public interface ITestPackageValidator
    {
        void Validate(TestPackage testPackage, List<ValidationError> errors);
    }
}
