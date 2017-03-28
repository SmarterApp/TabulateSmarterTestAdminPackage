using System.Xml.XPath;
using SmarterTestPackage.Common.Data;
using ValidateSmarterTestPackage;
using ValidateSmarterTestPackage.RestrictedValues.Enums;
using ValidateSmarterTestPackage.Validators;
using ValidateSmarterTestPackage.Validators.Convenience;

namespace ProcessSmarterTestPackage.Processors.Common.ItemPool.TestItem
{
    public class ItemScoreParameterProcessor : Processor
    {
        public ItemScoreParameterProcessor(XPathNavigator navigator, PackageType packageType)
            : base(navigator, packageType)
        {
            Attributes = new AttributeValidationDictionary
            {
                {
                    "measurementparameter", StringValidator.IsValidNonEmptyWithLength(50)
                        .AddAndReturn(new RequiredEnumValidator(ErrorSeverity.Degraded,
                            RestrictedListItems.MeasurementParameter))
                },
                {
                    "value", new ValidatorCollection
                    {
                        new RequiredDoubleValidator(ErrorSeverity.Degraded),
                        new MaxLengthValidator(ErrorSeverity.Degraded, 30)
                    }
                }
            };
        }
    }
}