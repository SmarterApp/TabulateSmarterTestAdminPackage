using System.Xml.XPath;
using ProcessSmarterTestPackage.Processors.Common;
using ProcessSmarterTestPackage.Processors.Common.ItemPool;
using ProcessSmarterTestPackage.Processors.Common.TestBlueprint;
using ProcessSmarterTestPackage.Processors.Common.TestForm;
using ProcessSmarterTestPackage.Processors.Scoring.PerformanceLevels;
using ProcessSmarterTestPackage.Processors.Scoring.ScoringRules;
using TabulateSmarterTestPackage.Common.RestrictedValues.Enums;
using TabulateSmarterTestPackage.Common.Utilities;

namespace ProcessSmarterTestPackage.Processors.Scoring
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