using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.Enums;
using TabulateSmarterTestAdminPackage.Common.Processors;
using TabulateSmarterTestAdminPackage.Common.Utilities;
using TabulateSmarterTestAdminPackage.Processors.Specification.TestSpecification.Administration.ItemPool.Passage;
using TabulateSmarterTestAdminPackage.Processors.Specification.TestSpecification.Administration.ItemPool.TestItem;

namespace TabulateSmarterTestAdminPackage.Processors.Specification.TestSpecification.Administration.ItemPool
{
    public class ItemPoolProcessor : Processor
    {
        public ItemPoolProcessor(XPathNavigator navigator, PackageType packageType) : base(navigator, packageType)
        {
            Navigator.GenerateList("passage").ForEach(x => Processors.Add(new PassageProcessor(x, packageType)));
            Navigator.GenerateList("testitem").ForEach(x => Processors.Add(new TestItemProcessor(x, packageType)));
        }
    }
}