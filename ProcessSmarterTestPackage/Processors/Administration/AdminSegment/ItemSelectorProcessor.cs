using System.Xml.XPath;
using ProcessSmarterTestPackage.Processors.Common;
using SmarterTestPackage.Common.Data;
using SmarterTestPackage.Common.Extensions;
using ValidateSmarterTestPackage;
using ValidateSmarterTestPackage.RestrictedValues.Enums;
using ValidateSmarterTestPackage.Validators;
using ValidateSmarterTestPackage.Validators.Convenience;

namespace ProcessSmarterTestPackage.Processors.Administration.AdminSegment
{
    public class ItemSelectorProcessor : Processor
    {
        public ItemSelectorProcessor(XPathNavigator navigator, PackageType packageType) : base(navigator, packageType)
        {
            Attributes = new AttributeValidationDictionary
            {
                {
                    "type", StringValidator.IsValidNonEmptyWithLength(100)
                        .AddAndReturn(new RequiredEnumValidator(ErrorSeverity.Degraded,
                            RestrictedListItems.ItemSelectionAlgorithm))
                }
            };

            Navigator.GenerateList("identifier").ForEach(x => Processors.Add(new IdentifierProcessor(x, packageType)));
            ReplaceAttributeValidation("identifier", new AttributeValidationDictionary
            {
                {
                    "uniqueid", StringValidator.IsValidNonEmptyWithLength(200)
                },
                {
                    "version", DecimalValidator.IsValidPositiveNonEmptyWithLength(20)
                }
            });

            Navigator.GenerateList("itemselectionparameter")
                .ForEach(x => Processors.Add(new ItemSelectionParameterProcessor(x, packageType)));
        }
    }
}