using System;
using TabulateSmarterTestAdminPackage.Common.Enums;

namespace TabulateSmarterTestAdminPackage.Common.Validators
{
    public class RequiredDateTimeValidator : Validator
    {
        public RequiredDateTimeValidator(ErrorSeverity errorSeverity, object parameter) : base(errorSeverity, parameter)
        {}

        public override bool IsValid(object value)
        {
            DateTime dateTimeValue;
            var valueString = value as string;
            return DateTime.TryParse(valueString, out dateTimeValue);
        }

        public override string GetMessage()
        {
            return "[RequiredDateTime]";
        }
    }
}