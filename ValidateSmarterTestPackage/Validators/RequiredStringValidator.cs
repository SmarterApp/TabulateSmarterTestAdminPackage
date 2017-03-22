using SmarterTestPackage.Common.Data;

namespace ValidateSmarterTestPackage.Validators
{
    public class RequiredStringValidator : Validator
    {
        public RequiredStringValidator(ErrorSeverity errorSeverity, object parameter = null)
            : base(errorSeverity, parameter) {}

        public override bool IsValid(object value)
        {
            return !string.IsNullOrEmpty(value as string);
        }

        public override string GetMessage()
        {
            return "[RequiredString]";
        }
    }
}