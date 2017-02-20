using System.Xml.XPath;
using TabulateSmarterTestPackage.Common.RestrictedValues.Enums;
using TabulateSmarterTestPackage.Common.Utilities;
using TabulateSmarterTestPackage.Common.Validators.Convenience;

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