using System;
using TabulateSmarterTestPackage.Common.RestrictedValues.Enums;

namespace ValidateSmarterTestPackage.Validators
{
    public class RequiredDateTimeValidator : Validator
    {
        public RequiredDateTimeValidator(ErrorSeverity errorSeverity, object parameter = null)
            : base(errorSeverity, parameter) {}

        public override bool IsValid(object value)
        {
            DateTime dateTimeValue;
            return DateTime.TryParse(value as string, out dateTimeValue);
        }

        public override string GetMessage()
        {
            return "[RequiredDateTime]";
        }
    }
}