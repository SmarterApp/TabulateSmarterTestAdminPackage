using System.Collections.Generic;
using System.Linq;
using ProcessSmarterTestPackage.Processors.Common;
using SmarterTestPackage.Common.Data;

namespace ProcessSmarterTestPackage.PostProcessors
{
    public class TestFormPostProcessor : PostProcessor
    {
        public TestFormPostProcessor(PackageType packageType, Processor processor) : base(packageType, processor) {}

        public override IList<ValidationError> GenerateErrors()
        {
            var result = new List<ValidationError>();

            int length;
            if (!int.TryParse(Processor.ValueForAttribute("length"), out length))
            {
                return result;
            }

            var itemCount = Processor.ChildNodesWithName("formpartition")
                .SelectMany(x => x.ChildNodesWithName("itemgroup")
                    .SelectMany(y => y.ChildNodesWithName("groupitem")))
                .Count();

            if (length != itemCount)
            {
                result.Add(new ValidationError
                {
                    ErrorSeverity = ErrorSeverity.Benign,
                    Location = string.Empty,
                    GeneratedMessage = $"[TestForm length {length} != GroupItem count {itemCount}]",
                    ItemId = Processor.ChildNodeWithName("identifier").ValueForAttribute("uniqueid"),
                    Key = "length",
                    PackageType = PackageType,
                    Value = Processor.Navigator.OuterXml
                });
            }

            return result;
        }
    }
}