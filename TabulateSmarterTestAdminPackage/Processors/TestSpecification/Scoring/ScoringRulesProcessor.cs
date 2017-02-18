using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.Processors;
using TabulateSmarterTestAdminPackage.Common.RestrictedValues.Enums;

namespace TabulateSmarterTestPackage.Processors.TestSpecification.Scoring
{
    public class ScoringRulesProcessor : Processor
    {
        public ScoringRulesProcessor(XPathNavigator navigator, PackageType packageType) : base(navigator, packageType) {}
    }
}