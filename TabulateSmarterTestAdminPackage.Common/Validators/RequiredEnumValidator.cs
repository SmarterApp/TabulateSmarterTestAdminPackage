using TabulateSmarterTestAdminPackage.Common.Enums;

namespace TabulateSmarterTestAdminPackage.Common.Validators
{
    public class RequiredEnumValidator : Validator
    {
        public RequiredEnumValidator(ErrorSeverity errorSeverity, object parameter = null) : base(errorSeverity, parameter) {}
        public override bool IsValid(object value)
        {
            throw new System.NotImplementedException();
        }

        public override string GetMessage()
        {
            throw new System.NotImplementedException();
        }
    }
}
