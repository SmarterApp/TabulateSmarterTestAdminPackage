using System.Collections.Generic;
using System.Linq;
using System.Xml.XPath;

namespace TabulateSmarterTestAdminPackage.Processors.Specification.TestSpecification.Administration.AdminSegment
{
    internal class SegmentBlueprintProcessor : Processor
    {
        internal SegmentBlueprintProcessor(XPathNavigator navigator)
        {
            SegmentBlueprintElementProcessors = new List<SegmentBlueprintElementProcessor>();
            var blueprintElements = navigator.Select("segmentbpelement");
            foreach (XPathNavigator blueprintElement in blueprintElements)
            {
                SegmentBlueprintElementProcessors.Add(new SegmentBlueprintElementProcessor(blueprintElement));
            }
        }

        private IList<SegmentBlueprintElementProcessor> SegmentBlueprintElementProcessors { get; }

        public override bool Process()
        {
            return SegmentBlueprintElementProcessors.All(x => x.Process());
        }
    }
}