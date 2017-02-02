using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.Enums;
using TabulateSmarterTestAdminPackage.Common.Validators;
using TabulateSmarterTestAdminPackage.Utility;

namespace TabulateSmarterTestAdminPackage.Processors.Specification
{
    internal class IdentifierProcessor : Processor
    {
        private static readonly XPathExpression sXp_Name = XPathExpression.Compile("@name");
        private static readonly XPathExpression sXp_UniqueId = XPathExpression.Compile("@uniqueid");
        private static readonly XPathExpression sXp_Label = XPathExpression.Compile("@label");
        private static readonly XPathExpression sXp_Version = XPathExpression.Compile("@version");

        private readonly XPathNavigator _navigator;

        internal IdentifierProcessor(XPathNavigator navigator)
        {
            _navigator = navigator;
        }

        private string Name { get; set; }
        private string Version { get; set; }
        private string Label { get; set; }
        private string UniqueId { get; set; }

        public override bool Process()
        {
            return IsValidUniqueId()
                   && IsValidName()
                   && IsValidLabel()
                   && IsValidVersion();
        }

        internal bool IsValidUniqueId()
        {
            var validators = new ValidatorCollection
            {
                new RequiredStringValidator(ErrorSeverity.Degraded),
                new MaxLengthValidator(ErrorSeverity.Degraded, 255)
            };
            UniqueId = _navigator.Eval(sXp_UniqueId);
            if (validators.IsValid(UniqueId))
            {
                return true;
            }

            AdminPackageUtility.ReportSpecificationError(_navigator.NamespaceURI, sXp_UniqueId.Expression, validators.GetMessage());
            return false;
        }

        internal bool IsValidName()
        {
            var validators = new ValidatorCollection
            {
                new RequiredStringValidator(ErrorSeverity.Degraded),
                new MaxLengthValidator(ErrorSeverity.Degraded, 200)
            };
            Name = _navigator.Eval(sXp_Name);
            if (validators.IsValid(Name))
            {
                return true;
            }

            AdminPackageUtility.ReportSpecificationError(_navigator.NamespaceURI, sXp_Name.Expression, validators.GetMessage());
            return false;
        }

        internal bool IsValidLabel()
        {
            var validators = new ValidatorCollection
            {
                new RequiredStringValidator(ErrorSeverity.Degraded),
                new MaxLengthValidator(ErrorSeverity.Degraded, 200)
            };
            Label = _navigator.Eval(sXp_Label);
            if (validators.IsValid(Label))
            {
                return true;
            }

            AdminPackageUtility.ReportSpecificationError(_navigator.NamespaceURI, sXp_Label.Expression, validators.GetMessage());
            return false;
        }

        internal bool IsValidVersion()
        {
            var validators = new ValidatorCollection
            {
                new RequiredIntValidator(ErrorSeverity.Degraded),
                new MaxLengthValidator(ErrorSeverity.Degraded, 10),
                new MinIntValueValidator(ErrorSeverity.Degraded, 0)
            };
            Version = _navigator.Eval(sXp_Version);
            if (validators.IsValid(Version))
            {
                return true;
            }
            AdminPackageUtility.ReportSpecificationError(_navigator.NamespaceURI, sXp_UniqueId.Expression, validators.GetMessage());
            return false;
        }
    }
}