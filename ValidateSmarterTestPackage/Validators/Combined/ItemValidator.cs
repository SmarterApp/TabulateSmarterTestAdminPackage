using System;
using System.Collections.Generic;
using NLog;
using SmarterTestPackage.Common.Data;
using ValidateSmarterTestPackage.RestrictedValues.Enums;

namespace ValidateSmarterTestPackage.Validators.Combined
{
    public class ItemValidator : ITestPackageValidator
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly string[] ItemTypes = new string[]
        {
            "ER",
            "GI",
            "MI",
            "MS",
            "EQ",
            "MC",
            "TI",
            "SA",
            "EBSR",
            "WER",
            "HTQ"
        };
        private static HashSet<string> RECOGNIZED_ITEM_TYPES = new HashSet<string>(ItemTypes);

        public void Validate(TestPackage testPackage, List<ValidationError> errors)
        {
            List<ItemGroupItem> items = new List<ItemGroupItem>();
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
                                    items.AddRange(itemgroup.Item);
                                }
                            }
                        }
                        else
                        {
                            var itemGroups = (segment.Item as TestSegmentPool)?.ItemGroup;
                            if (itemGroups != null)
                                foreach (var itemGroup in itemGroups)
                                {
                                    items.AddRange(itemGroup.Item);
                                }
                        }
                    }
                    
                }
            }

            validateItemIdsAreLongs(errors, items);
            validateItemTypesAreKnownTypes(errors, items);
        }

        private void validateItemTypesAreKnownTypes(List<ValidationError> errors, List<ItemGroupItem> items)
        {
            foreach (var item in items)
            {
                if (!RECOGNIZED_ITEM_TYPES.Contains(item.type.ToUpper()))
                {
                    var errStr =
                        $"The item \"{item.id}\" contained an item type \"{item.type}\" that is not a recognized TDS item type. Known item types include {String.Join(", ", RECOGNIZED_ITEM_TYPES)}";
                    Logger.Debug(errStr);
                    errors.Add(new ValidationError
                    {
                        ErrorSeverity = ErrorSeverity.Benign,
                        Location = "TestPackage/Test/Segments/SegmentForms/SegmentForm/ItemGroup/Item",
                        GeneratedMessage = errStr,
                        ItemId = item.id,
                        Key = "Item",
                        PackageType = PackageType.Combined,
                        Value = item.type
                    });
                }
            }
        }

        private void validateItemIdsAreLongs(List<ValidationError> errors, List<ItemGroupItem> items)
        {
            foreach (var item in items)
            {
                Logger.Debug($"Item id is {item.id}");
                try
                {
                    if (!Int64.TryParse(item.id, out var l))
                    {
                        var errStr =
                            $"The item with id \"{item.id}\" has an id that is not a LONG value. " +
                            "Currently, TDS only supports item ids that are of a 'LONG' data type";
                        Logger.Debug(errStr);
                        errors.Add(new ValidationError
                        {
                            ErrorSeverity = ErrorSeverity.Severe,
                            Location = "TestPackage/Test/Segments/SegmentForms/SegmentForm/ItemGroup/Item",
                            GeneratedMessage = errStr,
                            ItemId = item.id,
                            Key = "Item",
                            PackageType = PackageType.Combined,
                            Value = item.id
                        });
                    }
                }
                catch (Exception)
                {
                    var errStr =
                        $"The item with id \"{item.id}\" has an id that is not a LONG value. " +
                        "Currently, TDS only supports item ids that are of a 'LONG' data type";
                    Logger.Debug(errStr);
                    errors.Add(new ValidationError
                    {
                        ErrorSeverity = ErrorSeverity.Severe,
                        Location = "TestPackage/Test/Segments/SegmentForms/SegmentForm/ItemGroup/Item",
                        GeneratedMessage = errStr,
                        ItemId = item.id,
                        Key = "Item",
                        PackageType = PackageType.Combined,
                        Value = item.id
                    });
                }
            }
        }
    }
}