using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.Enums;
using TabulateSmarterTestAdminPackage.Common.Validators;
using TabulateSmarterTestAdminPackage.Utility;

namespace TabulateSmarterTestAdminPackage.Processors.Specification.TestSpecification
{
    internal class PropertyProcessor : Processor
    {
        private static readonly XPathExpression sXp_Name = XPathExpression.Compile("@name");
        private static readonly XPathExpression sXp_Value = XPathExpression.Compile("@value");
        private static readonly XPathExpression sXp_Label = XPathExpression.Compile("@label");

        private readonly XPathNavigator _navigator;

        internal PropertyProcessor(XPathNavigator navigator)
        {
            _navigator = navigator;
        }

        private string Name { get; set; }
        private string Value { get; set; }
        private string Label { get; set; }

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
            Name = _navigator.Eval(sXp_Name);
            if (validators.IsValid(Name))
            {
                return true;
            }
            AdminPackageUtility.ReportSpecificationError(_navigator.NamespaceURI, sXp_Name.Expression, validators.GetMessage());
            return false;
        }

        internal bool IsValidValue()
        {
            var validators = new ValidatorCollection
            {
                new RequiredStringValidator(ErrorSeverity.Degraded),
                new MaxLengthValidator(ErrorSeverity.Degraded, 200)
            };
            Value = _navigator.Eval(sXp_Value);
            if (validators.IsValid(200))
            {
                return true;
            }
            AdminPackageUtility.ReportSpecificationError(_navigator.NamespaceURI, sXp_Value.Expression, validators.GetMessage());
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
            if (validators.IsValid(200))
            {
                return true;
            }
            AdminPackageUtility.ReportSpecificationError(_navigator.NamespaceURI, sXp_Name.Expression, validators.GetMessage());
            return false;
        }
    }
}