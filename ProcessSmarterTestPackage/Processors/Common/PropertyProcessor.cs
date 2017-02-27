using System.Xml.XPath;
using ValidateSmarterTestPackage;
using ValidateSmarterTestPackage.RestrictedValues.Enums;
using ValidateSmarterTestPackage.Validators.Convenience;

namespace ProcessSmarterTestPackage.Processors.Common
{
    public class PropertyProcessor : Processor
    {
        public PropertyProcessor(XPathNavigator navigator, PackageType packageType) : base(navigator, packageType)
        {
            Attributes = new AttributeValidationDictionary
            {
                {
                    "name", StringValidator.IsValidNonEmptyWithLength(200)
                },
                {
                    "value", StringValidator.IsValidNonEmptyWithLength(200)
                },
                {
                    "label", StringValidator.IsValidNonEmptyWithLength(200)
                }
            };
        }
    }
}