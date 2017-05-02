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

            foreach (var itemGroup in itemGroups)
            {
                var match = Regex.Match(itemGroup.ChildNodeWithName("identifier")
                    .ValueForAttribute("uniqueid"), groupIdPattern);
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
                    if (!match.Captures[0].Value.Equals(partitionIdentifier))
                    {
                        result.Add(new ValidationError
                        {
                            ErrorSeverity = ErrorSeverity.Benign,
                            GeneratedMessage =
                                $"[Itemgroup identifier {itemGroup.ChildNodeWithName("identifier").ValueForAttribute("uniqueid")} first portion {match.Captures[0]} does not match partition ID and expected value {partitionIdentifier}]",
                            Key = "uniqueid",
                            ItemId = itemGroup.ChildNodeWithName("identifier").ValueForAttribute("uniqueid"),
                            PackageType = PackageType,
                            Location = "itemgroup/identifier"
                        });
                    }
                    if (match.Captures[1].Value.Equals("G"))
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
                        else if (!match.Captures[2].Value.Equals(
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
                    else if (match.Captures[1].Value.Equals("I"))
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
                        else if (!match.Captures[2].Value.Equals(
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