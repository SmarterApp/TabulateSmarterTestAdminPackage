using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.Enums;
using TabulateSmarterTestAdminPackage.Common.Utilities;
using TabulateSmarterTestAdminPackage.Common.Validators;

namespace TabulateSmarterTestAdminPackage.Processors.Specification.TestSpecification.Administration.TestForm
{
    internal class TestFormPoolPropertyProcessor : PoolPropertyProcessor
    {
        public TestFormPoolPropertyProcessor(XPathNavigator navigator) : base(navigator) {}

        internal new bool IsValidProperty()
        {
            var validators = new ValidatorCollection
            {
                new RequiredStringValidator(ErrorSeverity.Degraded),
                new MaxLengthValidator(ErrorSeverity.Degraded, 200)
            };
            Property = Navigator.Eval(sXp_Property);
            if (validators.IsValid(Property))
            {
                return true;
            }

            ReportingUtility.ReportSpecificationError(Navigator.NamespaceURI, sXp_Property.Expression,
                validators.GetMessage());
            return false;
        }

        internal new bool IsValidValue()
        {
            var validators = new ValidatorCollection
            {
                new RequiredStringValidator(ErrorSeverity.Degraded),
                new MaxLengthValidator(ErrorSeverity.Degraded, 200)
            };
            Value = Navigator.Eval(sXp_Value);
            if (validators.IsValid(Value))
            {
                return true;
            }

            ReportingUtility.ReportSpecificationError(Navigator.NamespaceURI, sXp_Value.Expression,
                validators.GetMessage());
            return false;
        }

        internal new bool IsValidLabel()
        {
            var validators = new ValidatorCollection
            {
                new RequiredStringValidator(ErrorSeverity.Degraded),
                new MaxLengthValidator(ErrorSeverity.Degraded, 150)
            };
            Label = Navigator.Eval(sXp_Label);
            if (validators.IsValid(Label))
            {
                return true;
            }

            ReportingUtility.ReportSpecificationError(Navigator.NamespaceURI, sXp_Label.Expression,
                validators.GetMessage());
            return false;
        }
    }
}