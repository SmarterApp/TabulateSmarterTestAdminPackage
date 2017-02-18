using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.RestrictedValues.Enums;
using TabulateSmarterTestAdminPackage.Common.Utilities;
using TabulateSmarterTestPackage.Processors.Administration.AdminSegment;
using TabulateSmarterTestPackage.Processors.Common;
using TabulateSmarterTestPackage.Processors.Common.ItemPool;
using TabulateSmarterTestPackage.Processors.Common.TestBlueprint;
using TabulateSmarterTestPackage.Processors.Common.TestForm;

namespace TabulateSmarterTestPackage.Processors.Administration
{
    public class AdministrationProcessor : Processor
    {
        public AdministrationProcessor(XPathNavigator navigator, PackageType packageType)
            : base(navigator, packageType)
        {
            Navigator.GenerateList("testblueprint")
                .ForEach(x => Processors.Add(new TestBlueprintProcessor(x, packageType)));
            Navigator.GenerateList("poolproperty")
                .ForEach(x => Processors.Add(new PoolPropertyProcessor(x, packageType)));
            Navigator.GenerateList("itempool").ForEach(x => Processors.Add(new ItemPoolProcessor(x, packageType)));
            Navigator.GenerateList("adminsegment")
                .ForEach(x => Processors.Add(new AdminSegmentProcessor(x, packageType)));
            Navigator.GenerateList("testform").ForEach(x => Processors.Add(new TestFormProcessor(x, packageType)));
        }
    }
}