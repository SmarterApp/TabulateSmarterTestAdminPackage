using TabulateSmarterTestAdminPackage.Common.Enums;

namespace TabulateSmarterTestAdminPackage.Common.Validators
{
    public abstract class Validator
    {
        protected Validator(ErrorSeverity errorSeverity, object parameter)
        {
            ErrorSeverity = errorSeverity;
            Parameter = parameter;
        }

        public ErrorSeverity ErrorSeverity { get; set; }
        public object Parameter { get; set; }

        public abstract bool IsValid(object value);

        public abstract string GetMessage();
    }
}