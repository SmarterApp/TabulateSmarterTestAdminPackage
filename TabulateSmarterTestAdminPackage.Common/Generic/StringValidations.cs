using System.Linq;

namespace TabulateSmarterTestAdminPackage.Common.Generic
{
    public static class StringValidations
    {
        public static bool NonemptyStringLessThanEqual(this string subject, int maxLength)
        {
            return subject.Any() 
                && subject.Length <= maxLength;
        }
    }
}
