using TabulateSmarterTestAdminPackage.Common.RestrictedValues.Enums;

namespace TabulateSmarterTestAdminPackage.Common.Validators
{
    public interface IValidator
    {
        ErrorSeverity ErrorSeverity { get; set; }
        bool IsValid(object value);
        bool IsValid(object value, bool isRequired);
        string GetMessage();
    }
}