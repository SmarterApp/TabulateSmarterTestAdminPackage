using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.Enums;
using TabulateSmarterTestAdminPackage.Common.Utilities;
using TabulateSmarterTestAdminPackage.Common.Validators;

namespace TabulateSmarterTestAdminPackage.Processors.Specification.TestSpecification.Administration.ItemPool.Passage
{
    internal class PassageIdentifierProcessor : IdentifierProcessor
    {
        public PassageIdentifierProcessor(XPathNavigator navigator) : base(navigator) {}

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

        // Not Required
        internal new bool IsValidName()
        {
            var validators = new ValidatorCollection
            {
                new RequiredStringValidator(ErrorSeverity.Benign),
                new MaxLengthValidator(ErrorSeverity.Benign, 100)
            };
            Name = Navigator.Eval(sXp_Name);
            if (Name == null || validators.IsValid(Name))
            {
                return true;
            }

            ReportingUtility.ReportSpecificationError(Navigator.NamespaceURI, sXp_Name.Expression,
                validators.GetMessage());
            return false;
        }
    }
}