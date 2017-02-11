using TabulateSmarterTestAdminPackage.Common.Enums;

namespace TabulateSmarterTestAdminPackage.Common.Validators.Convenience
{
    public static class IntValidator
    {
        public static ValidatorCollection IsValidNonEmptyWithLengthAndMinValue(int length, int minValue)
        {
            return new ValidatorCollection
            {
                new RequiredIntValidator(ErrorSeverity.Degraded),
                new MaxLengthValidator(ErrorSeverity.Degraded, length),
                new MinIntValueValidator(ErrorSeverity.Degraded, minValue)
            };
        }

        public static ValidatorCollection IsValidPositiveNonEmptyWithLength(int length)
        {
            return new ValidatorCollection
            {
                new RequiredIntValidator(ErrorSeverity.Degraded),
                new MaxLengthValidator(ErrorSeverity.Degraded, length),
                new MinIntValueValidator(ErrorSeverity.Degraded, "0")
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