using SmarterTestPackage.Common.Data;

namespace ValidateSmarterTestPackage.Validators
{
    public class RequiredDecimalValidator : Validator
    {
        public RequiredDecimalValidator(ErrorSeverity errorSeverity, object parameter = null)
            : base(errorSeverity, parameter) {}

        public override bool IsValid(object value)
        {
            decimal decimalValue;
            var stringValue = value as string;
            return !string.IsNullOrEmpty(stringValue)
                   && decimal.TryParse(stringValue, out decimalValue);
        }

        public override string GetMessage()
        {
            return "[RequiredDecimal]";
        }
    }
}