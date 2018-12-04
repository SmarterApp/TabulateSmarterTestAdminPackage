using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using ProcessSmarterTestPackage.Processors.Combined;
using ProcessSmarterTestPackage.Processors.Common;
using SmarterTestPackage.Common.Data;
using ValidateSmarterTestPackage.RestrictedValues.Enums;

namespace TabulateSmarterTestPackage.Models
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
                            "Test type is interim, but subtype is indeterminate. Interim test subtypes must be either 'ICA' or 'IAB'",
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

        public IDictionary<ItemFieldNames, string> RetrieveTestInformation(CombinedTestProcessor processor)
        {
            var result = new Dictionary<ItemFieldNames, string>();
            var testPackage = processor.TestPackage;
            var identifier = testPackage.Test[0];
            if (identifier == null)
            {
                Errors.Add(GenerateTestInformationValidationError(string.Empty, "Test node not found",
                    "Test", "TestPackage/Test", GetPackageType(processor)));
                result.Add(ItemFieldNames.AssessmentId, string.Empty);
                result.Add(ItemFieldNames.AssessmentName, string.Empty);
                result.Add(ItemFieldNames.AssessmentLabel, string.Empty);
                result.Add(ItemFieldNames.AssessmentVersion, string.Empty);
            }
            else
            {
                result.Add(ItemFieldNames.AssessmentId, $"({testPackage.publisher}){identifier.id}-{testPackage.academicYear}");
                result.Add(ItemFieldNames.AssessmentName, identifier.id);
                result.Add(ItemFieldNames.AssessmentLabel, identifier.label);
                result.Add(ItemFieldNames.AssessmentVersion, testPackage.version.ToString());
                result.Add(ItemFieldNames.Version, testPackage.version.ToString());
                result.Add(ItemFieldNames.AcademicYear, testPackage.academicYear);
                result.Add(ItemFieldNames.BankKey, testPackage.bankKey.ToString());
                result.Add(ItemFieldNames.AssessmentSubject, testPackage.subject);
                result.Add(ItemFieldNames.AssessmentGrade, identifier.Grades[0].value.ToString());
                result.Add(ItemFieldNames.AssessmentType, testPackage.type);
                if (!result[ItemFieldNames.AssessmentType].Equals("summative", StringComparison.OrdinalIgnoreCase))
                {
                    if ($"({testPackage.publisher}){identifier.id}-{testPackage.academicYear}".Contains("ICA"))
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
                            "Test type is interim, but subtype is indeterminate. Interim test subtypes must be either 'ICA' or 'IAB",
                            "TestPackage", "Test", GetPackageType(processor)));
                    }
                }
                else
                {
                    result.Add(ItemFieldNames.AssessmentSubtype, "summative");
                }

                foreach (var blueprint in testPackage.Blueprint)
                {
                    if (blueprint.Scoring != null && blueprint.Scoring.PerformanceLevels != null)
                    {
                        foreach (var performaceLevel in blueprint.Scoring.PerformanceLevels)
                        {
                            switch (performaceLevel.pLevel)
                            {
                                case 1:
                                    result[ItemFieldNames.CutPoint1] = performaceLevel.pLevel.ToString();
                                    result[ItemFieldNames.ScaledHigh1] = performaceLevel.scaledHi.ToString(CultureInfo.InvariantCulture);
                                    result[ItemFieldNames.ScaledLow1] = performaceLevel.scaledLo.ToString(CultureInfo.InvariantCulture);
                                    break;
                                case 2:
                                    result[ItemFieldNames.CutPoint2] = performaceLevel.pLevel.ToString();
                                    result[ItemFieldNames.ScaledHigh2] = performaceLevel.scaledHi.ToString(CultureInfo.InvariantCulture);
                                    result[ItemFieldNames.ScaledLow2] = performaceLevel.scaledLo.ToString(CultureInfo.InvariantCulture);
                                    break;
                                case 3:
                                    result[ItemFieldNames.CutPoint3] = performaceLevel.pLevel.ToString();
                                    result[ItemFieldNames.ScaledHigh3] = performaceLevel.scaledHi.ToString(CultureInfo.InvariantCulture);
                                    result[ItemFieldNames.ScaledLow3] = performaceLevel.scaledLo.ToString(CultureInfo.InvariantCulture);
                                    break;
                                case 4:
                                    result[ItemFieldNames.CutPoint4] = performaceLevel.pLevel.ToString();
                                    result[ItemFieldNames.ScaledHigh4] = performaceLevel.scaledHi.ToString(CultureInfo.InvariantCulture);
                                    result[ItemFieldNames.ScaledLow4] = performaceLevel.scaledLo.ToString(CultureInfo.InvariantCulture);
                                    break;

                            }
                        }
                    }
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