using System;
using System.Linq;
using TabulateSmarterTestAdminPackage.Common.RestrictedValues.Enums;
using TabulateSmarterTestAdminPackage.Common.RestrictedValues.RestrictedList;

namespace TabulateSmarterTestAdminPackage.Common.Validators
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
                : $"[RequiredValueIn:{RestrictedList.RestrictedLists[restrictedListItem]}";
        }
    }
}