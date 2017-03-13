using System.Collections.Generic;
using System.Linq;
using ProcessSmarterTestPackage.Processors.Common;
using SmarterTestPackage.Common.Data;

namespace ProcessSmarterTestPackage.PostProcessors
{
    public class ItemGroupPostProcessor : PostProcessor
    {
        public ItemGroupPostProcessor(PackageType packageType, Processor processor) : base(packageType, processor) {}

        public override IList<ValidationError> GenerateErrors()
        {
            var result = new List<ValidationError>();

            var groupItems = Processor.ChildNodesWithName("groupitem").ToList();
            int position;

            for (var i = 1; i <= groupItems.Count(); i++)
            {
                if (
                    !groupItems.Any(
                        x => int.TryParse(x.ValueForAttribute("formposition"), out position) && position == i))
                {
                    result.Add(new ValidationError
                    {
                        Value = Processor.Navigator.OuterXml,
                        GeneratedMessage = $"[Could not find groupitem within itemgroup with formposition {i}]",
                        Key = "formposition",
                        ErrorSeverity = ErrorSeverity.Degraded,
                        PackageType = Processor.PackageType,
                        Location = $"itemgroup/{Processor.Navigator.Name}",
                        ItemId = Processor.ChildNodeWithName("identifier").ValueForAttribute("uniqueid")
                    });
                }
                if (
                    !groupItems.Any(
                        x => int.TryParse(x.ValueForAttribute("groupposition"), out position) && position == i))
                {
                    result.Add(new ValidationError
                    {
                        Value = Processor.Navigator.OuterXml,
                        GeneratedMessage = $"[Could not find groupitem within itemgroup with groupposition {i}]",
                        Key = "groupposition",
                        ErrorSeverity = ErrorSeverity.Degraded,
                        PackageType = Processor.PackageType,
                        Location = $"itemgroup/{Processor.Navigator.Name}",
                        ItemId = Processor.ChildNodeWithName("identifier").ValueForAttribute("uniqueid")
                    });
                }
            }

            groupItems.Where(
                    x => int.TryParse(x.ValueForAttribute("formposition"), out position) && position > groupItems.Count)
                .ToList()
                .ForEach(x => result.Add(new ValidationError
                {
                    Value = Processor.Navigator.OuterXml,
                    GeneratedMessage =
                        $"[formposition of groupitem {x.ValueForAttribute("formposition")} > groupitem count]",
                    Key = "formposition",
                    ErrorSeverity = ErrorSeverity.Degraded,
                    PackageType = Processor.PackageType,
                    Location = $"itemgroup/{Processor.Navigator.Name}",
                    ItemId = Processor.ChildNodeWithName("identifier").ValueForAttribute("uniqueid")
                }));

            groupItems.Where(
                    x => int.TryParse(x.ValueForAttribute("groupposition"), out position) && position > groupItems.Count)
                .ToList()
                .ForEach(x => result.Add(new ValidationError
                {
                    Value = Processor.Navigator.OuterXml,
                    GeneratedMessage =
                        $"[groupposition of groupitem {x.ValueForAttribute("groupposition")} > groupitem count]",
                    Key = "groupposition",
                    ErrorSeverity = ErrorSeverity.Degraded,
                    PackageType = Processor.PackageType,
                    Location = $"itemgroup/{Processor.Navigator.Name}",
                    ItemId = Processor.ChildNodeWithName("identifier").ValueForAttribute("uniqueid")
                }));

            return result;
        }
    }
}