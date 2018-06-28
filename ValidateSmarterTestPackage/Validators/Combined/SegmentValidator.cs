using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using SmarterTestPackage.Common.Data;
using ValidateSmarterTestPackage.RestrictedValues.Enums;

namespace ValidateSmarterTestPackage.Validators.Combined
{
    public class SegmentValidator : ITestPackageValidator
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly HashSet<string> _knownAlgorithmTypes = new HashSet<string>
        {
            Algorithm.FIXEDFORM,
            Algorithm.ADAPTIVE
        };

        public void Validate(TestPackage testPackage, List<ValidationError> errors)
        {

            List<TestSegment> segments = new List<TestSegment>();
            foreach (var test in testPackage.Test)
            {
               segments.AddRange(test.Segments);
            }

            ValidateSegmentIdLength(segments, errors);
            ValidateSegmentsHaveUniqueIds(testPackage, errors);
            ValidateFixedFormSegmentsHaveSegmentForms(segments, errors);
            ValidateAdaptiveSegmentsHaveItemPool(segments, errors);
            ValidateKnownAlgorithms(segments, errors);
        }

        private void ValidateKnownAlgorithms(List<TestSegment> segments, List<ValidationError> errors)
        {
            foreach (var segment in segments)
            {
                if (!_knownAlgorithmTypes.Contains(segment.algorithmType))
                {
                    var errStr = $"Unrecognized algorithm type {segment.algorithmType} found for segment {segment.id}.";
                    errors.Add(new ValidationError
                    {
                        ErrorSeverity = ErrorSeverity.Benign,
                        Location = "TestPackage/Test/Segments/Segment",
                        GeneratedMessage = errStr,
                        ItemId = segment.id,
                        Key = "Segment",
                        PackageType = PackageType.Combined,
                        Value = segment.algorithmType
                    });
                }
            }
        }

        private void ValidateAdaptiveSegmentsHaveItemPool(List<TestSegment> segments, List<ValidationError> errors)
        {
            foreach (var segment in segments)
            {
                var segmentForms =
                    segment.Item is TestSegmentSegmentForms forms ? forms.SegmentForm : null;
                if (segmentForms != null)
                {
                    if (segment.algorithmType.Equals(Algorithm.ADAPTIVE,
                        StringComparison.CurrentCultureIgnoreCase))
                    {
                        var itemGroups = (segment.Item as TestSegmentPool)?.ItemGroup;
                        if (itemGroups == null || itemGroups.Length < 1)
                        {
                            var errStr = $"The segment{segment.id} has an algorithm type of \"adaptive\" " +
                                         "but did not contain any item pool defined";
                            errors.Add(new ValidationError
                            {
                                ErrorSeverity = ErrorSeverity.Severe,
                                Location = "TestPackage/Test/Segments/Segment",
                                GeneratedMessage = errStr,
                                ItemId = segment.id,
                                Key = "Segment",
                                PackageType = PackageType.Combined,
                                Value = segment.id
                            });
                        }
                    }
                }

            }
        }

        private void ValidateFixedFormSegmentsHaveSegmentForms(List<TestSegment> segments, List<ValidationError> errors)
        {
            foreach (var segment in segments)
            {
                if (segment.algorithmType.Equals(Algorithm.FIXEDFORM, StringComparison.InvariantCultureIgnoreCase))
                {
                    var segmentForms =
                        segment.Item is TestSegmentSegmentForms forms ? forms.SegmentForm : null;
                    if (segmentForms != null && segmentForms.Length < 1)
                    {
                        var errStr = $"The segment {segment.id} has an algorithm type of \"fixed form\" " +
                                     "but did not contain any forms";
                        errors.Add(new ValidationError
                        {
                            ErrorSeverity = ErrorSeverity.Severe,
                            Location = "TestPackage/Test/Segments/Segment",
                            GeneratedMessage = errStr,
                            ItemId = segment.id,
                            Key = "Segment",
                            PackageType = PackageType.Combined,
                            Value = segment.id
                        });
                    }
                }
            }
        }

        private void ValidateSegmentsHaveUniqueIds(TestPackage testPackage, List<ValidationError> errors)
        {
            foreach (var test in testPackage.Test)
            {
                if (test.Segments.Length > 1)
                {
                    errors.AddRange(from segment in test.Segments
                        where segment.id.Equals(test.id, StringComparison.CurrentCultureIgnoreCase)
                        let errStr = "A test/assessment with more than one segment cannot have an assessment " + $"id that matches a segment id. Segment ids must be unique. Test/assessment id: {segment.id}"
                        select new ValidationError
                        {
                            ErrorSeverity = ErrorSeverity.Severe,
                            Location = "TestPackage/Test/Segments/Segment",
                            GeneratedMessage = errStr,
                            ItemId = segment.id,
                            Key = "Segment",
                            PackageType = PackageType.Combined,
                            Value = segment.id
                        });
                }
            }
        }

        private void ValidateSegmentIdLength(List<TestSegment> segments, List<ValidationError> errors)
        {
            var maxLengthValidator = new MaxLengthValidator(ErrorSeverity.Severe, 255);
            foreach (var segment in segments)
            {
                if (segment.id != null && !maxLengthValidator.IsValid(segment.id))
                {
                    var errStr =
                        $"Cannot have a segment id ({segment.id}) that exceeds 255 characters.";
                    errors.Add(new ValidationError
                    {
                        ErrorSeverity = ErrorSeverity.Severe,
                        Location = "TestPackage/Test/Segments/Segment",
                        GeneratedMessage = errStr,
                        ItemId = segment.id,
                        Key = "Segment",
                        PackageType = PackageType.Combined,
                        Value = segment.id
                    });
                }
            }
            
        }
    }
}