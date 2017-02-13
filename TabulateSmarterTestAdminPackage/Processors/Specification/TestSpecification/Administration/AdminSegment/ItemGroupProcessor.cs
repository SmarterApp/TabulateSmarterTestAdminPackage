using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.Enums;
using TabulateSmarterTestAdminPackage.Common.Processors;
using TabulateSmarterTestAdminPackage.Common.Utilities;
using TabulateSmarterTestAdminPackage.Common.Validators.Convenience;

namespace TabulateSmarterTestAdminPackage.Processors.Specification.TestSpecification.Administration.AdminSegment
{
    public class ItemGroupProcessor : Processor
    {
        public ItemGroupProcessor(XPathNavigator navigator, PackageType packageType)
            : base(navigator, packageType)
        {
            Attributes = new AttributeValidationDictionary
            {
                {
                    "maxitems", StringValidator.IsValidNonEmptyWithLength(10)
                },
                {
                    "maxresponses", StringValidator.IsValidNonEmptyWithLength(10)
                }
            };

            Navigator.GenerateList("groupitem")
                .ForEach(x => Processors.Add(new GroupItemProcessor(x, packageType)));

            Navigator.GenerateList("identifier")
                .ForEach(x => Processors.Add(new IdentifierProcessor(x, packageType)));
            ReplaceAttributeValidation("identifier", new AttributeValidationDictionary
            {
                {
                    "uniqueid", StringValidator.IsValidNonEmptyWithLength(200)
                }
            });
            RemoveAttributeValidation("identifier", "label");
        }
    }
}