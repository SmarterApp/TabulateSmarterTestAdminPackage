using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.Processors;
using TabulateSmarterTestAdminPackage.Common.RestrictedValues.Enums;
using TabulateSmarterTestAdminPackage.Common.Utilities;
using TabulateSmarterTestPackage.Processors.TestSpecification.Administration.ItemPool;
using TabulateSmarterTestPackage.Processors.TestSpecification.Administration.TestBlueprint;
using TabulateSmarterTestPackage.Processors.TestSpecification.Administration.TestForm;

namespace TabulateSmarterTestPackage.Processors.TestSpecification.Scoring
{
    public class ScoringProcessor : Processor
    {
        public ScoringProcessor(XPathNavigator navigator, PackageType packageType) : base(navigator, packageType)
        {
            Navigator.GenerateList("testblueprint")
                .ForEach(x => Processors.Add(new TestBlueprintProcessor(x, packageType)));
            Navigator.GenerateList("performancelevels")
                .ForEach(x => Processors.Add(new PerformanceLevelsProcessor(x, packageType)));
            Navigator.GenerateList("itempool").ForEach(x => Processors.Add(new ItemPoolProcessor(x, packageType)));
            Navigator.GenerateList("scoringrules")
                .ForEach(x => Processors.Add(new ScoringRulesProcessor(x, packageType)));
            Navigator.GenerateList("testform").ForEach(x => Processors.Add(new TestFormProcessor(x, packageType)));
        }
    }
}