using TabulateSmarterTestAdminPackage.Common.Enums;

namespace TabulateSmarterTestAdminPackage.Common.Validators
{
    public class MinDecimalValueValidator : Validator
    {
        public MinDecimalValueValidator(ErrorSeverity errorSeverity, object parameter) : base(errorSeverity, parameter)
        {}

        public override bool IsValid(object value)
        {
            decimal decimalValue;
            decimal decimalParameter;
            var valueString = value as string;
            var parameterString = Parameter as string;
            return !string.IsNullOrEmpty(valueString)
                   && !string.IsNullOrEmpty(parameterString)
                   && decimal.TryParse(valueString, out decimalValue)
                   && decimal.TryParse(parameterString, out decimalParameter)
                   && decimalParameter <= decimalValue;
        }

        public override string GetMessage()
        {
            return "[MinDecimal]";
        }
    }
}