using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.Processors;
using TabulateSmarterTestAdminPackage.Common.RestrictedValues.Enums;
using TabulateSmarterTestAdminPackage.Common.Utilities;
using TabulateSmarterTestAdminPackage.Common.Validators.Convenience;

namespace TabulateSmarterTestPackage.Processors.TestSpecification.Scoring.ScoringRules
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