using System.Xml.XPath;
using SmarterTestPackage.Common.Data;
using SmarterTestPackage.Common.Extensions;
using ValidateSmarterTestPackage;
using ValidateSmarterTestPackage.RestrictedValues.Enums;
using ValidateSmarterTestPackage.RestrictedValues.RestrictedList;
using ValidateSmarterTestPackage.Validators;
using ValidateSmarterTestPackage.Validators.Convenience;

namespace ProcessSmarterTestPackage.Processors.Common.ItemPool.TestItem
{
    public class ItemScoredDimensionProcessor : Processor
    {
        public ItemScoredDimensionProcessor(XPathNavigator navigator, PackageType packageType)
            : base(navigator, packageType)
        {
            Attributes = new AttributeValidationDictionary
            {
                {
                    "measurementmodel", StringValidator.IsValidNonEmptyWithLength(100)
                        .AddAndReturn(new RequiredEnumValidator(ErrorSeverity.Degraded,
                            RestrictedList.RestrictedLists[RestrictedListItems.MeasurementModel]))
                },
                {
                    "scorepoints", IntValidator.IsValidPositiveNonEmptyWithLength(10)
                },
                {
                    "weight", DecimalValidator.IsValidPositiveNonEmptyWithLength(30)
                },
                {
                    "dimension", StringValidator.IsValidOptionalNonEmptyWithLength(200)
                }
            };
            Navigator.GenerateList("itemscoreparameter")
                .ForEach(x => Processors.Add(new ItemScoreParameterProcessor(x, packageType)));
        }
    }
}