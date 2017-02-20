using System.Xml.XPath;
using TabulateSmarterTestPackage.Common.RestrictedValues.Enums;
using TabulateSmarterTestPackage.Common.RestrictedValues.RestrictedList;
using TabulateSmarterTestPackage.Common.Utilities;
using TabulateSmarterTestPackage.Common.Validators;
using TabulateSmarterTestPackage.Common.Validators.Convenience;

namespace TabulateSmarterTestPackage.Processors.Common.ItemPool.TestItem
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
                            RestrictedList.RestrictedLists[RestrictedListItems.MeasurementParameter]))
                },
                {
                    "value", new ValidatorCollection
                    {
                        new RequiredDecimalValidator(ErrorSeverity.Degraded),
                        new MaxLengthValidator(ErrorSeverity.Degraded, 30)
                    }
                }
            };
        }
    }
}