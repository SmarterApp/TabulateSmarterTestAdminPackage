using TabulateSmarterTestPackage.Common.RestrictedValues.Enums;

namespace TabulateSmarterTestPackage.Common.Validators.Convenience
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

        public static ValidatorCollection IsValidOptionalNonEmptyWithLength(int length)
        {
            var validators = IsValidNonEmptyWithLength(length);
            validators.ForEach(x => x.ErrorSeverity = ErrorSeverity.Benign);
            return validators;
        }
    }
}