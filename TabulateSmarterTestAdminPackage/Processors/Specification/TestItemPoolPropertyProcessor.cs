using System.Xml.XPath;

namespace TabulateSmarterTestAdminPackage.Processors.Specification
{
    internal class TestItemPoolPropertyProcessor : PoolPropertyProcessor
    {
        public TestItemPoolPropertyProcessor(XPathNavigator navigator) : base(navigator) {}

        internal new bool Process()
        {
            return IsValidProperty()
                   && IsValidLabel()
                   && IsValidValue();
        }
    }
}