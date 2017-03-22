using System.Collections.Generic;
using ProcessSmarterTestPackage.Processors.Common;
using SmarterTestPackage.Common.Data;

namespace ProcessSmarterTestPackage.PostProcessors
{
    public class ScoringRulesPostProcessor : PostProcessor
    {
        public ScoringRulesPostProcessor(PackageType packageType, Processor processor) : base(packageType, processor) {}

        public override IList<ValidationError> GenerateErrors()
        {
            var result = new List<ValidationError>();

            var computationOrderList = new List<string>();

            foreach (var computationRule in Processor.ChildNodesWithName("computationrule"))
            {
                if (computationOrderList.Contains(computationRule.ValueForAttribute("computationorder")))
                {
                    result.Add(new ValidationError
                    {
                        ErrorSeverity = ErrorSeverity.Degraded,
                        GeneratedMessage =
                            $"[Property computationorder {computationRule.ValueForAttribute("computationOrder")} of computationrule {computationRule.ChildNodeWithName("identifier").ValueForAttribute("uniqueid")} matches another rule's computationorder property]",
                        Key = "computationorder",
                        ItemId = computationRule.ValueForAttribute("bpelementid"),
                        PackageType = PackageType,
                        Location = $"testspecification/{PackageType.ToString().ToLower()}/scoringrules/computationrule"
                    });
                }
                else
                {
                    computationOrderList.Add(computationRule.ValueForAttribute("computationOrder"));
                }
            }

            return result;
        }
    }
}