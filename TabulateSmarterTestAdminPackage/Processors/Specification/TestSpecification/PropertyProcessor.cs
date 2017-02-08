using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.Enums;
using TabulateSmarterTestAdminPackage.Common.Utilities;
using TabulateSmarterTestAdminPackage.Common.Validators;
using TabulateSmarterTestAdminPackage.Utility;

namespace TabulateSmarterTestAdminPackage.Processors.Specification.TestSpecification
{
    internal class PropertyProcessor : Processor
    {
        private static readonly XPathExpression sXp_Name = XPathExpression.Compile("@name");
        internal static readonly XPathExpression sXp_Value = XPathExpression.Compile("@value");
        internal static readonly XPathExpression sXp_Label = XPathExpression.Compile("@label");

        internal PropertyProcessor(XPathNavigator navigator) : base(navigator) {}

        private string Name { get; set; }
        internal string Value { get; set; }
        internal string Label { get; set; }

        public override bool Process()
        {
            return IsValidName()
                   && IsValidValue()
                   && IsValidLabel();
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

        internal bool IsValidValue()
        {
            var validators = new ValidatorCollection
            {
                new RequiredStringValidator(ErrorSeverity.Degraded),
                new MaxLengthValidator(ErrorSeverity.Degraded, 200)
            };
            Value = Navigator.Eval(sXp_Value);
            if (validators.IsValid(200))
            {
                return true;
            }
            AdminPackageUtility.ReportSpecificationError(Navigator.NamespaceURI, sXp_Value.Expression,
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
            if (validators.IsValid(200))
            {
                return true;
            }
            AdminPackageUtility.ReportSpecificationError(Navigator.NamespaceURI, sXp_Label.Expression,
                validators.GetMessage());
            return false;
        }
    }
}