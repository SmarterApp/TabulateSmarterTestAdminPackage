using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.Processors;
using TabulateSmarterTestAdminPackage.Common.RestrictedValues.Enums;

namespace TabulateSmarterTestPackage.Processors.TestSpecification.Scoring
{
    public class ScoringProcessor : Processor
    {
        public ScoringProcessor(XPathNavigator navigator, PackageType packageType) : base(navigator, packageType) {}
    }
}