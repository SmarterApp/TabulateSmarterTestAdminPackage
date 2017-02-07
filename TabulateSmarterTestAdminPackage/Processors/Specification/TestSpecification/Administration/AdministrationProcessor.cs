using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Processors.Specification.TestSpecification.Administration.ItemPool;
using TabulateSmarterTestAdminPackage.Processors.Specification.TestSpecification.Administration.TestBlueprint;

namespace TabulateSmarterTestAdminPackage.Processors.Specification.TestSpecification.Administration
{
    internal class AdministrationProcessor : Processor
    {
        internal AdministrationProcessor(XPathNavigator navigator)
        {
            TestBlueprintProcessor = new TestBlueprintProcessor(navigator.SelectSingleNode("testblueprint"));

            PoolPropertyProcessors = new List<PoolPropertyProcessor>();
            var poolProperties = navigator.Select("poolproperty");
            foreach (XPathNavigator poolProperty in poolProperties)
            {
                ((IList)PoolPropertyProcessors).Add(new PropertyProcessor(poolProperty));
            }

            ItemPoolProcessor = new ItemPoolProcessor(navigator.SelectSingleNode("itempool"));

            AdminSegmentProcessors = new List<AdminSegmentProcessor>();
            var adminSegments = navigator.Select("adminsegment");
            foreach (XPathNavigator adminSegment in adminSegments)
            {
                ((IList)AdminSegmentProcessors).Add(new AdminSegmentProcessor(adminSegment));
            }
        }

        private TestBlueprintProcessor TestBlueprintProcessor { get; }
        private IEnumerable<PoolPropertyProcessor> PoolPropertyProcessors { get; }
        private ItemPoolProcessor ItemPoolProcessor { get; }
        private IEnumerable<AdminSegmentProcessor> AdminSegmentProcessors { get; }

        public override bool Process()
        {
            return TestBlueprintProcessor.Process()
                   && PoolPropertyProcessors.All(x => x.Process())
                   && ItemPoolProcessor.Process()
                   && AdminSegmentProcessors.All(x => x.Process());
        }
    }
}