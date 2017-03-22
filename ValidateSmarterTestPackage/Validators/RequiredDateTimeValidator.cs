using System;
using SmarterTestPackage.Common.Data;

namespace ValidateSmarterTestPackage.Validators
{
    public class RequiredDateTimeValidator : Validator
    {
        public RequiredDateTimeValidator(ErrorSeverity errorSeverity, object parameter = null)
            : base(errorSeverity, parameter) {}

        public override bool IsValid(object value)
        {
            DateTime dateTimeValue;
            var stringValue = value as string;
            return !string.IsNullOrEmpty(stringValue)
                   && DateTime.TryParse(stringValue, out dateTimeValue);
        }

        public override string GetMessage()
        {
            return "[RequiredDateTime]";
        }
    }
}