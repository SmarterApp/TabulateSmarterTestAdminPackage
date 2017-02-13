using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.Enums;
using TabulateSmarterTestAdminPackage.Common.Processors;
using TabulateSmarterTestAdminPackage.Common.Utilities;
using TabulateSmarterTestAdminPackage.Processors.Specification.TestSpecification.Administration.AdminSegment;
using TabulateSmarterTestAdminPackage.Processors.Specification.TestSpecification.Administration.ItemPool;
using TabulateSmarterTestAdminPackage.Processors.Specification.TestSpecification.Administration.TestBlueprint;
using TabulateSmarterTestAdminPackage.Processors.Specification.TestSpecification.Administration.TestForm;

namespace TabulateSmarterTestAdminPackage.Processors.Specification.TestSpecification.Administration
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