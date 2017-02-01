using TabulateSmarterTestAdminPackage.Common.Enums;

namespace TabulateSmarterTestAdminPackage.Common.Validators
{
    public class RequiredIntValidator : Validator
    {
        public RequiredIntValidator(ErrorSeverity errorSeverity, object parameter) : base(errorSeverity, parameter)
        {}

        public override bool IsValid(object value)
        {
            int intValue;
            var valueString = value as string;
            return int.TryParse(valueString, out intValue);
        }

        public override string GetMessage()
        {
            return "[RequiredInt]";
        }
    }
}