namespace TabulateSmarterTestAdminPackage.Common.Validators
{
    public interface IValidator
    {
        bool IsValid(object value);
        string GetMessage();
    }
}