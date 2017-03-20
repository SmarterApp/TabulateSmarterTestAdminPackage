using System;
using System.Collections.Generic;
using System.Linq;
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
            foreach (var itemGroup in itemGroups)
            {
                if (!itemGroup.ChildNodeWithName("identifier")
                    .ValueForAttribute("uniqueid").Split(':').First()
                    .Equals(partitionIdentifier, StringComparison.OrdinalIgnoreCase))
                {
                    result.Add(new ValidationError
                    {
                        ErrorSeverity = ErrorSeverity.Benign,
                        GeneratedMessage =
                            $"[Itemgroup identifier {itemGroup.ChildNodeWithName("identifier").ValueForAttribute("uniqueid")} does not follow expected pattern {partitionIdentifier}:G-<ID>]",
                        Key = "uniqueid",
                        ItemId = itemGroup.ChildNodeWithName("identifier").ValueForAttribute("uniqueid"),
                        PackageType = PackageType,
                        Location = "itemgroup/identifier"
                    });
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