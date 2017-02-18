using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.RestrictedValues.Enums;
using TabulateSmarterTestAdminPackage.Common.RestrictedValues.RestrictedList;
using TabulateSmarterTestAdminPackage.Common.Utilities;
using TabulateSmarterTestAdminPackage.Common.Validators;
using TabulateSmarterTestAdminPackage.Common.Validators.Convenience;

namespace TabulateSmarterTestPackage.Processors.Common.ItemPool.TestItem
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