using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.Enums;
using TabulateSmarterTestAdminPackage.Common.Processors;
using TabulateSmarterTestAdminPackage.Common.Utilities;
using TabulateSmarterTestAdminPackage.Common.Validators.Convenience;

namespace TabulateSmarterTestAdminPackage.Processors.Specification.TestSpecification.Administration
{
    public class PoolPropertyProcessor : Processor
    {
        public PoolPropertyProcessor(XPathNavigator navigator, PackageType packageType) : base(navigator, packageType)
        {
            Attributes = new AttributeValidationDictionary
            {
                {
                    "property", StringValidator.IsValidNonEmptyWithLength(50)
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