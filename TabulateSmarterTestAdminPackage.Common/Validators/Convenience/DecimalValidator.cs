using TabulateSmarterTestAdminPackage.Common.Enums;

namespace TabulateSmarterTestAdminPackage.Common.Validators.Convenience
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
    }
}