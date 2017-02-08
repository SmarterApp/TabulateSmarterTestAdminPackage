using System;
using System.Xml.XPath;

namespace TabulateSmarterTestAdminPackage.Processors.Specification.TestSpecification.Administration.TestForm
{
    internal class TestFormProcessor : Processor
    {
        private readonly XPathNavigator _navigator;

        internal TestFormProcessor(XPathNavigator navigator)
        {
            _navigator = navigator;
        }

        public override bool Process()
        {
            throw new NotImplementedException();
        }
    }
}