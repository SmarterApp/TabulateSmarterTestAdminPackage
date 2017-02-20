using System.Xml.XPath;
using TabulateSmarterTestPackage.Common.RestrictedValues.Enums;
using TabulateSmarterTestPackage.Common.Utilities;
using TabulateSmarterTestPackage.Common.Validators;
using TabulateSmarterTestPackage.Common.Validators.Convenience;
using TabulateSmarterTestPackage.Processors.Common;

namespace TabulateSmarterTestPackage.Processors.Administration
{
    public class PoolPropertyProcessor : Processor
    {
        public PoolPropertyProcessor(XPathNavigator navigator, PackageType packageType) : base(navigator, packageType)
        {
            Attributes = new AttributeValidationDictionary
            {
                {
                    "property", StringValidator.IsValidNonEmptyWithLength(50)
                        .AddAndReturn(new RequiredEnumValidator(ErrorSeverity.Degraded,
                            RestrictedListItems.PoolPropertyName))
                },
                {
                    "value", StringValidator.IsValidNonEmptyWithLength(128)
                },
                {
                    "label", StringValidator.IsValidNonEmptyWithLength(150)
                },
                {
                    "itemcount", IntValidator.IsValidPositiveNonEmptyWithLength(10)
                }
            };
        }
    }
}