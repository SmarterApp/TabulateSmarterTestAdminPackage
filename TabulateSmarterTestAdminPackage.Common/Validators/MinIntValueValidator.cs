using TabulateSmarterTestPackage.Common.RestrictedValues.Enums;

namespace TabulateSmarterTestPackage.Common.Validators
{
    public class MinIntValueValidator : Validator
    {
        public MinIntValueValidator(ErrorSeverity errorSeverity, object parameter = null)
            : base(errorSeverity, parameter) {}

        public override bool IsValid(object value)
        {
            int intValue;
            int intParameter;
            return !string.IsNullOrEmpty(value as string)
                   && !string.IsNullOrEmpty(Parameter as string)
                   && int.TryParse((string) value, out intValue)
                   && int.TryParse((string) Parameter, out intParameter)
                   && intParameter <= intValue;
        }

        public override string GetMessage()
        {
            return $"[MinInt>{Parameter}]";
        }
    }
}