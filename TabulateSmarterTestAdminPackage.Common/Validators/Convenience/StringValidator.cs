using TabulateSmarterTestAdminPackage.Common.Enums;

namespace TabulateSmarterTestAdminPackage.Common.Validators.Convenience
{
    public static class StringValidator
    {
        public static ValidatorCollection IsValidNonEmptyWithLength(int length)
        {
            return new ValidatorCollection
            {
                new RequiredStringValidator(ErrorSeverity.Degraded),
                new MaxLengthValidator(ErrorSeverity.Degraded, length)
            };
        }
    }
}
