using System.Xml.XPath;
using ProcessSmarterTestPackage.Processors.Common;
using ValidateSmarterTestPackage;
using ValidateSmarterTestPackage.RestrictedValues.Enums;
using ValidateSmarterTestPackage.Validators.Convenience;

namespace ProcessSmarterTestPackage.Processors.Scoring.PerformanceLevels
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