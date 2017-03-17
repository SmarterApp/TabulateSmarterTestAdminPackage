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
            foreach (var itemGroup in Processor.ChildNodesWithName("itemgroup"))
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

            return result;
        }
    }
}