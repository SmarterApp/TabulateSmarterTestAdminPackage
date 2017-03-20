using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ProcessSmarterTestPackage.Processors.Common;
using SmarterTestPackage.Common.Data;
using ValidateSmarterTestPackage.RestrictedValues.Enums;

namespace TabulateSmarterTestPackage.Data
{
    public class TestInformation
    {
        public IList<ValidationError> Errors { get; set; } = new List<ValidationError>();

        public IDictionary<ItemFieldNames, string> RetrieveTestInformation(TestSpecificationProcessor processor)
        {
            var result = new Dictionary<ItemFieldNames, string>();

            var identifier = processor.ChildNodeWithName("identifier");
            if (identifier == null)
            {
                Errors.Add(GenerateTestInformationValidationError(string.Empty, "Identifier node not found",
                    "identifier", "testspecification/identifier", GetPackageType(processor)));
                result.Add(ItemFieldNames.AssessmentId, string.Empty);
                result.Add(ItemFieldNames.AssessmentName, string.Empty);
                result.Add(ItemFieldNames.AssessmentLabel, string.Empty);
                result.Add(ItemFieldNames.AssessmentVersion, string.Empty);
            }
            else
            {
                result.Add(ItemFieldNames.AssessmentId, identifier.ValueForAttribute("uniqueid"));
                result.Add(ItemFieldNames.AssessmentName, identifier.ValueForAttribute("name"));
                result.Add(ItemFieldNames.AssessmentLabel, identifier.ValueForAttribute("label"));
                result.Add(ItemFieldNames.AssessmentVersion, identifier.ValueForAttribute("version"));
            }

            var assessmentYear = Regex.Match(result[ItemFieldNames.AssessmentId], @"(\d{4}-\d{4})");
            result.Add(ItemFieldNames.AcademicYear,
                assessmentYear.Captures.Count != 0
                    ? assessmentYear.Captures[0].Value
                    : string.Empty);

            var subjectProperty = processor
                .ChildNodeWithNameAndPropertyValue("property", "name", "subject");
            if (subjectProperty == null)
            {
                Errors.Add(GenerateTestInformationValidationError(result[ItemFieldNames.AssessmentId],
                    "Required property \"subject\" not found", "property", "testspecification/property",
                    GetPackageType(processor)));
                result.Add(ItemFieldNames.AssessmentSubject, string.Empty);
            }
            else
            {
                result.Add(ItemFieldNames.AssessmentSubject, subjectProperty.ValueForAttribute("value"));
            }

            var gradeProperty = processor
                .ChildNodeWithNameAndPropertyValue("property", "name", "grade");
            if (gradeProperty == null)
            {
                Errors.Add(GenerateTestInformationValidationError(result[ItemFieldNames.AssessmentId],
                    "Required property \"grade\" not found", "property", "testspecification/property",
                    GetPackageType(processor)));
                result.Add(ItemFieldNames.AssessmentGrade, string.Empty);
            }
            else
            {
                result.Add(ItemFieldNames.AssessmentGrade, gradeProperty.ValueForAttribute("value"));
            }

            var typeProperty = processor
                .ChildNodeWithNameAndPropertyValue("property", "name", "type");
            if (typeProperty == null)
            {
                Errors.Add(GenerateTestInformationValidationError(result[ItemFieldNames.AssessmentId],
                    "Required property \"type\" not found", "property", "testspecification/property",
                    GetPackageType(processor)));
                result.Add(ItemFieldNames.AssessmentType, string.Empty);
                result.Add(ItemFieldNames.AssessmentSubtype, string.Empty);
            }
            else
            {
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
                        Errors.Add(GenerateTestInformationValidationError(result[ItemFieldNames.AssessmentId],
                            "Test type is summative, but subtype is indeterminate. Summative test subtypes must be either \"ICA\" or \"IAB\"",
                            "property", "testspecification/property", GetPackageType(processor)));
                    }
                }
                else
                {
                    result.Add(ItemFieldNames.AssessmentSubtype, "summative");
                }
            }

            return result;
        }

        private static PackageType GetPackageType(Processor processor)
        {
            return processor.ValueForAttribute("purpose").Equals("scoring")
                ? PackageType.Scoring
                : PackageType.Administration;
        }

        public ValidationError GenerateTestInformationValidationError(string assessmentId, string message, string key,
            string location, PackageType packageType)
        {
            return new ValidationError
            {
                AssessmentId = assessmentId,
                ErrorSeverity = ErrorSeverity.Severe,
                GeneratedMessage = message,
                Key = key,
                Location = location,
                PackageType = packageType
            };
        }
    }
}