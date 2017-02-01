using System.Xml.XPath;

namespace TabulateSmarterTestAdminPackage.Processors.Specification
{
    internal class PoolPropertyProcessor : Processor
    {
        private static readonly XPathExpression sXp_Property = XPathExpression.Compile("@property");
        private static readonly XPathExpression sXp_Value = XPathExpression.Compile("@value");
        private static readonly XPathExpression sXp_Label = XPathExpression.Compile("@label");
        private static readonly XPathExpression sXp_ItemCount = XPathExpression.Compile("@itemcount");

        private readonly XPathNavigator _navigator;

        internal PoolPropertyProcessor(XPathNavigator navigator)
        {
            _navigator = navigator;
        }

        private string Property { get; set; }
        private string Value { get; set; }
        private string Label { get; set; }
        private string ItemCount { get; set; }

        public void Dispose()
        {}

        public override bool Process()
        {
            return false;
        }
    }
}