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

        public IList<KeyValuePair<string, ValidatedAttribute>> Validate(XPathNavigator navigator)
        {
            return this.Select(
                x => new KeyValuePair<string, ValidatedAttribute>(
                    x.Key,
                    new ValidatedAttribute
                    {
                        IsValid = x.Value.IsValid(navigator.Eval(XPathExpression.Compile($"@{x.Key}"))),
                        Value = navigator.Eval(XPathExpression.Compile($"@{x.Key}"))
                    })).ToList();
        }
    }
}