using TabulateSmarterTestAdminPackage.Common.Enums;

namespace TabulateSmarterTestAdminPackage.Common.Validators
{
    public class MinDecimalValueValidator : Validator
    {
        public MinDecimalValueValidator(ErrorSeverity errorSeverity, object parameter = null)
            : base(errorSeverity, parameter) {}

        public override bool IsValid(object value)
        {
            decimal decimalValue;
            decimal decimalParameter;
            return !string.IsNullOrEmpty(value as string)
                   && !string.IsNullOrEmpty(Parameter as string)
                   && decimal.TryParse((string) value, out decimalValue)
                   && decimal.TryParse((string) Parameter, out decimalParameter)
                   && decimalParameter <= decimalValue;
        }

        public override string GetMessage()
        {
            return $"[MinDecimal>{Parameter}]";
        }
    }
}