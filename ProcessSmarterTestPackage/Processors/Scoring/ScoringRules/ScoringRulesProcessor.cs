using System.Xml.XPath;
using ProcessSmarterTestPackage.Processors.Common;
using TabulateSmarterTestPackage.Common.RestrictedValues.Enums;
using TabulateSmarterTestPackage.Common.Utilities;

namespace ProcessSmarterTestPackage.Processors.Scoring.ScoringRules
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