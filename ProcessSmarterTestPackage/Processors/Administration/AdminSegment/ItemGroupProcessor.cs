using System.Xml.XPath;
using ProcessSmarterTestPackage.Processors.Common;
using SmarterTestPackage.Common.Data;
using SmarterTestPackage.Common.Extensions;
using ValidateSmarterTestPackage;
using ValidateSmarterTestPackage.Validators.Convenience;

namespace ProcessSmarterTestPackage.Processors.Administration.AdminSegment
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
                },
                {
                    "formposition", IntValidator.IsValidNonEmptyWithLengthAndMinValue(10, 1)
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