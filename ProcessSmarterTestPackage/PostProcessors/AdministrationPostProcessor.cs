using System;
using System.Collections.Generic;
using System.Linq;
using ProcessSmarterTestPackage.Processors.Common;
using SmarterTestPackage.Common.Data;

namespace ProcessSmarterTestPackage.PostProcessors
{
    public class AdministrationPostProcessor : PostProcessor
    {
        public AdministrationPostProcessor(PackageType packageType, Processor processor) : base(packageType, processor) {}

        public override IList<ValidationError> GenerateErrors()
        {
            var result = new List<ValidationError>();

            var nonBrailleTestForms =
                Processor.ChildNodesWithName("testform")
                    .Where(
                        x =>
                            x.ChildNodesWithName("property")
                                .Any(
                                    y =>
                                        y.ValueForAttribute("name")
                                            .Equals("language", StringComparison.OrdinalIgnoreCase) &&
                                        y.ValueForAttribute("value").ToLower().Contains("braille")));
            var testFormItemGroups =
                nonBrailleTestForms.SelectMany(x => x.ChildNodesWithName("formpartition"))
                    .SelectMany(x => x.ChildNodesWithName("itemgroup"));
            var segmentPoolItemGroups =
                Processor.ChildNodesWithName("adminsegment")
                    .SelectMany(x => x.ChildNodesWithName("segmentpool"))
                    .SelectMany(x => x.ChildNodesWithName("itemgroup"));
            var indexGroupItemInfo = new Dictionary<string, GroupItemInfo>();

            var combinedItemGroups = new List<Processor>();
            combinedItemGroups.AddRange(testFormItemGroups);
            combinedItemGroups.AddRange(segmentPoolItemGroups);

            foreach (var itemGroup in combinedItemGroups)
            {
                foreach (var info in GetInfo(itemGroup))
                {
                    GroupItemInfo gii;
                    if (indexGroupItemInfo.TryGetValue(info.ItemId, out gii))
                    {
                        // Legacy validations
                        if (!string.Equals(gii.IsFieldTest, info.IsFieldTest, StringComparison.OrdinalIgnoreCase))
                        {
                            result.Add(GenerateGroupItemValidationError(info,
                                $"Conflicting isfieldtest info for same itemId {info.ItemId} between forms: '{info.IsFieldTest}' != '{gii.IsFieldTest}'",
                                "isfieldtest", itemGroup.PackageType, itemGroup.Navigator.OuterXml));
                        }
                        if (!string.Equals(gii.IsActive, info.IsActive, StringComparison.OrdinalIgnoreCase))
                        {
                            result.Add(GenerateGroupItemValidationError(info,
                                $"Conflicting isActive info for same itemId {info.ItemId} between forms: '{info.IsActive}' != '{gii.IsActive}'",
                                "isactive", itemGroup.PackageType, itemGroup.Navigator.OuterXml));
                        }
                        if (
                            !string.Equals(gii.ResponseRequired, info.ResponseRequired,
                                StringComparison.OrdinalIgnoreCase))
                        {
                            result.Add(GenerateGroupItemValidationError(info,
                                $"Conflicting responseRequired info for same itemId {info.ItemId} between forms: '{info.ResponseRequired}' != '{gii.ResponseRequired}'",
                                "responserequired", itemGroup.PackageType, itemGroup.Navigator.OuterXml));
                        }
                        if (!string.Equals(gii.AdminRequired, info.AdminRequired, StringComparison.OrdinalIgnoreCase))
                        {
                            result.Add(GenerateGroupItemValidationError(info,
                                $"Conflicting adminRequired info for same itemId {info.ItemId} between forms: '{info.AdminRequired}' != '{gii.AdminRequired}'",
                                "adminrequired", itemGroup.PackageType, itemGroup.Navigator.OuterXml));
                        }
                        if (!string.Equals(gii.FormPosition, info.FormPosition, StringComparison.OrdinalIgnoreCase))
                        {
                            result.Add(GenerateGroupItemValidationError(info,
                                $"Conflicting formPosition info for same itemId {info.ItemId} between forms: '{info.FormPosition}' != '{gii.FormPosition}'",
                                "formposition", itemGroup.PackageType, itemGroup.Navigator.OuterXml));
                        }
                    }
                    else
                    {
                        indexGroupItemInfo.Add(info.ItemId, info);
                    }
                }
            }

            return result;
        }

        private IEnumerable<GroupItemInfo> GetInfo(Processor processor)
        {
            var result = new List<GroupItemInfo>();

            var groupId = processor.ChildNodeWithName("identifier").ValueForAttribute("uniqueid");
            processor.ChildNodesWithName("groupitem").ToList().ForEach(x => result.Add(new GroupItemInfo
            {
                AdminRequired = x.ValueForAttribute("adminrequired"),
                FormPosition = x.ValueForAttribute("formposition"),
                GroupId = groupId,
                IsActive = x.ValueForAttribute("isactive"),
                IsFieldTest = x.ValueForAttribute("isfieldtest"),
                ResponseRequired = x.ValueForAttribute("responserequired"),
                ItemId = x.ValueForAttribute("itemid")
            }));

            return result;
        }

        private ValidationError GenerateGroupItemValidationError(GroupItemInfo info, string violation, string key,
            PackageType packageType, string value)
        {
            return new ValidationError
            {
                ErrorSeverity = ErrorSeverity.Degraded,
                GeneratedMessage = $"[GroupItem {info.ItemId} within itemgroup {info.GroupId} violated {violation}]",
                ItemId = info.ItemId,
                Key = key,
                Location =
                    "adminsegment/segmentpool/itemgroup/groupitem || testspecification/administration/testform/formpartition/itemgroup/groupitem",
                PackageType = packageType,
                Value = value
            };
        }
    }
}