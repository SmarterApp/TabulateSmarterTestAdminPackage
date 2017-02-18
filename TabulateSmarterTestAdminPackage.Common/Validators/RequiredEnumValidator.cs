using System;
using System.Linq;
using TabulateSmarterTestAdminPackage.Common.RestrictedValues.Enums;

namespace TabulateSmarterTestAdminPackage.Common.Validators
{
    public class RequiredEnumValidator : Validator
    {
        public RequiredEnumValidator(ErrorSeverity errorSeverity, object parameter = null)
            : base(errorSeverity, parameter) {}

        public override bool IsValid(object value)
        {
            return Enum.GetNames(Parameter.GetType()).Contains(value as string);
        }

        public override string GetMessage()
        {
            return $"[RequiredEnumIn:{Enum.GetNames(Parameter.GetType())}]";
        }
    }
}