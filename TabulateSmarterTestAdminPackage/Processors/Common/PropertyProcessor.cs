using System.Xml.XPath;
using TabulateSmarterTestPackage.Common.RestrictedValues.Enums;
using TabulateSmarterTestPackage.Common.Utilities;
using TabulateSmarterTestPackage.Common.Validators.Convenience;

namespace TabulateSmarterTestPackage.Processors.Common
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