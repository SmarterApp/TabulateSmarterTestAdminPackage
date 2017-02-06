using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.Enums;
using TabulateSmarterTestAdminPackage.Common.Validators;
using TabulateSmarterTestAdminPackage.Processors.Specification;
using TabulateSmarterTestAdminPackage.Utility;

namespace TabulateSmarterTestAdminPackage.Processors.Loader
{
    internal class TestSpecificationErrorProcessor : TestSpecificationProcessor
    {
        internal TestSpecificationErrorProcessor(XPathNavigator navigator, PackageType expectedPackageType) : base(navigator, expectedPackageType) {}

        internal new bool IsValidPublisher()
        {
            var validators = new ValidatorCollection
            {
                new RequiredStringValidator(ErrorSeverity.Severe),
                new MaxLengthValidator(ErrorSeverity.Severe, 100)
            };

            Publisher = Navigator.Eval(sXp_Publisher);
            // In this case, the loader's enforcement is stricter than the spec. If it passes loader enforcement, it's good.
            if (validators.IsValid(Publisher))
            {
                return true;
            }

            AdminPackageUtility.ReportLoadError(Navigator.NamespaceURI, sXp_Publisher.Expression, validators.GetMessage());
            // If it fails loader enforcement we're going to check if it meets spec enforcement.
            base.IsValidPublisher();
            return false;
        }
    }
}