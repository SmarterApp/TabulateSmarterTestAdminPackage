using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.RestrictedValues.Enums;
using TabulateSmarterTestAdminPackage.Common.Utilities;
using TabulateSmarterTestAdminPackage.Common.Validators.Convenience;
using TabulateSmarterTestPackage.Processors.Common;

namespace TabulateSmarterTestPackage.Processors.Scoring.ScoringRules
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