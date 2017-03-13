using System.Collections.Generic;
using System.Linq;
using ProcessSmarterTestPackage.Processors.Common;
using SmarterTestPackage.Common.Data;

namespace ProcessSmarterTestPackage.PostProcessors
{
    public class ItemPoolPostProcessor : PostProcessor
    {
        public ItemPoolPostProcessor(PackageType packageType, Processor processor) : base(packageType, processor) {}

        public override IList<ValidationError> GenerateErrors()
        {
            var validationErrors = new List<ValidationError>();

            var passageIdList = Processor.ChildNodesWithName("passage")
                .Select(x => x.ChildNodeWithName("identifier").ValueForAttribute("uniqueid")).ToList();

            var itemsWithPassageRefList =
                Processor.ChildNodesWithName("testitem").Where(x => x.ChildNodesWithName("passageref").Any()).ToList();

            foreach (var item in itemsWithPassageRefList)
            {
                if (!passageIdList.Contains(item.ChildNodeWithName("passageref").ValueForAttribute("passageref")))
                {
                    validationErrors.Add(new ValidationError
                    {
                        ErrorSeverity = ErrorSeverity.Degraded,
                        Location = "testitem/passageref",
                        GeneratedMessage =
                            $"[No passage ID matches item {item.ChildNodeWithName("identifier").ValueForAttribute("uniqueid")} passageref {item.ChildNodeWithName("passageref").ValueForAttribute("passageref")}]",
                        ItemId =
                            item.ChildNodeWithName("identifier")
                                .ValueForAttribute("uniqueid")
                                .Split('-')
                                .LastOrDefault(),
                        Key = "passageref",
                        PackageType = PackageType,
                        Value = item.ChildNodeWithName("passageref").Navigator.OuterXml
                    });
                }
            }

            return validationErrors;
        }
    }
}