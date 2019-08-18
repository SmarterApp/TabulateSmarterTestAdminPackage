namespace ValidateSmarterTestPackage.RestrictedValues.Enums
{
    public enum ItemFieldNames
    {
        AssessmentId,
        AssessmentName,
        AssessmentSubject,
        AssessmentGrade,
        AssessmentType, // PT or Summative
        AssessmentSubtype,
        AssessmentLabel,
        AssessmentVersion,
        AcademicYear,
        FullItemKey,
        BankKey, //10
        ItemId, // Strip the "200-" bankId prefix
        Filename,
        Version,
        ItemType,
        Grade,
        Standard,
        Claim,
        Target,
        PassageId,
        ASL, //20
        Braille,
        LanguageBraille,
        DOK,
        Language,
        AllowCalculator,
        MathematicalPractice,
        MaxPoints,
        Glossary,
        ScoringEngine,
        Spanish, //30
        IsFieldTest,
        IsActive,
        ResponseRequired,
        AdminRequired,
        FormPosition, // ignore for RDW 35
        ItemPosition,
        MeasurementModel_1, 
        Weight_1,
        dimension_1, // ignore for RDW 39
        ScorePoints_1, // 40
        a,
        b0_b,
        b1_c,
        b2,
        b3,
        avg_b,
        MeasurementModel_d2, // ignore for RDW 47
        Weight_d2, // ignore for RDW 48
        dimension_d2, // ignore for RDW 49
        ScorePoints_d2, // ignore for RDW 50
        a_d2, // ignore for RDW 51 
        b0_d2, // ignore for RDW 52
        b1_d2, // ignore for RDW 53
        b2_d2, // ignore for RDW 54
        b3_d2, // ignore for RDW 55
        bpref1,
        bpref2,
        bpref3,
        bpref4,
        bpref5, //60
        bpref6,
        bpref7,
        CommonCore,
        ClaimContentTarget,
        SecondaryCommonCore,
        SecondaryClaimContentTarget,
        AnswerKey,
        NumberOfAnswerOptions, //68
        PtWritingType,
        CutPoint1, //70
        ScaledLow1,
        ScaledHigh1,
        CutPoint2,
        ScaledLow2,
        ScaledHigh2,
        CutPoint3,
        ScaledLow3,
        ScaledHigh3,
        CutPoint4,
        ScaledLow4, //80
        ScaledHigh4,
        HandScored, // ignore for RDW 82
        DoNotScore // ignore for RDW 83
    }

    public enum ItemFieldNamesRDW
    {
        AssessmentId,
        AssessmentName,
        AssessmentSubject,
        AssessmentGrade,
        AssessmentType, // PT or Summative
        AssessmentSubtype,
        AssessmentLabel,
        AssessmentVersion,
        AcademicYear,
        FullItemKey,
        BankKey,
        ItemId, // Strip the "200-" bankId prefix
        Filename,
        Version,
        ItemType,
        Grade,
        Standard,
        Claim,
        Target,
        PassageId,
        ASL,
        Braille,
        LanguageBraille,
        DOK,
        Language,
        AllowCalculator,
        MathematicalPractice,
        MaxPoints,
        Glossary,
        ScoringEngine,
        Spanish,
        IsFieldTest,
        IsActive,
        ResponseRequired,
        AdminRequired,
        ItemPosition,
        MeasurementModel,
        Weight,
        ScorePoints,
        a,
        b0_b,
        b1_c,
        b2,
        b3,
        avg_b,
        bpref1,
        bpref2,
        bpref3,
        bpref4,
        bpref5,
        bpref6,
        bpref7,
        CommonCore,
        ClaimContentTarget,
        SecondaryCommonCore,
        SecondaryClaimContentTarget,
        AnswerKey,
        NumberOfAnswerOptions,
        PerformanceTask,
        PtWritingType,
        CutPoint1,
        ScaledLow1,
        ScaledHigh1,
        CutPoint2,
        ScaledLow2,
        ScaledHigh2,
        CutPoint3,
        ScaledLow3,
        ScaledHigh3,
        CutPoint4,
        ScaledLow4,
        ScaledHigh4
    }
}