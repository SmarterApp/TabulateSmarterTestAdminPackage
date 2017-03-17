using SmarterTestPackage.Common.Data;

namespace ValidateSmarterTestPackage.Validators
{
    public class RequiredDoubleValidator : Validator
    {
        public RequiredDoubleValidator(ErrorSeverity errorSeverity, object parameter = null)
            : base(errorSeverity, parameter) {}

        public override bool IsValid(object value)
        {
            double doubleValue;
            var stringValue = value as string;
            return !string.IsNullOrEmpty(stringValue)
                   && double.TryParse(stringValue, out doubleValue);
        }

        public override string GetMessage()
        {
            return "[RequiredDouble]";
        }
    }
}