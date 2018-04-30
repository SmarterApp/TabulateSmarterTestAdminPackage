using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ValidateSmarterTestPackage.Validators.Combined
{
    public interface ITestPackageValidator
    {
        void Validate(object testPackage, object errors);
    }
}
