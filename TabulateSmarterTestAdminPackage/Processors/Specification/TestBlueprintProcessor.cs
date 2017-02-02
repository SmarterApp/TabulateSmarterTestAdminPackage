﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.XPath;

namespace TabulateSmarterTestAdminPackage.Processors.Specification
{
    internal class TestBlueprintProcessor : Processor
    {
        internal TestBlueprintProcessor(XPathNavigator navigator)
        {
            var blueprintElements = navigator.Select("bpelement");
            foreach (XPathNavigator bpelement in blueprintElements)
            {
                ((IList)BlueprintElementProcessors).Add(new BlueprintElementProcessor(bpelement));
            }
        }

        private IEnumerable<BlueprintElementProcessor> BlueprintElementProcessors { get; }

        public override bool Process()
        {
            return BlueprintElementProcessors.All(x => x.Process());
        }
    }
}