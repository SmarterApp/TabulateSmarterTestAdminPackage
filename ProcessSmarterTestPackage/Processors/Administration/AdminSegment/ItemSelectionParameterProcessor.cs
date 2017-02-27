using System.Xml.XPath;
using ProcessSmarterTestPackage.Processors.Common;
using SmarterTestPackage.Common.Extensions;
using ValidateSmarterTestPackage;
using ValidateSmarterTestPackage.RestrictedValues.Enums;
using ValidateSmarterTestPackage.Validators.Convenience;

namespace ProcessSmarterTestPackage.Processors.Administration.AdminSegment
{
    public class ItemSelectionParameterProcessor : Processor
    {
        public ItemSelectionParameterProcessor(XPathNavigator navigator, PackageType packageType)
            : base(navigator, packageType)
        {
            Attributes = new AttributeValidationDictionary
            {
                {
                    "bpelementid", StringValidator.IsValidNonEmptyWithLength(150)
                }
            };

            Navigator.GenerateList("property")
                .ForEach(x => Processors.Add(new PropertyProcessor(x, packageType)));
            ReplaceAttributeValidation("property", new AttributeValidationDictionary
            {
                {
                    "value", StringValidator.IsValidNonEmptyWithLength(100)
                },
                {
                    "label", StringValidator.IsValidNonEmptyWithLength(500)
                }
            });
        }
    }
}