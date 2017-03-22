using System.Text.RegularExpressions;
using SmarterTestPackage.Common.Data;

namespace ValidateSmarterTestPackage.Validators
{
    public class RequiredRegularExpressionValidator : Validator
    {
        public RequiredRegularExpressionValidator(ErrorSeverity errorSeverity, object parameter = null)
            : base(errorSeverity, parameter) {}

        public override bool IsValid(object value)
        {
            if (!(value is string) || !(Parameter is string))
            {
                return false;
            }
            var matcher = new Regex((string) Parameter);
            return matcher.IsMatch((string) value);
        }

        public override string GetMessage()
        {
            return $"[RequiredMatch:{Parameter}]";
        }
    }
}