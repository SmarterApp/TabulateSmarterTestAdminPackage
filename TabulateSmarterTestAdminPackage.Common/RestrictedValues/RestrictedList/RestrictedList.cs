using System.Collections.Generic;
using TabulateSmarterTestAdminPackage.Common.RestrictedValues.Enums;
using TabulateSmarterTestAdminPackage.Common.Validators;

namespace TabulateSmarterTestAdminPackage.Common.RestrictedValues.RestrictedList
{
    public static class RestrictedList
    {
        public static IDictionary<RestrictedListItems, IList<string>> RestrictedLists { get; set; } =
            new Dictionary<RestrictedListItems, IList<string>>();

        public static void Initialize()
        {
            RestrictedLists.Add(RestrictedListItems.Grade, new List<string>
            {
                "IT",
                "PR",
                "PK",
                "TK",
                "KG",
                "01",
                "02",
                "03",
                "04",
                "05",
                "06",
                "07",
                "08",
                "09",
                "10",
                "11",
                "12",
                "13",
                "PS",
                "ABE",
                "ASE",
                "AdultESL",
                "UG",
                "Other"
            });
            RestrictedLists.Add(RestrictedListItems.Subject, new List<string>
            {
                "ELA",
                "MATH",
                "Student Help"
            });
        }
    }
}