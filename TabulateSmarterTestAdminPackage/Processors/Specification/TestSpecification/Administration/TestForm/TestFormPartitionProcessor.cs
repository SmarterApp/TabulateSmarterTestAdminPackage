using System.Collections.Generic;
using System.Linq;
using System.Xml.XPath;

namespace TabulateSmarterTestAdminPackage.Processors.Specification.TestSpecification.Administration.TestForm
{
    internal class TestFormPartitionProcessor : Processor
    {
        internal TestFormPartitionProcessor(XPathNavigator navigator) : base(navigator)
        {
            TestFormPartitionIdentifierProcessor =
                new TestFormPartitionIdentifierProcessor(navigator.SelectSingleNode("identifier"));

            TestFormPartitionItemGroupProcessors = new List<TestFormPartitionItemGroupProcessor>();
            var itemGroups = navigator.Select("itemgroup");
            foreach (XPathNavigator itemGroup in itemGroups)
            {
                TestFormPartitionItemGroupProcessors.Add(new TestFormPartitionItemGroupProcessor(itemGroup));
            }
        }

        private TestFormPartitionIdentifierProcessor TestFormPartitionIdentifierProcessor { get; }
        private IList<TestFormPartitionItemGroupProcessor> TestFormPartitionItemGroupProcessors { get; }

        public override bool Process()
        {
            return TestFormPartitionIdentifierProcessor.Process()
                   && TestFormPartitionItemGroupProcessors.All(x => x.Process());
        }
    }
}