using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.Generic;
using TabulateSmarterTestAdminPackage.Processors.Specification;
using TabulateSmarterTestAdminPackage.Utility;

namespace TabulateSmarterTestAdminPackage.Processors.Loader
{
    internal class TestSpecificationErrorProcessor : TestSpecificationProcessor
    {
        internal TestSpecificationErrorProcessor(XPathNavigator navigator) : base(navigator){}

        internal new bool IsValidPublisher()
        {
            Publisher = Navigator.Eval(sXp_Publisher);
            // In this case, the loader's enforcement is stricter than the spec. If it passes loader enforcement, it's good.
            if (Publisher.NonemptyStringLessThanEqual(100))
            {
                return true;
            }
            AdminPackageUtility.ReportLoadError(Navigator.NamespaceURI, sXp_Publisher.Expression, "string required [length<=100]");
            // If it fails loader enforcement we're goingto check if it meets spec enforcement.
            base.IsValidPublisher();
            return false;
        }
    }
}
