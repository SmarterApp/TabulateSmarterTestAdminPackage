using System.Collections.Generic;
using System.Linq;
using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Processors.Specification.TestSpecification.Administration.ItemPool.Passage;
using TabulateSmarterTestAdminPackage.Processors.Specification.TestSpecification.Administration.ItemPool.TestItem;

namespace TabulateSmarterTestAdminPackage.Processors.Specification.TestSpecification.Administration.ItemPool
{
    internal class ItemPoolProcessor : Processor
    {
        internal ItemPoolProcessor(XPathNavigator navigator) : base(navigator)
        {
            PassageProcessors = new List<PassageProcessor>();
            var passages = navigator.Select("passage");
            foreach (XPathNavigator passage in passages)
            {
                PassageProcessors.Add(new PassageProcessor(passage));
            }

            TestItemProcessors = new List<TestItemProcessor>();
            var testItems = navigator.Select("testitem");
            foreach (XPathNavigator testItem in testItems)
            {
                TestItemProcessors.Add(new TestItemProcessor(testItem));
            }
        }

        private IList<PassageProcessor> PassageProcessors { get; }
        private IList<TestItemProcessor> TestItemProcessors { get; }

        public override bool Process()
        {
            return PassageProcessors.All(x => x.Process())
                   && TestItemProcessors.All(x => x.Process());
        }
    }
}