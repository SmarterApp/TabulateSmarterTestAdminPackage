using System.Xml.XPath;
using ProcessSmarterTestPackage.Processors.Common;
using SmarterTestPackage.Common.Data;
using ValidateSmarterTestPackage;
using ValidateSmarterTestPackage.Validators;
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
                    "scaledlo", new ValidatorCollection
                    {
                        new RequiredDoubleValidator(ErrorSeverity.Degraded),
                        new MaxLengthValidator(ErrorSeverity.Degraded, 30)
                    }
                },
                {
                    "scaledhi", new ValidatorCollection
                    {
                        new RequiredDoubleValidator(ErrorSeverity.Degraded),
                        new MaxLengthValidator(ErrorSeverity.Degraded, 30)
                    }
                }
            };
        }
    }
}