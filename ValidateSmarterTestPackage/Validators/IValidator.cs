using TabulateSmarterTestPackage.Common.RestrictedValues.Enums;

namespace ValidateSmarterTestPackage.Validators
{
    public interface IValidator
    {
        ErrorSeverity ErrorSeverity { get; set; }
        bool IsValid(object value);
        bool IsValid(object value, bool isRequired);
        string GetMessage();
    }
}