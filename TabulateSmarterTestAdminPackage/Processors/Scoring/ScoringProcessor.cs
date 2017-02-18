using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.RestrictedValues.Enums;
using TabulateSmarterTestAdminPackage.Common.Utilities;
using TabulateSmarterTestPackage.Processors.Common;
using TabulateSmarterTestPackage.Processors.Common.ItemPool;
using TabulateSmarterTestPackage.Processors.Common.TestBlueprint;
using TabulateSmarterTestPackage.Processors.Common.TestForm;
using TabulateSmarterTestPackage.Processors.Scoring.PerformanceLevels;
using TabulateSmarterTestPackage.Processors.Scoring.ScoringRules;

namespace TabulateSmarterTestPackage.Processors.Scoring
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