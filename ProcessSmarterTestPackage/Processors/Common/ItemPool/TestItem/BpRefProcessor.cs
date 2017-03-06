using System.Xml.XPath;
using ValidateSmarterTestPackage;
using ValidateSmarterTestPackage.RestrictedValues.Enums;
using ValidateSmarterTestPackage.Validators.Convenience;

namespace ProcessSmarterTestPackage.Processors.Common.ItemPool.TestItem
{
    public class BpRefProcessor : Processor
    {
        public BpRefProcessor(XPathNavigator navigator, PackageType packageType) : base(navigator, packageType) {}

        // This processor is a special case because the value is in the element instead of an attribute
        public new bool Process()
        {
            var validators = StringValidator.IsValidNonEmptyWithLength(150);
            var bpref = Navigator.Value;

            ValidatedAttributes.Add("bpref", new ValidatedAttribute
            {
                IsValid = validators.IsValid(bpref),
                Name = "bpref",
                Value = Navigator.Value,
                Validator = validators
            });

            return ValidatedAttributes["bpref"].IsValid;
        }
    }
}