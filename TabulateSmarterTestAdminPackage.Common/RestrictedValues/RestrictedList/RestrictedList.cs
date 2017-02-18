using System;
using System.Collections.Generic;
using TabulateSmarterTestAdminPackage.Common.RestrictedValues.Enums;

namespace TabulateSmarterTestAdminPackage.Common.RestrictedValues.RestrictedList
{
    public static class RestrictedList
    {
        public static IDictionary<RestrictedListItems, IList<string>> RestrictedLists { get; set; } =
            new Dictionary<RestrictedListItems, IList<string>>
            {
                {
                    RestrictedListItems.Grade, new List<string>
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
                    }
                },
                {
                    RestrictedListItems.Subject, new List<string>
                    {
                        "ELA",
                        "MATH",
                        "Student Help"
                    }
                },
                {
                    RestrictedListItems.BlueprintElementType, new List<string>
                    {
                        "test",
                        "segment",
                        "strand",
                        "contentlevel"
                    }
                },
                {
                    RestrictedListItems.ErrorSeverity, Enum.GetNames(typeof(ErrorSeverity))
                },
                {
                    RestrictedListItems.ItemFieldNames, Enum.GetNames(typeof(ItemFieldNames))
                },
                {
                    RestrictedListItems.ItemSelectionAlgorithm, new List<string>
                    {
                        "fixedform",
                        "adaptive",
                        "adaptive2"
                    }
                },
                {
                    RestrictedListItems.ItemType, new List<string>
                    {
                        "MC",
                        "MS",
                        "EQ",
                        "ER",
                        "GI",
                        "SA",
                        "NL",
                        "TI",
                        "MI",
                        "EBSR",
                        "WER",
                        "HTQ",
                        "HT"
                    }
                },
                {
                    RestrictedListItems.MeasurementModel, new List<string>
                    {
                        "RAWSCORE",
                        "IRT3PLn",
                        "IRTGPC",
                        "IRT3pl",
                        "IRTPCL",
                        "IRTGRL"
                    }
                },
                {
                    RestrictedListItems.MeasurementParameter, new List<string>
                    {
                        "a",
                        "b",
                        "c",
                        "b0",
                        "b1",
                        "b2",
                        "b3"
                    }
                },
                {
                    RestrictedListItems.PackageType, Enum.GetNames(typeof(PackageType))
                },
                {
                    RestrictedListItems.StimFieldNames, Enum.GetNames(typeof(StimFieldNames))
                },
                {
                    RestrictedListItems.TestType, new List<string>
                    {
                        "interim",
                        "summative"
                    }
                }
            };
    }
}