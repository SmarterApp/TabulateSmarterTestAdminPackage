using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.RestrictedValues.Enums;
using TabulateSmarterTestAdminPackage.Common.Utilities;

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