using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.Enums;
using TabulateSmarterTestAdminPackage.Common.Utilities;
using TabulateSmarterTestAdminPackage.Common.Validators;
using TabulateSmarterTestAdminPackage.Utility;

namespace TabulateSmarterTestAdminPackage.Processors.Specification.TestSpecification.Administration.AdminSegment
{
    internal class ItemSelectionParameterPropertyProcessor : PropertyProcessor
    {
        public ItemSelectionParameterPropertyProcessor(XPathNavigator navigator) : base(navigator) {}

        internal new bool IsValidValue()
        {
            var validators = new ValidatorCollection
            {
                new RequiredStringValidator(ErrorSeverity.Degraded),
                new MaxLengthValidator(ErrorSeverity.Degraded, 100)
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

        internal new bool IsValidLabel()
        {
            var validators = new ValidatorCollection
            {
                new RequiredStringValidator(ErrorSeverity.Degraded),
                new MaxLengthValidator(ErrorSeverity.Degraded, 500)
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