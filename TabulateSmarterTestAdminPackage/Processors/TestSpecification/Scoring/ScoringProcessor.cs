using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.RestrictedValues.Enums;
using TabulateSmarterTestAdminPackage.Common.Utilities;
using TabulateSmarterTestPackage.Processors.TestSpecification.Common;
using TabulateSmarterTestPackage.Processors.TestSpecification.Common.ItemPool;
using TabulateSmarterTestPackage.Processors.TestSpecification.Common.TestBlueprint;
using TabulateSmarterTestPackage.Processors.TestSpecification.Common.TestForm;
using TabulateSmarterTestPackage.Processors.TestSpecification.Scoring.PerformanceLevels;
using TabulateSmarterTestPackage.Processors.TestSpecification.Scoring.ScoringRules;

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