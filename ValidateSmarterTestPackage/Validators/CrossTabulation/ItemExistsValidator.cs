using System.IO;
using SmarterTestPackage.Common.Data;

namespace ValidateSmarterTestPackage.Validators.CrossTabulation
{
    public class ItemExistsValidator : Validator
    {
        public ItemExistsValidator(ErrorSeverity errorSeverity, object parameter = null)
            : base(errorSeverity, parameter) {}

        public string Item { get; set; }

        public override bool IsValid(object value)
        {
            if (!(value is string))
            {
                return false;
            }
            Item = ((string) value).Split('.')[0];
            return Parameter is string &&
                   Directory.Exists(Path.Combine((string) Parameter, "Items", Item));
        }

        public override string GetMessage()
        {
            return $"[Item: {Item} Does not exist at path: {Path.Combine((string) Parameter, "Items", Item)}";
        }
    }
}