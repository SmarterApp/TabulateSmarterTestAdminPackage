using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.Processors;
using TabulateSmarterTestAdminPackage.Common.RestrictedValues.Enums;
using TabulateSmarterTestAdminPackage.Common.Utilities;
using TabulateSmarterTestAdminPackage.Common.Validators.Convenience;

namespace TabulateSmarterTestPackage.Processors.TestSpecification.Scoring
{
    public class PerformanceLevelProcessor : Processor
    {
        public PerformanceLevelProcessor(XPathNavigator navigator, PackageType packageType)
            : base(navigator, packageType)
        {
            Attributes = new AttributeValidationDictionary
            {
                {
                    "bpelementid", StringValidator.IsValidNonEmptyWithLength(150)
                },
                {
                    "plevel", IntValidator.IsValidNonEmptyWithLengthAndMinValue(10, 1)
                },
                {
                    "scaledlo", DecimalValidator.IsValidPositiveNonEmptyWithLength(30)
                },
                {
                    "scaledhi", DecimalValidator.IsValidPositiveNonEmptyWithLength(30)
                }
            };
        }
    }
}