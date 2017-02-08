using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.Enums;
using TabulateSmarterTestAdminPackage.Common.Validators;
using TabulateSmarterTestAdminPackage.Utility;

namespace TabulateSmarterTestAdminPackage.Processors.Specification.TestSpecification.Administration.TestForm
{
    internal class TestFormIdentifierProcessor : IdentifierProcessor
    {
        public TestFormIdentifierProcessor(XPathNavigator navigator) : base(navigator) {}

        public override bool Process()
        {
            return IsValidUniqueId()
                   && IsValidName()
                   && IsValidVersion();
        }

        internal new bool IsValidUniqueId()
        {
            var validators = new ValidatorCollection
            {
                new RequiredStringValidator(ErrorSeverity.Degraded),
                new MaxLengthValidator(ErrorSeverity.Degraded, 100)
            };
            UniqueId = Navigator.Eval(sXp_UniqueId);
            if (validators.IsValid(UniqueId))
            {
                return true;
            }

            AdminPackageUtility.ReportSpecificationError(Navigator.NamespaceURI, sXp_UniqueId.Expression, validators.GetMessage());
            return false;
        }
    }
}