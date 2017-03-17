using System.Xml.XPath;
using ProcessSmarterTestPackage.Processors.Common;
using SmarterTestPackage.Common.Data;
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
                    "index", new NoValidator(ErrorSeverity.Benign)
                },
                {
                    "value", StringValidator.IsValidNonEmptyWithLength(256)
                }
            };
        }
    }
}