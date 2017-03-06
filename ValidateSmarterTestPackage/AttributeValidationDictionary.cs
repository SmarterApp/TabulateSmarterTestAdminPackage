using System.Collections.Generic;
using System.Linq;
using System.Xml.XPath;
using SmarterTestPackage.Common.Data;
using SmarterTestPackage.Common.Extensions;
using ValidateSmarterTestPackage.Validators;

namespace ValidateSmarterTestPackage
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
                        IsValid = x.Value.IsValid(navigator.Eval(XPathExpression.Compile($"@{x.Key}"))
                            , x.Value.ErrorSeverity != ErrorSeverity.Benign), // If it's benign, it's an optional field
                        Value = navigator.Eval(XPathExpression.Compile($"@{x.Key}")),
                        Validator = x.Value
                    }).ToDictionary(x => x.Name);
        }

        public IDictionary<string, ValidatedAttribute> ValidateAttribute(XPathNavigator navigator, string attribute)
        {
            return this.Where(x => x.Key.Equals(attribute))
                .Select(
                    x =>
                        new ValidatedAttribute
                        {
                            Name = x.Key,
                            IsValid = x.Value.IsValid(navigator.Eval(XPathExpression.Compile($"@{x.Key}"))),
                            Value = navigator.Eval(XPathExpression.Compile($"@{x.Key}")),
                            Validator = x.Value
                        }).ToDictionary(x => x.Name);
        }
    }
}