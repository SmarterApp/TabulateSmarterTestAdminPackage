using System.Collections.Generic;
using System.Linq;
using ProcessSmarterTestPackage.Processors.Common;
using SmarterTestPackage.Common.Data;

namespace ProcessSmarterTestPackage.PostProcessors
{
    public class SegmentBlueprintPostProcessor : PostProcessor
    {
        public SegmentBlueprintPostProcessor(PackageType packageType, Processor processor)
            : base(packageType, processor) {}

        public override IList<ValidationError> GenerateErrors()
        {
            var result = new List<ValidationError>();

            int min;
            int max;

            Processor.ChildNodesWithName("segmentbpelement")
                .Where(
                    x =>
                        int.TryParse(x.ValueForAttribute("minopitems"), out min) &&
                        int.TryParse(x.ValueForAttribute("maxopitems"), out max) && min > max)
                .ToList()
                .ForEach(x => result.Add(new ValidationError
                {
                    ErrorSeverity = ErrorSeverity.Benign,
                    Location = x.Navigator.Name,
                    GeneratedMessage =
                        $"[segmentbpelement minopitems {x.ValueForAttribute("minopitems")} > maxopitems {x.ValueForAttribute("maxopitems")}]",
                    ItemId = x.ValueForAttribute("bpelementid"),
                    Key = "minopitems",
                    PackageType = PackageType,
                    Value = x.Navigator.OuterXml
                }));

            return result;
        }
    }
}