using TabulateSmarterTestAdminPackage.Common.Enums;

namespace TabulateSmarterTestAdminPackage.Common.Validators
{
    public class RequiredIntValidator : Validator
    {
        public RequiredIntValidator(ErrorSeverity errorSeverity, object parameter = null)
            : base(errorSeverity, parameter) {}

        public override bool IsValid(object value)
        {
            int intValue;
            return int.TryParse(value as string, out intValue);
        }

        public override string GetMessage()
        {
            return "[RequiredInt]";
        }
    }
}