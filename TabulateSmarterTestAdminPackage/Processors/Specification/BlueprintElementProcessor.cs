using System;
using System.Xml.XPath;

namespace TabulateSmarterTestAdminPackage.Processors.Specification
{
    internal class BlueprintElementProcessor : Processor
    {
        private XPathNavigator _navigator;

        internal BlueprintElementProcessor(XPathNavigator navigator)
        {
            _navigator = navigator;
        }

        public override bool Process()
        {
            throw new NotImplementedException();
        }
    }
}