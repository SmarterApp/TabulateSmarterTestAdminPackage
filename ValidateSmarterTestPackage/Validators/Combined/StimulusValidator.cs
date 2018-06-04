using System;
using System.Collections.Generic;
using NLog;
using SmarterTestPackage.Common.Data;
using ValidateSmarterTestPackage.RestrictedValues.Enums;

namespace ValidateSmarterTestPackage.Validators.Combined
{
    public class StimulusValidator : ITestPackageValidator
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public void Validate(TestPackage testPackage, List<ValidationError> errors)
        {
            List<ItemGroupStimulus> stimuli = new List<ItemGroupStimulus>();
            foreach (var test in testPackage.Test)
            {
                foreach (var segment in test.Segments)
                {
                    var segmentForms =
                        segment.Item is TestSegmentSegmentForms forms ? forms.SegmentForm : null;
                    if (segmentForms != null)
                    {
                        if (segment.algorithmType.Equals(Algorithm.FIXEDFORM,
                            StringComparison.CurrentCultureIgnoreCase))
                        {
                            foreach (var segmentForm in segmentForms)
                            {
                                foreach (var itemgroup in segmentForm.ItemGroup)
                                {
                                    stimuli.Add(itemgroup.Stimulus);
                                }
                            }
                        }
                        else //Assume adaptive
                        {
                            var itemGroups = (segment.Item as TestSegmentPool)?.ItemGroup;
                            if (itemGroups != null)
                                foreach (var itemGroup in itemGroups)
                                {
                                    stimuli.Add(itemGroup.Stimulus);
                                }
                        }
                    }
                }
            }

            foreach (var stimulus in stimuli)
            {
                try
                {
                    if (!Int64.TryParse(stimulus.id, out var l))
                    {
                        var errStr =
                            $"The stimulus with id \"{stimulus.id}\" has an id that is not a LONG value. " +
                            "Currently, TDS only supports stimuli  ids that are of a 'LONG' data type";
                        Logger.Debug(errStr);
                        errors.Add(new ValidationError
                        {
                            ErrorSeverity = ErrorSeverity.Severe,
                            Location = "TestPackage/Test/Segments/Segment/SegmentForms/SegmentForm/ItemGroup/Stimulus",
                            GeneratedMessage = errStr,
                            ItemId = stimulus.id,
                            Key = "Stimulus",
                            PackageType = PackageType.Combined,
                            Value = stimulus.id
                        });
                    }
                }
                catch (Exception)
                {
                    var errStr =
                        $"The stimulus with id \"{stimulus.id}\" has an id that is not a LONG value. " +
                        "Currently, TDS only supports stimuli  ids that are of a 'LONG' data type";
                    Logger.Debug(errStr);
                    errors.Add(new ValidationError
                    {
                        ErrorSeverity = ErrorSeverity.Severe,
                        Location = "TestPackage/Test/Segments/Segment/SegmentForms/SegmentForm/ItemGroup/Stimulus",
                        GeneratedMessage = errStr,
                        ItemId = stimulus.id,
                        Key = "Stimulus",
                        PackageType = PackageType.Combined,
                        Value = stimulus.id
                    });
                }
            }
        }
    }
}