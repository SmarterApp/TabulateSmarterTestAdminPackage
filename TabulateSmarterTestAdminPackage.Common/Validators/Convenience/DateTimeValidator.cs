using TabulateSmarterTestAdminPackage.Common.Enums;

namespace TabulateSmarterTestAdminPackage.Common.Validators.Convenience
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
    }
}