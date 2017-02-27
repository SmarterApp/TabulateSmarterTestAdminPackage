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
            return decimal.TryParse(value as string, out decimalValue);
        }

        public override string GetMessage()
        {
            return "[RequiredDecimal]";
        }
    }
}