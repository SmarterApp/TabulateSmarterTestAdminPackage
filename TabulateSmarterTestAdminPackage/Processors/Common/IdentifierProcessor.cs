using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.RestrictedValues.Enums;
using TabulateSmarterTestAdminPackage.Common.Utilities;
using TabulateSmarterTestAdminPackage.Common.Validators.Convenience;

namespace TabulateSmarterTestPackage.Processors.Common
{
    public class IdentifierProcessor : Processor
    {
        public IdentifierProcessor(XPathNavigator navigator, PackageType packageType) : base(navigator, packageType)
        {
            Attributes = new AttributeValidationDictionary
            {
                {
                    "name", StringValidator.IsValidNonEmptyWithLength(200)
                },
                {
                    "version", IntValidator.IsValidPositiveNonEmptyWithLength(10)
                },
                {
                    "uniqueid", StringValidator.IsValidNonEmptyWithLength(250)
                },
                {
                    "label", StringValidator.IsValidNonEmptyWithLength(200)
                }
            };
        }
    }
}