using System.IO;
using TabulateSmarterTestPackage.Common.RestrictedValues.Enums;

namespace TabulateSmarterTestPackage.Common.Validators.CrossTabulation
{
    public class StimuliExistsValidator : Validator
    {
        public StimuliExistsValidator(ErrorSeverity errorSeverity, object parameter = null)
            : base(errorSeverity, parameter) {}

        public string Stimuli { get; set; }

        public override bool IsValid(object value)
        {
            if (!(value is string))
            {
                return false;
            }
            Stimuli = ((string) value).Split('.')[0];
            return Parameter is string &&
                   Directory.Exists(Path.Combine((string) Parameter, "Stimuli", Stimuli));
        }

        public override string GetMessage()
        {
            return $"[Stimuli: {Stimuli} Does not exist at path: {Path.Combine((string) Parameter, "Stimuli", Stimuli)}";
        }
    }
}