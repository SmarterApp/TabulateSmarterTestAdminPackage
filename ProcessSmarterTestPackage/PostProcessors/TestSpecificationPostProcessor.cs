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
            return result;
        }
    }
}