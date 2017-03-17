using System;
using System.Collections.Generic;
using System.Linq;
using ProcessSmarterTestPackage.Processors.Common;
using SmarterTestPackage.Common.Data;

namespace ProcessSmarterTestPackage.PostProcessors
{
    public class PerformanceLevelsPostProcessor : PostProcessor
    {
        public PerformanceLevelsPostProcessor(PackageType packageType, Processor processor)
            : base(packageType, processor) {}

        public override IList<ValidationError> GenerateErrors()
        {
            var result = new List<ValidationError>();

            var performanceLevels = Processor.ChildNodesWithName("performancelevel").ToList();
            for (var i = 1; i <= performanceLevels.Count(); i++)
            {
                var performanceLevel = performanceLevels
                    .Where(x => x.ValueForAttribute("plevel").Equals(i.ToString(), StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (!performanceLevel.Any())
                {
                    result.Add(new ValidationError
                    {
                        ErrorSeverity = ErrorSeverity.Degraded,
                        GeneratedMessage =
                            $"[Property plevel {i} is not present in performancelevels]",
                        Key = "plevel",
                        PackageType = PackageType,
                        Location =
                            "performancelevels/performancelevel"
                    });
                }
                else if (performanceLevel.Count() > 1)
                {
                    result.Add(new ValidationError
                    {
                        ErrorSeverity = ErrorSeverity.Degraded,
                        GeneratedMessage =
                            $"[Property plevel {i} is duplicated by <{performanceLevel.Select(x => x.ValueForAttribute("bpelementid")).Aggregate((x, y) => $"{x},{y}")}> for performancelevels]",
                        Key = "position",
                        PackageType = PackageType,
                        Location =
                            "scoringrules/computationrule/computationruleparameter"
                    });
                }
            }

            foreach (var performanceLevel in performanceLevels)
            {
                double lo;
                double hi;

                if (double.TryParse(performanceLevel.ValueForAttribute("scaledlo"), out lo) &&
                    double.TryParse(performanceLevel.ValueForAttribute("scaledhi"), out hi) && lo > hi)
                {
                    result.Add(new ValidationError
                    {
                        ErrorSeverity = ErrorSeverity.Degraded,
                        GeneratedMessage =
                            $"[scaledlo{performanceLevel.ValueForAttribute("scaledlo")}>{performanceLevel.ValueForAttribute("scaledhi")}]",
                        Key = "scaledlo",
                        ItemId = Processor.ValueForAttribute("bpelementid"),
                        PackageType = PackageType,
                        Location =
                            $"testspecification/{PackageType.ToString().ToLower()}/scoringrules/computationrule/computationruleparameter",
                        Value = performanceLevel.Navigator.OuterXml
                    });
                }
            }

            return result;
        }
    }
}