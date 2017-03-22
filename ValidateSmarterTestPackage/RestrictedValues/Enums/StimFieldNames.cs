namespace ValidateSmarterTestPackage.RestrictedValues.Enums
{
    public enum StimFieldNames
    {
        AssessmentName,
        AssessmentSubject,
        AssessmentGrade,
        AssessmentType, // PT or Summative
        StimuliId, // Strip the "200-" bankId prefix
        FileName,
        Version
    }
}