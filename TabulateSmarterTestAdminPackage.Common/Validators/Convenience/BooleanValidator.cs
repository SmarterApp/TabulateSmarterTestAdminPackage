using TabulateSmarterTestPackage.Common.RestrictedValues.Enums;

namespace TabulateSmarterTestPackage.Common.Validators.Convenience
{
    public class BooleanValidator
    {
        public static ValidatorCollection IsValidNonEmptyWithLength(int length)
        {
            return new ValidatorCollection
            {
                new RequiredBooleanValidator(ErrorSeverity.Degraded),
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