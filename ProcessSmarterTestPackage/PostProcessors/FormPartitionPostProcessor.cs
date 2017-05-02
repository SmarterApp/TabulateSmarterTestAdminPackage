using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ProcessSmarterTestPackage.Processors.Common;
using SmarterTestPackage.Common.Data;

namespace ProcessSmarterTestPackage.PostProcessors
{
    public class FormPartitionPostProcessor : PostProcessor
    {
        public FormPartitionPostProcessor(PackageType packageType, Processor processor) : base(packageType, processor) {}

        public override IList<ValidationError> GenerateErrors()
        {
            var result = new List<ValidationError>();

            var partitionIdentifier = Processor.ChildNodeWithName("identifier").ValueForAttribute("uniqueid");
            var itemGroups = Processor.ChildNodesWithName("itemgroup").ToList();
            const string groupIdPattern = @"(\d+-\d+):([G|I])-(\d+-\d+)";

            /**
             * The following conditionals emit errors around group identifiers. 
             * In the Student application, groups identifiers whose second portion begins with "G-" are treated differently than those that start with "I-". 
             * Other identifier prefixes are not recognized. By convention, the first part of a group ID (the part preceding the colon) should be the form partition ID that the group belongs to. 
             * The part following should be either the associated stimuli ID found in the passageref element (for "G-" item groups) or the item ID of the single item in the group (for "I-" groups). 
             * Errors emitted as "Severe" will cause the application to crash at runtime when it attempts to load those groups. Benign errors here are violations of the convention for group IDs that 
             * have no material effect on loading and administering assessments.
             **/

            foreach (var itemGroup in itemGroups)
            {
                var match = Regex.Match(itemGroup.ChildNodeWithName("identifier")
                    .ValueForAttribute("uniqueid"), groupIdPattern);
                // These identifiers are parsed when retrieving items to present in Student. If they do not follow this pattern, they may cause (hard to debug) failures at runtime.
                if (!match.Success)
                {
                    result.Add(new ValidationError
                    {
                        ErrorSeverity = ErrorSeverity.Severe,
                        GeneratedMessage =
                            $"[Itemgroup identifier {itemGroup.ChildNodeWithName("identifier").ValueForAttribute("uniqueid")} does not follow expected pattern {groupIdPattern}]",
                        Key = "uniqueid",
                        ItemId = itemGroup.ChildNodeWithName("identifier").ValueForAttribute("uniqueid"),
                        PackageType = PackageType,
                        Location = "itemgroup/identifier"
                    });
                }
                else
                {
                    if (!match.Groups[1].Value.Equals(partitionIdentifier))
                    {
                        result.Add(new ValidationError
                        {
                            ErrorSeverity = ErrorSeverity.Benign,
                            GeneratedMessage =
                                $"[Itemgroup identifier {itemGroup.ChildNodeWithName("identifier").ValueForAttribute("uniqueid")} first portion {match.Groups[1]} does not match partition ID and expected value {partitionIdentifier}]",
                            Key = "uniqueid",
                            ItemId = itemGroup.ChildNodeWithName("identifier").ValueForAttribute("uniqueid"),
                            PackageType = PackageType,
                            Location = "itemgroup/identifier"
                        });
                    }
                    // "G-" items that do not contain a passageref that matches a valid stimuli fail at runtime.
                    if (match.Groups[2].Value.Equals("G"))
                    {
                        if (itemGroup.ChildNodesWithName("passageref").Count() != 1)
                        {
                            result.Add(new ValidationError
                            {
                                ErrorSeverity = ErrorSeverity.Severe,
                                GeneratedMessage =
                                    $"[Itemgroup identifier {itemGroup.ChildNodeWithName("identifier").ValueForAttribute("uniqueid")} is a multi-item specified \"G-\" item group, but does not have a reference to a single child passageref element. This assessment will fail in TDS at runtime]",
                                Key = "uniqueid",
                                ItemId = itemGroup.ChildNodeWithName("identifier").ValueForAttribute("uniqueid"),
                                PackageType = PackageType,
                                Location = "itemgroup/identifier"
                            });
                        }
                        // "G-" item group identifiers second portion should match their associated passage ID by convention.
                        else if (!match.Groups[3].Value.Equals(
                            itemGroup.ChildNodeWithName("passageref").ValidatedAttributes["passageref"].Value))
                        {
                            result.Add(new ValidationError
                            {
                                ErrorSeverity = ErrorSeverity.Benign,
                                GeneratedMessage =
                                    $"[Itemgroup identifier {itemGroup.ChildNodeWithName("identifier").ValueForAttribute("uniqueid")} is a multi-item specified \"G-\" item group, and conventionally includes the ID of its associated passage ({itemGroup.ChildNodeWithName("passageref").ValidatedAttributes["passageref"].Value}) as the second part of its identifier]",
                                Key = "uniqueid",
                                ItemId = itemGroup.ChildNodeWithName("identifier").ValueForAttribute("uniqueid"),
                                PackageType = PackageType,
                                Location = "itemgroup/identifier"
                            });
                        }
                    }
                    // "I-" item group identifiers indicate that the group in question should only contain a single item
                    else if (match.Groups[2].Value.Equals("I"))
                    {
                        if (itemGroup.ChildNodesWithName("groupitem").Count() != 1)
                        {
                            result.Add(new ValidationError
                            {
                                ErrorSeverity = ErrorSeverity.Benign,
                                GeneratedMessage =
                                    $"[Itemgroup identifier {itemGroup.ChildNodeWithName("identifier").ValueForAttribute("uniqueid")} is a single-item specified \"I-\" item group, but does not have a reference to a single child groupitem element]",
                                Key = "uniqueid",
                                ItemId = itemGroup.ChildNodeWithName("identifier").ValueForAttribute("uniqueid"),
                                PackageType = PackageType,
                                Location = "itemgroup/identifier"
                            });
                        }
                        // "I-" item group identifiers second portion should match their group item by convention.
                        else if (!match.Groups[3].Value.Equals(
                            itemGroup.ChildNodeWithName("groupitem").ValueForAttribute("itemid")))
                        {
                            result.Add(new ValidationError
                            {
                                ErrorSeverity = ErrorSeverity.Benign,
                                GeneratedMessage =
                                    $"[Itemgroup identifier {itemGroup.ChildNodeWithName("identifier").ValueForAttribute("uniqueid")} is a single-item specified \"I-\" item group, and conventionally includes the ID of its associated groupitem ({itemGroup.ChildNodeWithName("groupitem").ValueForAttribute("itemid")}) as the second part of its identifier]",
                                Key = "uniqueid",
                                ItemId = itemGroup.ChildNodeWithName("identifier").ValueForAttribute("uniqueid"),
                                PackageType = PackageType,
                                Location = "itemgroup/identifier"
                            });
                        }
                    }
                }
            }

            int parsedInt;
            for (var i = 1; i < itemGroups.Count(); i++)
            {
                if (
                    !itemGroups.Any(
                        x => int.TryParse(x.ValueForAttribute("formposition"), out parsedInt) && parsedInt == i))
                {
                    result.Add(new ValidationError
                    {
                        ErrorSeverity = ErrorSeverity.Degraded,
                        GeneratedMessage =
                            $"[formpartition is missing itemgroup with formposition {i}]",
                        Key = "formposition",
                        ItemId = Processor.ChildNodeWithName("identifier").ValueForAttribute("uniqueid"),
                        PackageType = PackageType,
                        Location = "itemgroup"
                    });
                }
            }

            itemGroups.Where(
                    x =>
                        int.TryParse(x.ValueForAttribute("formposition"), out parsedInt) &&
                        parsedInt > itemGroups.Count())
                .ToList()
                .ForEach(x => result.Add(new ValidationError
                {
                    ErrorSeverity = ErrorSeverity.Degraded,
                    GeneratedMessage =
                        $"[formposition {x.ValueForAttribute("formposition")} > itemgroup count {itemGroups.Count}]",
                    Key = "formposition",
                    ItemId = x.ChildNodeWithName("identifier").ValueForAttribute("uniqueid"),
                    PackageType = PackageType,
                    Location = "itemgroup"
                }));

            return result;
        }
    }
}