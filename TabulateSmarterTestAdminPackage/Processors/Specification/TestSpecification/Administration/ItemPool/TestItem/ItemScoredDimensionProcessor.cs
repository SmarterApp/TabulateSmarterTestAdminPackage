using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.Enums;
using TabulateSmarterTestAdminPackage.Common.Processors;
using TabulateSmarterTestAdminPackage.Common.Utilities;
using TabulateSmarterTestAdminPackage.Common.Validators.Convenience;

namespace TabulateSmarterTestAdminPackage.Processors.Specification.TestSpecification.Administration.ItemPool.TestItem
{
    internal class ItemScoredDimensionProcessor : Processor
    {
        internal ItemScoredDimensionProcessor(XPathNavigator navigator, PackageType packageType)
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