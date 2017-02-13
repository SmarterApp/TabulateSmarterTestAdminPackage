using System;
using TabulateSmarterTestAdminPackage.Common.Enums;

namespace TabulateSmarterTestAdminPackage.Common.Validators
{
    public class StringMatchValidator : Validator
    {
        public StringMatchValidator(ErrorSeverity errorSeverity, object parameter = null)
            : base(errorSeverity, parameter) {}

        public override bool IsValid(object value)
        {
            return value is string
                   && Parameter is string
                   && string.Equals(
                       (string) value,
                       (string) Parameter,
                       StringComparison.OrdinalIgnoreCase);
        }

        public override string GetMessage()
        {
            return $"[Value!={Parameter}";
        }
    }
}