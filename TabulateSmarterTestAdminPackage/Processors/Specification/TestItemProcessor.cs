using System;
using System.Collections.Generic;
using System.Xml.XPath;

namespace TabulateSmarterTestAdminPackage.Processors.Specification
{
    internal class TestItemProcessor : Processor
    {
        private static readonly XPathExpression sXp_FileName = XPathExpression.Compile("@filename");
        private static readonly XPathExpression sXp_ItemType = XPathExpression.Compile("@itemtype");

        private TestItemIdentifierProcessor TestItemIdentifierProcessor { get; }
        private IList<BPrefProcessor> BPrefProcessors { get; }

        public override bool Process()
        {
            throw new NotImplementedException();
        }
    }
}