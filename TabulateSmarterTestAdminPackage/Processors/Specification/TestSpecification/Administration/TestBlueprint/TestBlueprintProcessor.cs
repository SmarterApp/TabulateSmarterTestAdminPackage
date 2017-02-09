using System.Collections.Generic;
using System.Linq;
using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.Processors;

namespace TabulateSmarterTestAdminPackage.Processors.Specification.TestSpecification.Administration.TestBlueprint
{
    internal class TestBlueprintProcessor : Processor
    {
        internal TestBlueprintProcessor(XPathNavigator navigator) : base(navigator)
        {
            BlueprintElementProcessors = new List<BlueprintElementProcessor>();
            var blueprintElements = navigator.Select("bpelement");
            foreach (XPathNavigator bpelement in blueprintElements)
            {
                BlueprintElementProcessors.Add(new BlueprintElementProcessor(bpelement));
            }
        }

        private IList<BlueprintElementProcessor> BlueprintElementProcessors { get; }

        public override bool Process()
        {
            return BlueprintElementProcessors.All(x => x.Process());
        }
    }
}