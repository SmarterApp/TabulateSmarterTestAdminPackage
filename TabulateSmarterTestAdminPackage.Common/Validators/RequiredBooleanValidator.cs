using TabulateSmarterTestAdminPackage.Common.Enums;

namespace TabulateSmarterTestAdminPackage.Common.Validators
{
    public class RequiredBooleanValidator : Validator
    {
        public RequiredBooleanValidator(ErrorSeverity errorSeverity, object parameter = null)
            : base(errorSeverity, parameter) {}

        public override bool IsValid(object value)
        {
            bool boolValue;
            return bool.TryParse((value as string), out boolValue);
        }

        public override string GetMessage()
        {
            return "[RequiredBoolean]";
        }
    }
}