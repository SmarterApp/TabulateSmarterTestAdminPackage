using System;
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
                    Location = $"testspecification/{PackageType.ToString().ToLower()}/itempool/testitem/bpref",
                    GeneratedMessage = $"[bpref node count ({Processor.ChildNodesWithName("bpref").Count()}) > max (7)]",
                    ItemId = Processor.ChildNodeWithName("identifier").ValueForAttribute("uniqueid").Split('-').Last(),
                    Key = "bpref",
                    PackageType = PackageType,
                    Value = $"testspecification/{PackageType.ToString().ToLower()}/itempool/testitem/bpref"
                });
            }

            return validationErrors;
        }
    }
}
