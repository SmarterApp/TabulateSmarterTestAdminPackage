using TabulateSmarterTestPackage.Common.RestrictedValues.Enums;

namespace ValidateSmarterTestPackage.Validators.Convenience
{
    public static class DecimalValidator
    {
        public static ValidatorCollection IsValidNonEmptyWithLengthAndMinValue(int length, decimal minValue)
        {
            return new ValidatorCollection
            {
                new RequiredDecimalValidator(ErrorSeverity.Degraded),
                new MaxLengthValidator(ErrorSeverity.Degraded, length),
                new MinDecimalValueValidator(ErrorSeverity.Degraded, minValue)
            };
        }

        public static ValidatorCollection IsValidPositiveNonEmptyWithLength(int length)
        {
            return new ValidatorCollection
            {
                new RequiredDecimalValidator(ErrorSeverity.Degraded),
                new MaxLengthValidator(ErrorSeverity.Degraded, length),
                new MinDecimalValueValidator(ErrorSeverity.Degraded, "0")
            };
        }

        public static ValidatorCollection IsValidOptionalNonEmptyWithLengthAndMinValue(int length, int minValue)
        {
            var validators =
                IsValidNonEmptyWithLengthAndMinValue(length, minValue);
            validators.ForEach(x => x.ErrorSeverity = ErrorSeverity.Benign);
            return validators;
        }

        public static ValidatorCollection IsValidOptionalPositiveNonEmptyWithLength(int length)
        {
            var validators = IsValidPositiveNonEmptyWithLength(length);
            validators.ForEach(x => x.ErrorSeverity = ErrorSeverity.Benign);
            return validators;
        }
    }
}