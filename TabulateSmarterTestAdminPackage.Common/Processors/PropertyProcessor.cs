using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.Enums;
using TabulateSmarterTestAdminPackage.Common.Utilities;
using TabulateSmarterTestAdminPackage.Common.Validators;

namespace TabulateSmarterTestAdminPackage.Common.Processors
{
    public class PropertyProcessor : Processor
    {
        public static readonly XPathExpression sXp_Name = XPathExpression.Compile("@name");
        public static readonly XPathExpression sXp_Value = XPathExpression.Compile("@value");
        public static readonly XPathExpression sXp_Label = XPathExpression.Compile("@label");

        public PropertyProcessor(XPathNavigator navigator) : base(navigator) {}

        public string Name { get; set; }
        public string Value { get; set; }
        public string Label { get; set; }

        public override bool Process()
        {
            return IsValidName()
                   && IsValidValue()
                   && IsValidLabel();
        }

        public bool IsValidName()
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
            ReportingUtility.ReportSpecificationError(Navigator.NamespaceURI, sXp_Name.Expression,
                validators.GetMessage());
            return false;
        }

        public bool IsValidValue()
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
            ReportingUtility.ReportSpecificationError(Navigator.NamespaceURI, sXp_Value.Expression,
                validators.GetMessage());
            return false;
        }

        public bool IsValidLabel()
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
            ReportingUtility.ReportSpecificationError(Navigator.NamespaceURI, sXp_Label.Expression,
                validators.GetMessage());
            return false;
        }
    }
}