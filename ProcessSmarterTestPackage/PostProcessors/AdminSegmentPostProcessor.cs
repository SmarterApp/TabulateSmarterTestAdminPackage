using System;
using System.Collections.Generic;
using System.Linq;
using ProcessSmarterTestPackage.Processors.Common;
using SmarterTestPackage.Common.Data;

namespace ProcessSmarterTestPackage.PostProcessors
{
    public class AdminSegmentPostProcessor : PostProcessor
    {
        public AdminSegmentPostProcessor(PackageType packageType, Processor processor) : base(packageType, processor) {}

        public override IList<ValidationError> GenerateErrors()
        {
            var result = new List<ValidationError>();

            if (Processor.ValueForAttribute("itemselection").Equals("fixedform", StringComparison.OrdinalIgnoreCase) &&
                !Processor.ChildNodesWithName("segmentform").Any())
            {
                result.Add(new ValidationError
                {
                    Value = Processor.Navigator.OuterXml,
                    GeneratedMessage =
                        $"[AdminSegment {Processor.ValueForAttribute("segmentid")} has a fixedform selector type, but no valid segmentforms]",
                    Key = "segmentform",
                    ErrorSeverity = ErrorSeverity.Severe,
                    PackageType = Processor.PackageType,
                    Location = Processor.Navigator.Name,
                    ItemId = Processor.ValueForAttribute("segmentid")
                });
            }

            return result;
        }
    }
}