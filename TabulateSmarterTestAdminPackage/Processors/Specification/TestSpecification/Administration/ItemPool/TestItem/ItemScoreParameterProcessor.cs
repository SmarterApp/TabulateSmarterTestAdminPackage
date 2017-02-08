using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.Enums;
using TabulateSmarterTestAdminPackage.Common.Utilities;
using TabulateSmarterTestAdminPackage.Common.Validators;
using TabulateSmarterTestAdminPackage.Utility;

namespace TabulateSmarterTestAdminPackage.Processors.Specification.TestSpecification.Administration.ItemPool.TestItem
{
    internal class ItemScoreParameterProcessor : Processor
    {
        private static readonly XPathExpression sXp_MeasurementParameter =
            XPathExpression.Compile("@measurementparameter");

        private static readonly XPathExpression sXp_Value = XPathExpression.Compile("@value");

        internal ItemScoreParameterProcessor(XPathNavigator navigator) : base(navigator) {}

        private string MeasurementParameter { get; set; }
        private string Value { get; set; }

        public override bool Process()
        {
            return IsValidMeasurementParameter()
                   && IsValidValue();
        }

        // TODO: enum
        internal bool IsValidMeasurementParameter()
        {
            var validators = new ValidatorCollection
            {
                new RequiredStringValidator(ErrorSeverity.Degraded),
                new MaxLengthValidator(ErrorSeverity.Degraded, 50)
            };
            MeasurementParameter = Navigator.Eval(sXp_MeasurementParameter);
            if (validators.IsValid(MeasurementParameter))
            {
                return true;
            }

            AdminPackageUtility.ReportSpecificationError(Navigator.NamespaceURI, sXp_MeasurementParameter.Expression,
                validators.GetMessage());
            return false;
        }

        internal bool IsValidValue()
        {
            var validators = new ValidatorCollection
            {
                new RequiredDecimalValidator(ErrorSeverity.Degraded),
                new MaxLengthValidator(ErrorSeverity.Degraded, 30)
            };
            Value = Navigator.Eval(sXp_Value);
            if (validators.IsValid(Value))
            {
                return true;
            }

            AdminPackageUtility.ReportSpecificationError(Navigator.NamespaceURI, sXp_Value.Expression,
                validators.GetMessage());
            return false;
        }
    }
}