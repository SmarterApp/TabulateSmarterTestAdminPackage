using System;
using System.Xml.XPath;

namespace TabulateSmarterTestAdminPackage.Processors.Specification.TestSpecification.Administration
{
    public class AdminSegmentProcessor : Processor
    {
        private static readonly XPathExpression sXp_SegmentId = XPathExpression.Compile("@segmentid");
        private static readonly XPathExpression sXp_Position = XPathExpression.Compile("@position");
        private static readonly XPathExpression sXp_ItemSelection = XPathExpression.Compile("@itemselection");

        private readonly XPathNavigator _navigator;

        internal AdminSegmentProcessor(XPathNavigator navigator)
        {
            _navigator = navigator;
        }

        public override bool Process()
        {
            throw new NotImplementedException();
        }
    }
}