using System.Xml.XPath;
using ProcessSmarterTestPackage.Processors.Common;
using SmarterTestPackage.Common.Extensions;
using ValidateSmarterTestPackage.RestrictedValues.Enums;

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