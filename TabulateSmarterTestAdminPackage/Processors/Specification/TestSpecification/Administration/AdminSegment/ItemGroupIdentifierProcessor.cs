using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.Enums;
using TabulateSmarterTestAdminPackage.Common.Processors;
using TabulateSmarterTestAdminPackage.Common.Utilities;
using TabulateSmarterTestAdminPackage.Common.Validators;

namespace TabulateSmarterTestAdminPackage.Processors.Specification.TestSpecification.Administration.AdminSegment
{
    internal class ItemGroupIdentifierProcessor : IdentifierProcessor
    {
        public ItemGroupIdentifierProcessor(XPathNavigator navigator) : base(navigator) {}

        internal new bool IsValidUniqueId()
        {
            var validators = new ValidatorCollection
            {
                new RequiredStringValidator(ErrorSeverity.Degraded),
                new MaxLengthValidator(ErrorSeverity.Degraded, 200)
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