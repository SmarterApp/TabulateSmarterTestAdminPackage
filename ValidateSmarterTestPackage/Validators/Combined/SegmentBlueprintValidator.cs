using System.Collections.Generic;
using NLog;
using SmarterTestPackage.Common.Data;

namespace ValidateSmarterTestPackage.Validators.Combined
{
    public class SegmentBlueprintValidator : ITestPackageValidator
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public void Validate(TestPackage testPackage, List<ValidationError> errors)
        {
            List<TestSegmentSegmentBlueprintElement> segmentBlueprintElements = new List<TestSegmentSegmentBlueprintElement>();
            foreach (var test in testPackage.Test)
            {
                foreach (var segment in test.Segments)
                {
                    segmentBlueprintElements.AddRange(segment.SegmentBlueprint);
                }
            }
            validateMinExamItemCountLessThanMax(segmentBlueprintElements, errors);
            validateMinFieldTestItemCountLessThanMax(segmentBlueprintElements, errors);
            validateTestPackageContainsSegmentBlueprintForSegmentId(testPackage, errors);
        }

        private void validateTestPackageContainsSegmentBlueprintForSegmentId(TestPackage testPackage, List<ValidationError> errors)
        {
            List<string> segmentIds = new List<string>();
            List<string> segmentBlueprintIds = new List<string>();
            foreach (var test in testPackage.Test)
            {
                foreach (var segment in test.Segments)
                {
                    segmentIds.Add(segment.id);
                    foreach (var segmentBlueprint in segment.SegmentBlueprint)
                    {
                        segmentBlueprintIds.Add(segmentBlueprint.idRef);
                    }
                }
            }

            foreach (var segmentId in segmentIds)
            {
                if (!segmentBlueprintIds.Contains(segmentId))
                {
                    var errStr = $"No segment blueprint element was found with the id for the segment {segmentId}";
                    errors.Add(new ValidationError
                    {
                        ErrorSeverity = ErrorSeverity.Severe,
                        Location = "TestPackage/Test/Segments/Segment/SegmentBlueprint/SegmentBlueprintElement",
                        GeneratedMessage = errStr,
                        ItemId = segmentId,
                        Key = "SegmentBlueprintElement",
                        PackageType = PackageType.Combined,
                        Value = segmentId
                    });
                }
            }
            
        }

        private void validateMinFieldTestItemCountLessThanMax(List<TestSegmentSegmentBlueprintElement> segmentBlueprintElements, List<ValidationError> errors)
        {
            foreach (var segmentBp in segmentBlueprintElements)
            {
                if (segmentBp.minFieldTestItems > segmentBp.maxFieldTestItems)
                {
                    var errStr =
                        "Cannot have a minFieldTestItems value that " +
                        $"is greater than the maxFieldTestItems value for segment blueprint element with idRef {segmentBp.idRef}.";
                    errors.Add(new ValidationError
                    {
                        ErrorSeverity = ErrorSeverity.Severe,
                        Location = "TestPackage/Test/Segments/Segment/SegmentBlueprint/SegmentBlueprintElement",
                        GeneratedMessage = errStr,
                        ItemId = segmentBp.idRef,
                        Key = "SegmentBlueprintElement",
                        PackageType = PackageType.Combined,
                        Value = segmentBp.idRef
                    });
                }
            }
        }


        private void validateMinExamItemCountLessThanMax(List<TestSegmentSegmentBlueprintElement> segmentBlueprintElements, List<ValidationError> errors)
        {
            foreach (var segmentBp in segmentBlueprintElements)
            {
                if (segmentBp.minExamItems > segmentBp.maxExamItems)
                {
                    var errStr =
                        "Cannot have a minExamItem value that " +
                        $"is greater than the maxExamItem value for segment blueprint element with idRef {segmentBp.idRef}.";
                    errors.Add(new ValidationError
                    {
                        ErrorSeverity = ErrorSeverity.Severe,
                        Location = "TestPackage/Test/Segments/Segment/SegmentBlueprint/SegmentBlueprintElement",
                        GeneratedMessage = errStr,
                        ItemId = segmentBp.idRef,
                        Key = "SegmentBlueprintElement",
                        PackageType = PackageType.Combined,
                        Value = segmentBp.idRef
                    });
                }
            }
        }
    }
}