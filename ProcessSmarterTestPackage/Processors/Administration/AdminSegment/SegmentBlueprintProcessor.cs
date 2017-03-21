using System.Collections.Generic;
using System.Linq;
using System.Xml.XPath;
using ProcessSmarterTestPackage.PostProcessors;
using ProcessSmarterTestPackage.Processors.Common;
using SmarterTestPackage.Common.Data;
using SmarterTestPackage.Common.Extensions;

namespace ProcessSmarterTestPackage.Processors.Administration.AdminSegment
{
    public class SegmentBlueprintProcessor : Processor
    {
        public SegmentBlueprintProcessor(XPathNavigator navigator, PackageType packageType)
            : base(navigator, packageType)
        {
            Navigator.GenerateList("segmentbpelement")
                .ForEach(x => Processors.Add(new SegmentBlueprintElementProcessor(x, packageType)));
        }

        public override List<ValidationError> AdditionalValidations()
        {
            return new SegmentBlueprintPostProcessor(PackageType, this).GenerateErrors().ToList();
        }
    }
}