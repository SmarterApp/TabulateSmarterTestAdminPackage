namespace TabulateSmarterTestAdminPackage.Common.Validators
{
    public interface IValidator
    {
        bool IsValid(object value);
        bool IsValid(object value, bool isRequired);
        string GetMessage();
    }
}