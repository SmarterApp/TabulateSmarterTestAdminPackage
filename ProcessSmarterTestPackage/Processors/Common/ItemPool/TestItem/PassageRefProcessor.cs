using System.Xml.XPath;
using SmarterTestPackage.Common.Data;
using ValidateSmarterTestPackage;
using ValidateSmarterTestPackage.Validators.Convenience;

namespace ProcessSmarterTestPackage.Processors.Common.ItemPool.TestItem
{
    public class PassageRefProcessor : Processor
    {
        public PassageRefProcessor(XPathNavigator navigator, PackageType packageType) : base(navigator, packageType) {}

        // This processor is a special case because the value is in the element instead of an attribute
        public override bool Process()
        {
            var validators =
                StringValidator.IsValidNonEmptyWithLength(100);
            var passageref = Navigator.Value;

            ValidatedAttributes.Add("passageref", new ValidatedAttribute
            {
                IsValid = validators.IsValid(passageref),
                Name = "passageref",
                Value = Navigator.Value,
                Validator = validators
            });
            return ValidatedAttributes["passageref"].IsValid;
        }
    }
}