using TabulateSmarterTestAdminPackage.Common.Enums;

namespace TabulateSmarterTestAdminPackage.Common.Validators
{
    public class RequiredDecimalValidator : Validator
    {
        public RequiredDecimalValidator(ErrorSeverity errorSeverity, object parameter) : base(errorSeverity, parameter)
        {}

        public override bool IsValid(object value)
        {
            decimal decimalValue;
            var valueString = value as string;
            return decimal.TryParse(valueString, out decimalValue);
        }

        public override string GetMessage()
        {
            return "[RequiredDecimal]";
        }
    }
}