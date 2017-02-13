﻿using TabulateSmarterTestAdminPackage.Common.Enums;

namespace TabulateSmarterTestAdminPackage.Common.Validators
{
    public abstract class Validator : IValidator
    {
        protected Validator(ErrorSeverity errorSeverity, object parameter = null)
        {
            ErrorSeverity = errorSeverity;
            Parameter = parameter;
        }

        public ErrorSeverity ErrorSeverity { get; set; }
        public object Parameter { get; set; }

        public abstract bool IsValid(object value);

        public bool IsValid(object value, bool isRequired)
        {
            return IsValid(value)
                || !isRequired && value == null;
        }

        public abstract string GetMessage();
    }
}