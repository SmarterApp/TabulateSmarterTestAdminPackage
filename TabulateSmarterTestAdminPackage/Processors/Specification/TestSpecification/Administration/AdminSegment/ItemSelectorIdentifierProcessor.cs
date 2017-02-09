using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.Enums;
using TabulateSmarterTestAdminPackage.Common.Utilities;
using TabulateSmarterTestAdminPackage.Common.Validators;

namespace TabulateSmarterTestAdminPackage.Processors.Specification.TestSpecification.Administration.AdminSegment
{
    internal class ItemSelectorIdentifierProcessor : IdentifierProcessor
    {
        public ItemSelectorIdentifierProcessor(XPathNavigator navigator) : base(navigator) {}

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

        internal new bool IsValidVersion()
        {
            var validators = new ValidatorCollection
            {
                new RequiredIntValidator(ErrorSeverity.Degraded),
                new MaxLengthValidator(ErrorSeverity.Degraded, 20),
                new MinIntValueValidator(ErrorSeverity.Degraded, 0)
            };
            Version = Navigator.Eval(sXp_Version);
            if (validators.IsValid(Version))
            {
                return true;
            }
            ReportingUtility.ReportSpecificationError(Navigator.NamespaceURI, sXp_UniqueId.Expression,
                validators.GetMessage());
            return false;
        }
    }
}