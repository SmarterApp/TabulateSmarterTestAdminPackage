using System;
using System.Collections.Generic;
using System.Linq;
using ProcessSmarterTestPackage.Processors.Common;
using SmarterTestPackage.Common.Data;
using ValidateSmarterTestPackage.RestrictedValues.Enums;

namespace ProcessSmarterTestPackage.PostProcessors
{
    public class TestSpecificationPostProcessor : PostProcessor
    {
        public TestSpecificationPostProcessor(PackageType packageType, Processor processor)
            : base(packageType, processor) {}

        public override IList<ValidationError> GenerateErrors()
        {
            var result = new List<ValidationError>();
            var id = ((TestSpecificationProcessor) Processor).GetUniqueId();
            var blueprintElement =
                Processor.ChildNodeWithName(PackageType.ToString().ToLower())
                    .ChildNodeWithName("testblueprint")
                    .ChildNodesWithName("bpelement")
                    .First(x => x.ValueForAttribute("elementtype")
                        .Equals("test", StringComparison.OrdinalIgnoreCase));
            var blueprintId = blueprintElement.ChildNodeWithName("identifier").ValueForAttribute("uniqueid");
            if (!id.Equals(blueprintId, StringComparison.OrdinalIgnoreCase))
            {
                result.Add(new ValidationError
                {
                    ErrorSeverity = ErrorSeverity.Severe,
                    GeneratedMessage = $"[TestBlueprint test uniqueid {blueprintId} != TestSpecification uniqueid {id}]",
                    Key = "identifier",
                    Location =
                        $"testspecification/{PackageType.ToString().ToLower()}/testblueprint/bpelement/identifier"
                });
            }
            // Getting the identifiers of all items in the testblueprint, then all testitems
            result.AddRange(CheckBpRef(Processor
                    .ChildNodeWithName(PackageType.ToString().ToLower())
                    .ChildNodeWithName("testblueprint")
                    .ChildNodesWithName("bpelement")
                    .SelectMany(x => x.ChildNodesWithName("identifier"))
                    .Select(x => x.ValueForAttribute("uniqueid")),
                Processor.ChildNodeWithName(PackageType.ToString().ToLower())
                    .ChildNodeWithName("itempool")
                    .ChildNodesWithName("testitem")));
            return result;
        }

        private IEnumerable<ValidationError> CheckBpRef(IEnumerable<string> elementIds,
            IEnumerable<Processor> testItems)
        {
            var result = new List<ValidationError>();
            foreach (var testItem in testItems)
            {
                foreach (var bpRef in testItem.ChildNodesWithName("bpref"))
                {
                    var found =
                        elementIds.Any(
                            element =>
                                element.Equals(bpRef.ValueForAttribute("bpref"), StringComparison.OrdinalIgnoreCase));
                    if (!found)
                    {
                        result.Add(new ValidationError
                        {
                            ErrorSeverity = ErrorSeverity.Degraded,
                            GeneratedMessage =
                                $"[BpRef {bpRef.ValueForAttribute("bpref")} is not present in TestBlueprint]",
                            Key = "bpref",
                            Location = $"testspecification/{PackageType.ToString().ToLower()}/itempool/testitem/bpref",
                            ItemId = testItem.ChildNodeWithName("identifier").ValueForAttribute("uniqueid")
                        });
                    }
                }
            }
            return result;
        }
    }
}