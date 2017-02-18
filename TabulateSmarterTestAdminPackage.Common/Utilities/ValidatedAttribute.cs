using TabulateSmarterTestAdminPackage.Common.Validators;

namespace TabulateSmarterTestAdminPackage.Common.Utilities
{
    public class ValidatedAttribute
    {
        public string Value { get; set; }
        public bool IsValid { get; set; }
        public string Name { get; set; }
        public IValidator Validator { get; set; }
    }
}