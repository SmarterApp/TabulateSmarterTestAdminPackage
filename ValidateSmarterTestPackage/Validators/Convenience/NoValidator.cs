using SmarterTestPackage.Common.Data;

namespace ValidateSmarterTestPackage.Validators.Convenience
{
    public class NoValidator : Validator
    {
        public NoValidator(ErrorSeverity errorSeverity, object parameter = null) : base(errorSeverity, parameter) {}

        public override bool IsValid(object value)
        {
            return true;
        }

        public override string GetMessage()
        {
            return string.Empty;
        }
    }
}