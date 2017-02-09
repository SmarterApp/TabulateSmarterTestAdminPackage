﻿using System.Collections.Generic;
using System.Linq;

namespace TabulateSmarterTestAdminPackage.Common.Validators
{
    public class ValidatorCollection : List<Validator>, IValidator
    {
        private string _message;

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
            }
            else
            {
                _message = null;
            }


            return isValid;
        }

        public bool IsValid(object value, bool isRequired)
        {
            return IsValid(value) || !isRequired && value == null;
        }

        public ValidatorCollection AddAndReturn(Validator validator)
        {
            Add(validator);
            return this;
        }
    }
}