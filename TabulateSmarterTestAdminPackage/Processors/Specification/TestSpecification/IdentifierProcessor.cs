using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.Enums;
using TabulateSmarterTestAdminPackage.Common.Utilities;
using TabulateSmarterTestAdminPackage.Common.Validators;
using TabulateSmarterTestAdminPackage.Utility;

namespace TabulateSmarterTestAdminPackage.Processors.Specification.TestSpecification
{
    internal class IdentifierProcessor : Processor
    {
        internal static readonly XPathExpression sXp_Name = XPathExpression.Compile("@name");
        internal static readonly XPathExpression sXp_UniqueId = XPathExpression.Compile("@uniqueid");
        internal static readonly XPathExpression sXp_Version = XPathExpression.Compile("@version");
        private static readonly XPathExpression sXp_Label = XPathExpression.Compile("@label");

        internal IdentifierProcessor(XPathNavigator navigator) : base(navigator) {}

        internal string Name { get; set; }
        internal string Version { get; set; }
        internal string UniqueId { get; set; }
        private string Label { get; set; }

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
                new MaxLengthValidator(ErrorSeverity.Degraded, 250)
            };
            UniqueId = Navigator.Eval(sXp_UniqueId);
            if (validators.IsValid(UniqueId))
            {
                return true;
            }

            AdminPackageUtility.ReportSpecificationError(Navigator.NamespaceURI, sXp_UniqueId.Expression,
                validators.GetMessage());
            return false;
        }

        internal bool IsValidName()
        {
            var validators = new ValidatorCollection
            {
                new RequiredStringValidator(ErrorSeverity.Degraded),
                new MaxLengthValidator(ErrorSeverity.Degraded, 200)
            };
            Name = Navigator.Eval(sXp_Name);
            if (validators.IsValid(Name))
            {
                return true;
            }

            AdminPackageUtility.ReportSpecificationError(Navigator.NamespaceURI, sXp_Name.Expression,
                validators.GetMessage());
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
            Version = Navigator.Eval(sXp_Version);
            if (validators.IsValid(Version))
            {
                return true;
            }
            AdminPackageUtility.ReportSpecificationError(Navigator.NamespaceURI, sXp_UniqueId.Expression,
                validators.GetMessage());
            return false;
        }

        internal bool IsValidLabel()
        {
            var validators = new ValidatorCollection
            {
                new RequiredStringValidator(ErrorSeverity.Degraded),
                new MaxLengthValidator(ErrorSeverity.Degraded, 200)
            };
            Label = Navigator.Eval(sXp_Label);
            if (validators.IsValid(Label))
            {
                return true;
            }

            AdminPackageUtility.ReportSpecificationError(Navigator.NamespaceURI, sXp_Label.Expression,
                validators.GetMessage());
            return false;
        }
    }
}