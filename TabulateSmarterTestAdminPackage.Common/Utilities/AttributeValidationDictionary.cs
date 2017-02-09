using System.Collections.Generic;
using System.Linq;
using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.Enums;
using TabulateSmarterTestAdminPackage.Common.Validators;

namespace TabulateSmarterTestAdminPackage.Common.Utilities
{
    public class AttributeValidationDictionary : Dictionary<string, IValidator>
    {
        public PackageType PackageType { get; set; }

        public IDictionary<string, ValidatedAttribute> Validate(XPathNavigator navigator)
        {
            return this.Select(
                x => 
                    new ValidatedAttribute
                    {
                        Name = x.Key,
                        IsValid = x.Value.IsValid(navigator.Eval(XPathExpression.Compile($"@{x.Key}"))),
                        Value = navigator.Eval(XPathExpression.Compile($"@{x.Key}"))
                    }).ToDictionary(x => x.Name);
        }
    }
}