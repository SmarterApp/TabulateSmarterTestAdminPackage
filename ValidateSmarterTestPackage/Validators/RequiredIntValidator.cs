using SmarterTestPackage.Common.Data;

namespace ValidateSmarterTestPackage.Validators
{
    public class RequiredIntValidator : Validator
    {
        public RequiredIntValidator(ErrorSeverity errorSeverity, object parameter = null)
            : base(errorSeverity, parameter) {}

        public override bool IsValid(object value)
        {
            int intValue;
            var stringValue = value as string;
            return !string.IsNullOrEmpty(stringValue)
                   && int.TryParse(stringValue, out intValue);
        }

        public override string GetMessage()
        {
            return "[RequiredInt]";
        }
    }
}