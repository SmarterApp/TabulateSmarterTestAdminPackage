using TabulateSmarterTestAdminPackage.Common.Enums;

namespace TabulateSmarterTestAdminPackage.Common.Validators
{
    public class MinIntValueValidator : Validator
    {
        public MinIntValueValidator(ErrorSeverity errorSeverity, object parameter) : base(errorSeverity, parameter)
        {}

        public override bool IsValid(object value)
        {
            int intValue;
            int intParameter;
            var valueString = value as string;
            var parameterString = Parameter as string;
            return !string.IsNullOrEmpty(valueString)
                   && !string.IsNullOrEmpty(parameterString)
                   && int.TryParse(valueString, out intValue)
                   && int.TryParse(parameterString, out intParameter)
                   && intParameter <= intValue;
        }

        public override string GetMessage()
        {
            return "[MinInt]";
        }
    }
}