using System.Collections.Generic;
using System.Linq;
using ProcessSmarterTestPackage.Processors.Common;
using SmarterTestPackage.Common.Data;

namespace ProcessSmarterTestPackage.PostProcessors
{
    public class TestItemPostProcessor : PostProcessor
    {
        public TestItemPostProcessor(PackageType packageType, Processor processor) : base(packageType, processor) {}

        public override IList<ValidationError> GenerateErrors()
        {
            var validationErrors = new List<ValidationError>();

            if (Processor.ChildNodesWithName("bpref").Count() > 7)
            {
                validationErrors.Add(new ValidationError
                {
                    ErrorSeverity = ErrorSeverity.Benign,
                    Location = "testitem/bpref",
                    GeneratedMessage = $"[bpref node count ({Processor.ChildNodesWithName("bpref").Count()}) > max (7)]",
                    ItemId = Processor.ChildNodeWithName("identifier").ValueForAttribute("uniqueid").Split('-').Last(),
                    Key = "bpref",
                    PackageType = PackageType,
                    Value = Processor.Navigator.OuterXml
                });
            }

            // If there is a duplicate pool property, where pool property uniqueness is defined as unique property and value, record an error
            Processor.ChildNodesWithName("poolproperty")
                .GroupBy(x => $"{x.ValueForAttribute("property")}|{x.ValueForAttribute("value")}")
                .Where(x => x.Count() > 1)
                .ToList()
                .ForEach(x => validationErrors.Add(new ValidationError
                {
                    ErrorSeverity = ErrorSeverity.Severe,
                    Location = "testitem/poolproperty",
                    GeneratedMessage = "[Duplicate poolproperty values exist for property: " +
                                       $"{x.Key.Split('|').FirstOrDefault() ?? string.Empty} " +
                                       $"with value: {x.Key.Split('|').LastOrDefault() ?? string.Empty}]",
                    ItemId = Processor.ChildNodeWithName("identifier").ValueForAttribute("uniqueid").Split('-').Last(),
                    Key = "poolproperty",
                    PackageType = PackageType,
                    Value = Processor.Navigator.OuterXml
                }));

            return validationErrors;
        }
    }
}