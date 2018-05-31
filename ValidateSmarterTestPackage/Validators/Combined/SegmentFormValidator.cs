using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using SmarterTestPackage.Common.Data;
using ValidateSmarterTestPackage.RestrictedValues.Enums;

namespace ValidateSmarterTestPackage.Validators.Combined
{
    public class SegmentFormValidator : ITestPackageValidator
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public void Validate(TestPackage testPackage, List<ValidationError> errors)
        {
            List<TestSegmentSegmentFormsSegmentForm> segForms = new List<TestSegmentSegmentFormsSegmentForm>();
            foreach (var test in testPackage.Test)
            {
                TestSegment[] testSegments = test.Segments;
                foreach (var segment in testSegments)
                {
                    if (segment.algorithmType.Equals(Algorithm.FIXEDFORM, StringComparison.InvariantCultureIgnoreCase))
                    {
                        var segmentForms =
                            segment.Item is TestSegmentSegmentForms forms ? forms.SegmentForm : null;
                        if (segmentForms != null)
                        {
                            segForms.AddRange(segmentForms);
                        }
                    }
                }
            }

            validateSegmentFormIdsAreUnique(errors, segForms);
            validateItemPresentationsAreAlsoAtFormLevel(segForms, errors);
        }

        private void validateItemPresentationsAreAlsoAtFormLevel(List<TestSegmentSegmentFormsSegmentForm> segForms, List<ValidationError> errors)
        {
            foreach (var form in segForms)
            {
                HashSet<string> languageCodes = new HashSet<string>();
                foreach (var presentation in form.Presentations)
                {
                    languageCodes.Add(presentation.code);
                }
                foreach (var itemGroup in form.ItemGroup)
                {
                    foreach (var item in itemGroup.Item)
                    {
                        foreach (var presenation in item.Presentations)
                        {
                            if (!languageCodes.Contains(presenation.code))
                            {
                                var errStr = $"An item contained a presentation code \"{presenation.code}\" that was not declared at the segment form level. " +
                                             "A segment form presentation list must contain the set of all presentation codes within its contained items";
                                Logger.Debug(errStr);
                                errors.Add(new ValidationError
                                {
                                    ErrorSeverity = ErrorSeverity.Severe,
                                    Location = "TestPackage/Test/Segments/Segment/SegmentForms/SegmentForm/Presentations/Presentation",
                                    GeneratedMessage = errStr,
                                    ItemId = presenation.code,
                                    Key = "Presentation",
                                    PackageType = PackageType.Combined,
                                    Value = presenation.code
                                });
                            }
                        }
                    }
                }
            }
            
        }

        private void validateSegmentFormIdsAreUnique(List<ValidationError> errors, List<TestSegmentSegmentFormsSegmentForm> forms)
        {
            // If there are multiple segment forms with the same ID, we have a problem. They must be unique, certainly within the test package
            List<string> formIds = new List<string>();
            foreach (var form in forms)
            {
                formIds.Add(form.id);
            }

            if (formIds.Count != formIds.Distinct().Count())
            {
                var distinct = formIds.Distinct();
                foreach (var formId in formIds)
                {
                    IEnumerable<string> enumerable = distinct.ToList();
                    if (!enumerable.Contains(formId))
                    {
                        var errStr = $"Multiple segment forms with the same id were detected. Each segment form must contain a unique id.";
                        Logger.Debug(errStr);
                        errors.Add(new ValidationError
                        {
                            ErrorSeverity = ErrorSeverity.Severe,
                            Location = "TestPackage/Test/Segments/Segment/SegmentForms/SegmentForm",
                            GeneratedMessage = errStr,
                            ItemId = formId,
                            Key = "SegmentForm",
                            PackageType = PackageType.Combined,
                            Value = formId
                        });
                    }
                }
                
            }
        }
    }
}