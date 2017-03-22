using SmarterTestPackage.Common.Data;

namespace ValidateSmarterTestPackage.Validators.Convenience
{
    public static class DateTimeValidator
    {
        public static ValidatorCollection IsValidNonEmptyWithLength(int length)
        {
            return new ValidatorCollection
            {
                new RequiredDateTimeValidator(ErrorSeverity.Degraded),
                new MaxLengthValidator(ErrorSeverity.Degraded, length)
            };
        }

        public static ValidatorCollection IsValidOptionalNonEmptyWithLength(int length)
        {
            var validators = IsValidNonEmptyWithLength(length);
            validators.ForEach(x => x.ErrorSeverity = ErrorSeverity.Benign);
            return validators;
        }
    }
}