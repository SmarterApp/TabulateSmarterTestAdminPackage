using System;
using System.Collections.Generic;
using System.Linq;
using ProcessSmarterTestPackage.Processors.Common;
using SmarterTestPackage.Common.Data;

namespace ProcessSmarterTestPackage.PostProcessors
{
    public class ComputationRulePostProcessor : PostProcessor
    {
        public ComputationRulePostProcessor(PackageType packageType, Processor processor) : base(packageType, processor) {}

        public override IList<ValidationError> GenerateErrors()
        {
            var result = new List<ValidationError>();

            for (var i = 1; i <= Processor.ChildNodesWithName("computationruleparameter").Count(); i++)
            {
                var computationRuleParameters = Processor.ChildNodesWithName("computationruleparameter")
                    .Where(x => x.ValueForAttribute("position").Equals(i.ToString(), StringComparison.OrdinalIgnoreCase))
                    .ToList();
                if (!computationRuleParameters.Any())
                {
                    result.Add(new ValidationError
                    {
                        ErrorSeverity = ErrorSeverity.Degraded,
                        GeneratedMessage =
                            $"[Property position {i} is not present in computationruleparameters for computationrule {Processor.ChildNodeWithName("identifier").ValueForAttribute("uniqueid")}]",
                        Key = "position",
                        ItemId = Processor.ValueForAttribute("bpelementid"),
                        PackageType = PackageType,
                        Location =
                            $"testspecification/{PackageType.ToString().ToLower()}/scoringrules/computationrule/computationruleparameter"
                    });
                }
                else if (computationRuleParameters.Count() > 1)
                {
                    result.Add(new ValidationError
                    {
                        ErrorSeverity = ErrorSeverity.Degraded,
                        GeneratedMessage =
                            $"[Property position {i} is duplicated by <{computationRuleParameters.Select(x => x.ChildNodeWithName("identifier").ValueForAttribute("uniqueid")).Aggregate((x, y) => $"{x},{y}")}> for computationrule {Processor.ChildNodeWithName("identifier").ValueForAttribute("uniqueid")}]",
                        Key = "position",
                        ItemId = Processor.ValueForAttribute("position"),
                        PackageType = PackageType,
                        Location =
                            $"testspecification/{PackageType.ToString().ToLower()}/scoringrules/computationrule/computationruleparameter"
                    });
                }
            }

            return result;
        }
    }
}