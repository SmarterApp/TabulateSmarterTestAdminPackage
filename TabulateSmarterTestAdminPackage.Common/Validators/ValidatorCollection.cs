using System.Collections.Generic;
using System.Linq;

namespace TabulateSmarterTestAdminPackage.Common.Validators
{
    public class ValidatorCollection
    {
        private readonly Queue<Validator> _validators;

        public ValidatorCollection()
        {
            _validators = new Queue<Validator>();
        }

        public ValidatorCollection(Validator validator)
        {
            _validators = new Queue<Validator>();
            _validators.Enqueue(validator);
        }

        public ValidatorCollection(IEnumerable<Validator> validators)
        {
            _validators = new Queue<Validator>();
            Add(validators);
        }

        public ValidatorCollection Add(Validator validator)
        {
            _validators.Enqueue(validator);
            return this;
        }

        public ValidatorCollection Add(IEnumerable<Validator> validators)
        {
            foreach (var validator in validators)
            {
                _validators.Enqueue(validator);
            }
            return this;
        }

        public bool ObjectPassesValidation(object value)
        {
            return _validators.All(x => x.IsValid(value));
        }

        public string ObjectValidationErrors(object value)
        {
            return _validators
                .Where(x => !x.IsValid(value))
                .Select(x => x.GetMessage())
                .Aggregate((i, j) => i + j);
        }
    }
}