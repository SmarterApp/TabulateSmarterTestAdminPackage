using System.Collections.Generic;
using System.Linq;
using System.Xml.XPath;
using ProcessSmarterTestPackage.PostProcessors;
using ProcessSmarterTestPackage.Processors.Common.ItemPool.Passage;
using ProcessSmarterTestPackage.Processors.Common.ItemPool.TestItem;
using SmarterTestPackage.Common.Data;
using SmarterTestPackage.Common.Extensions;

namespace ProcessSmarterTestPackage.Processors.Common.ItemPool
{
    public class ItemPoolProcessor : Processor
    {
        public ItemPoolProcessor(XPathNavigator navigator, PackageType packageType) : base(navigator, packageType)
        {
            Navigator.GenerateList("passage").ForEach(x => Processors.Add(new PassageProcessor(x, packageType)));
            Navigator.GenerateList("testitem").ForEach(x => Processors.Add(new TestItemProcessor(x, packageType)));
        }

        public override List<ValidationError> AdditionalValidations()
        {
            return new ItemPoolPostProcessor(PackageType, this).GenerateErrors().ToList();
        }
    }
}