using System.Collections.Generic;
using System.Linq;
using SmarterTestPackage.Common.Data;

namespace ValidateSmarterTestPackage.Validators
{
    public class ValidatorCollection : List<Validator>, IValidator
    {
        private string _message;
        public ErrorSeverity ErrorSeverity { get; set; }

        public string GetMessage()
        {
            return _message;
        }

        public bool IsValid(object value)
        {
            var isValid = this.All(x => x.IsValid(value));

            if (!isValid)
            {
                _message = this.Where(x => !x.IsValid(value))
                    .Select(x => x.GetMessage())
                    .Aggregate((i, j) => i + j);
                ErrorSeverity = this.Where(x => !x.IsValid(value))
                    .Select(x => x.ErrorSeverity).Max();
            }
            else
            {
                _message = null;
            }


            return isValid;
        }

        public bool IsValid(object value, bool isRequired)
        {
            return IsValid(value)
                   || !isRequired;
        }

        public ValidatorCollection AddAndReturn(Validator validator)
        {
            Add(validator);
            return this;
        }
    }
}