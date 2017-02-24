using System.Xml.XPath;
using ProcessSmarterTestPackage.Processors.Common;
using TabulateSmarterTestPackage.Common.RestrictedValues.Enums;
using ValidateSmarterTestPackage;
using ValidateSmarterTestPackage.Validators.Convenience;

namespace ProcessSmarterTestPackage.Processors.Scoring.ScoringRules
{
    public class ComputationRuleParameterValueProcessor : Processor
    {
        public ComputationRuleParameterValueProcessor(XPathNavigator navigator, PackageType packageType)
            : base(navigator, packageType)
        {
            Attributes = new AttributeValidationDictionary
            {
                {
                    "index", IntValidator.IsValidPositiveNonEmptyWithLength(10)
                },
                {
                    "value", StringValidator.IsValidNonEmptyWithLength(256)
                }
            };
        }
    }
}