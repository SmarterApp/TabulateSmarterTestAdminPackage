using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.RestrictedValues.Enums;
using TabulateSmarterTestAdminPackage.Common.Utilities;
using TabulateSmarterTestAdminPackage.Common.Validators.Convenience;

namespace TabulateSmarterTestAdminPackage.Common.Processors
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