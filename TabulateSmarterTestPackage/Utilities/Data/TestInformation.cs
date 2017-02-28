using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ProcessSmarterTestPackage.Processors.Common;
using ValidateSmarterTestPackage.RestrictedValues.Enums;

namespace TabulateSmarterTestPackage.Utilities.Data
{
    public static class TestInformation
    {
        public static IDictionary<ItemFieldNames, string> RetrieveTestInformation(TestSpecificationProcessor processor)
        {
            var result = new Dictionary<ItemFieldNames, string>();

            var identifier = processor.ChildNodeWithName("identifier");
            result.Add(ItemFieldNames.AssessmentId, identifier.ValueForAttribute("uniqueid"));
            result.Add(ItemFieldNames.AssessmentName, identifier.ValueForAttribute("name"));
            result.Add(ItemFieldNames.AssessmentLabel, identifier.ValueForAttribute("label"));
            result.Add(ItemFieldNames.AssessmentVersion, identifier.ValueForAttribute("version"));

            var assessmentYear = Regex.Match(result[ItemFieldNames.AssessmentId], @"(\d{4}-\d{4})");
            result.Add(ItemFieldNames.AcademicYear,
                assessmentYear.Captures.Count != 0
                    ? assessmentYear.Captures[0].Value
                    : string.Empty);

            var subjectProperty = processor
                .ChildNodeWithNameAndPropertyValue("property", "name", "subject");
            result.Add(ItemFieldNames.AssessmentSubject, subjectProperty.ValueForAttribute("value"));

            var gradeProperty = processor
                .ChildNodeWithNameAndPropertyValue("property", "name", "grade");
            result.Add(ItemFieldNames.AssessmentGrade, gradeProperty.ValueForAttribute("value"));

            var typeProperty = processor
                .ChildNodeWithNameAndPropertyValue("property", "name", "type");
            result.Add(ItemFieldNames.AssessmentType, typeProperty.ValueForAttribute("value"));

            if (!result[ItemFieldNames.AssessmentType].Equals("summative", StringComparison.OrdinalIgnoreCase))
            {
                if (result[ItemFieldNames.AssessmentId].Contains("ICA"))
                {
                    result.Add(ItemFieldNames.AssessmentSubtype, "ICA");
                }
                else if (result[ItemFieldNames.AssessmentId].Contains("IAB"))
                {
                    result.Add(ItemFieldNames.AssessmentSubtype, "IAB");
                }
                else
                {
                    result.Add(ItemFieldNames.AssessmentSubtype, string.Empty);
                    // This is an error condition! Write an error.
                }
            }
            else
            {
                result.Add(ItemFieldNames.AssessmentSubtype, "summative");
            }

            return result;
        }
    }
}