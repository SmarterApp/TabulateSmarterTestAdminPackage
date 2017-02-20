using System.Xml.XPath;
using TabulateSmarterTestPackage.Common.RestrictedValues.Enums;
using TabulateSmarterTestPackage.Common.Utilities;

namespace TabulateSmarterTestPackage.Processors.Common.TestBlueprint
{
    public class TestBlueprintProcessor : Processor
    {
        public TestBlueprintProcessor(XPathNavigator navigator, PackageType packageType) : base(navigator, packageType)
        {
            Navigator.GenerateList("bpelement")
                .ForEach(x => Processors.Add(new BlueprintElementProcessor(x, packageType)));
        }
    }
}