using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.Enums;
using TabulateSmarterTestAdminPackage.Common.Utilities;
using TabulateSmarterTestAdminPackage.Common.Validators;

namespace TabulateSmarterTestAdminPackage.Processors.Specification.TestSpecification.Administration.TestForm
{
    internal class TestFormPartitionIdentifierProcessor : IdentifierProcessor
    {
        public TestFormPartitionIdentifierProcessor(XPathNavigator navigator) : base(navigator) {}

        public new bool Process()
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

            ReportingUtility.ReportSpecificationError(Navigator.NamespaceURI, sXp_UniqueId.Expression,
                validators.GetMessage());
            return false;
        }
    }
}