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

            if (identifier.Contains("G-") && !Processor.ChildNodesWithName("passageref").Any())
            {
                result.Add(new ValidationError
                {
                    Value = Processor.Navigator.OuterXml,
                    GeneratedMessage =
                        $"[itemgroup with group identifier {identifier} is a 'G-' group, but does not have an associated passageref child element. Test will fail at runtime in TDS]",
                    Key = "uniqueid",
                    ErrorSeverity = ErrorSeverity.Severe,
                    PackageType = Processor.PackageType,
                    Location = "itemgroup/identifier",
                    ItemId = identifier
                });
            }

            return result;
        }
    }
}