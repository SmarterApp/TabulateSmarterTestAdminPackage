using System.Xml.XPath;
using TabulateSmarterTestPackage.Common.RestrictedValues.Enums;
using TabulateSmarterTestPackage.Common.Utilities;
using TabulateSmarterTestPackage.Common.Validators.Convenience;
using TabulateSmarterTestPackage.Processors.Common;

namespace TabulateSmarterTestPackage.Processors.Administration.AdminSegment
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