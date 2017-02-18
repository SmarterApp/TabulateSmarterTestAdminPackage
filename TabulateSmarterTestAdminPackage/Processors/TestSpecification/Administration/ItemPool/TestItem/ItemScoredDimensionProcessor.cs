using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.Processors;
using TabulateSmarterTestAdminPackage.Common.RestrictedValues.Enums;
using TabulateSmarterTestAdminPackage.Common.Utilities;
using TabulateSmarterTestAdminPackage.Common.Validators.Convenience;

namespace TabulateSmarterTestPackage.Processors.TestSpecification.Administration.ItemPool.TestItem
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