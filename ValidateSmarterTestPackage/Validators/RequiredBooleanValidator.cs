using SmarterTestPackage.Common.Data;

namespace ValidateSmarterTestPackage.Validators
{
    public class RequiredBooleanValidator : Validator
    {
        public RequiredBooleanValidator(ErrorSeverity errorSeverity, object parameter = null)
            : base(errorSeverity, parameter) {}

        public override bool IsValid(object value)
        {
            bool boolValue;
            var stringValue = value as string;
            return !string.IsNullOrEmpty(stringValue)
                   && bool.TryParse(stringValue, out boolValue);
        }

        public override string GetMessage()
        {
            return "[RequiredBoolean]";
        }
    }
}