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
            if (Parameter is Enum)
            {
                return false;
            }
            {
                if (Parameter is RestrictedListItems)
                {
                    RestrictedListItems restrictedListItem;
                    Enum.TryParse(Parameter.ToString(), out restrictedListItem);
                    return RestrictedList.RestrictedLists[restrictedListItem].Any(
                        x => x.Equals(value as string, StringComparison.OrdinalIgnoreCase));
                }
            }
            return Enum.GetNames(Parameter.GetType()).Any(
                x => x.Equals(value as string, StringComparison.OrdinalIgnoreCase));
        }

        public override string GetMessage()
        {
            if (!(Parameter is Enum))
            {
                return $"[IncorrectArgumentProvidedToValidator:{Parameter}]";
            }
            if (!(Parameter is RestrictedListItems))
            {
                return $"[RequiredValueIn:{Enum.GetNames(Parameter.GetType())}]";
            }
            RestrictedListItems restrictedListItem;
            Enum.TryParse(Parameter.ToString(), out restrictedListItem);
            return
                $"[RequiredValueIn:{RestrictedList.RestrictedLists[restrictedListItem]}";
        }
    }
}