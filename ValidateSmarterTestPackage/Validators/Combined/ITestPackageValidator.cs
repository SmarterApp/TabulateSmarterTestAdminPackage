using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmarterTestPackage.Common.Data;
using ValidateSmarterTestPackage.Resources;

namespace ValidateSmarterTestPackage.Validators.Combined
{
    public interface ITestPackageValidator
    {
        void Validate(TestPackage testPackage, List<ValidationError> errors);
    }
}
