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
            var identifier = Processor.ChildNodeWithName("identifier").ValueForAttribute("uniqueid");
            int position;

            for (var i = 1; i <= groupItems.Count(); i++)
            {
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
                        ItemId = identifier
                    });
                }
            }

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
                    ItemId = identifier
                }));

            return result;
        }
    }
}