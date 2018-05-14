using System;
using System.Collections.Generic;
using SmarterTestPackage.Common.Data;
using ValidateSmarterTestPackage.Resources;
using ValidateSmarterTestPackage.RestrictedValues.Enums;
using NLog;

namespace ValidateSmarterTestPackage.Validators.Combined
{
    public class ItemGroupValidator : ITestPackageValidator
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public void Validate(TestPackage testPackage, List<ValidationError> errors)
        {
            List<ItemGroup> itemGroups = new List<ItemGroup>();
            Test[] tests = testPackage.Test;
            foreach (var test in tests)
            {
                TestSegment[] testSegments = test.Segments;
                foreach (var segment in testSegments)
                {
                    if (segment.algorithmType.Equals(Algorithm.FIXEDFORM, StringComparison.InvariantCultureIgnoreCase))
                    {
                        var segmentForms =
                            segment.Item is TestSegmentSegmentForms forms ? forms.SegmentForm : null;
                        if (segmentForms != null)
                            foreach (var segmentForm in segmentForms)
                            {
                                itemGroups.AddRange(segmentForm.ItemGroup);
                            }
                    }
                    else
                    {
                        itemGroups.AddRange(
                            ((TestSegmentPool)
                                segment.Item).ItemGroup);

                    }
                }
            }
            ValidateItemGroupIdsAreLongs(errors, itemGroups);
            ValidateMaxItemsAndResponses(errors, itemGroups);
        }

        private void ValidateMaxItemsAndResponses(List<ValidationError> errors, List<ItemGroup> itemGroups)
        {
            foreach (var itemGroup in itemGroups)
            {
                if (!itemGroup.maxItems.Equals("ALL", StringComparison.InvariantCultureIgnoreCase) && (!long.TryParse(itemGroup.maxItems, out long n)))
                {
                    var errStr =
                        "The item group with id " + itemGroup.id + " contains a \"maxItems\" value that is neither 'ALL' nor numeric.";
                    Logger.Debug(errStr);
                    errors.Add(new ValidationError
                    {
                        ErrorSeverity = ErrorSeverity.Severe,
                        Location = "TestPackage/Test/Segments/SegmentForms/SegmentForm/ItemGroup",
                        GeneratedMessage =
                            errStr,
                        ItemId =
                            itemGroup.id,
                        Key = "ItemGroup",
                        PackageType = PackageType.Combined,
                        Value = itemGroup.maxItems
                    });
                }

                

            }
        }

        private void ValidateItemGroupIdsAreLongs(List<ValidationError> errors, List<ItemGroup> itemGroups)
        {
            foreach (var itemGroup in itemGroups)
            {
                if (!itemGroup.maxResponses.Equals("ALL", StringComparison.InvariantCultureIgnoreCase) && (!long.TryParse(itemGroup.maxResponses, out long m)))
                {
                    var errStr =
                        "The item group with id " + itemGroup.id + " contains a \"maxResponses\" value that is neither 'ALL' nor numeric.";
                    Logger.Debug(errStr);
                    errors.Add(new ValidationError
                    {
                        ErrorSeverity = ErrorSeverity.Severe,
                        Location = "TestPackage/Test/Segments/Segment/Pool/ItemGroup",
                        GeneratedMessage =
                            errStr,
                        ItemId =
                            itemGroup.id,
                        Key = "passageref",
                        PackageType = PackageType.Combined,
                        Value = itemGroup.maxResponses
                    });
                }
            }
        }

    }
}
