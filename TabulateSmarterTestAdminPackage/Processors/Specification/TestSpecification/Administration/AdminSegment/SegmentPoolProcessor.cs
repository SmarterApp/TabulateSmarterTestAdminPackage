using System;
using System.Xml.XPath;

namespace TabulateSmarterTestAdminPackage.Processors.Specification.TestSpecification.Administration.AdminSegment
{
    internal class SegmentPoolProcessor : Processor
    {
        private readonly XPathNavigator _navigator;

        internal SegmentPoolProcessor(XPathNavigator navigator)
        {
            _navigator = navigator;
        }

        public override bool Process()
        {
            throw new NotImplementedException();
        }
    }
}