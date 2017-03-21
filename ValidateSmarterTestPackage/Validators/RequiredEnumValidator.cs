using System;
using System.Collections.Generic;
using System.Linq;
using SmarterTestPackage.Common.Data;
using ValidateSmarterTestPackage.RestrictedValues.Enums;
using ValidateSmarterTestPackage.RestrictedValues.RestrictedList;

namespace ValidateSmarterTestPackage.Validators
{
    public class RequiredEnumValidator : Validator
    {
        public RequiredEnumValidator(ErrorSeverity errorSeverity, object parameter = null)
            : base(errorSeverity, parameter) {}

        public override bool IsValid(object value)
        {
            {
                RestrictedListItems restrictedListItem;
                return Enum.TryParse(Parameter.ToString(), out restrictedListItem) &&
                       RestrictedList.RestrictedLists[restrictedListItem].Any(
                           x => x.Equals(value as string, StringComparison.OrdinalIgnoreCase));
            }
        }

        public override string GetMessage()
        {
            RestrictedListItems restrictedListItem;
            return !Enum.TryParse(Parameter.ToString(), out restrictedListItem)
                ? $"[IncorrectArgumentProvidedToValidator:{Parameter}]"
                : $"[RequiredValueIn:{StringifyList(RestrictedList.RestrictedLists[restrictedListItem].ToList())}]";
        }

        private string StringifyList(List<string> list)
        {
            var result = string.Empty;
            list.ForEach(x => result = $"{result}<{x}>");
            return $"({result})";
        }
    }
}