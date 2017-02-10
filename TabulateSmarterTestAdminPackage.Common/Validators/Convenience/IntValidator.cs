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
    }
}