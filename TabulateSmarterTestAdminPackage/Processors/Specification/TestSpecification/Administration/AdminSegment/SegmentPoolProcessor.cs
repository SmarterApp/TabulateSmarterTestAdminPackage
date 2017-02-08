using System.Collections.Generic;
using System.Linq;
using System.Xml.XPath;

namespace TabulateSmarterTestAdminPackage.Processors.Specification.TestSpecification.Administration.AdminSegment
{
    internal class SegmentPoolProcessor : Processor
    {
        internal SegmentPoolProcessor(XPathNavigator navigator)
        {
            SegmentPoolItemGroupProcessors = new List<SegmentPoolItemGroupProcessor>();
            var itemGroups = navigator.Select("itemgroup");
            foreach (XPathNavigator itemGroup in itemGroups)
            {
                SegmentPoolItemGroupProcessors.Add(new SegmentPoolItemGroupProcessor(itemGroup));
            }
        }

        private IList<SegmentPoolItemGroupProcessor> SegmentPoolItemGroupProcessors { get; }

        public override bool Process()
        {
            return SegmentPoolItemGroupProcessors.All(x => x.Process());
        }
    }
}