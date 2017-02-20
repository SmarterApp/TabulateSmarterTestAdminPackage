using System.Xml.XPath;
using TabulateSmarterTestPackage.Common.RestrictedValues.Enums;
using TabulateSmarterTestPackage.Common.Utilities;
using TabulateSmarterTestPackage.Common.Validators.Convenience;
using TabulateSmarterTestPackage.Processors.Common;

namespace TabulateSmarterTestPackage.Processors.Scoring.PerformanceLevels
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