using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.Processors;
using TabulateSmarterTestAdminPackage.Common.RestrictedValues.Enums;
using TabulateSmarterTestAdminPackage.Common.Utilities;

namespace TabulateSmarterTestPackage.Processors.TestSpecification.Scoring.ScoringRules
{
    public class ScoringRulesProcessor : Processor
    {
        public ScoringRulesProcessor(XPathNavigator navigator, PackageType packageType) : base(navigator, packageType)
        {
            Navigator.GenerateList("computationrule")
                .ForEach(x => Processors.Add(new ComputationRuleProcessor(x, packageType)));
        }
    }
}