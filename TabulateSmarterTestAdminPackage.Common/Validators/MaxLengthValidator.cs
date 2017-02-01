using TabulateSmarterTestAdminPackage.Common.Enums;

namespace TabulateSmarterTestAdminPackage.Common.Validators
{
    public class MaxLengthValidator : Validator
    {
        public MaxLengthValidator(ErrorSeverity errorSeverity, object parameter) : base(errorSeverity, parameter)
        {}

        public override bool IsValid(object value)
        {
            return value is string
                   && Parameter is int
                   && ((string)value).Length <= (int)Parameter;
        }

        public override string GetMessage()
        {
            return $"[Length<={(Parameter is int ? Parameter.ToString() : string.Empty)}]";
        }
    }
}