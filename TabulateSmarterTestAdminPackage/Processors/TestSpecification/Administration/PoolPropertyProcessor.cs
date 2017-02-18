using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.RestrictedValues.Enums;
using TabulateSmarterTestAdminPackage.Common.RestrictedValues.RestrictedList;
using TabulateSmarterTestAdminPackage.Common.Utilities;
using TabulateSmarterTestAdminPackage.Common.Validators;
using TabulateSmarterTestAdminPackage.Common.Validators.Convenience;
using TabulateSmarterTestPackage.Processors.TestSpecification.Common;

namespace TabulateSmarterTestPackage.Processors.TestSpecification.Administration
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
                            RestrictedList.RestrictedLists[RestrictedListItems.PoolPropertyName]))
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