using System.Xml.XPath;
using ProcessSmarterTestPackage.Processors.Common;
using TabulateSmarterTestPackage.Common.RestrictedValues.Enums;
using TabulateSmarterTestPackage.Common.Utilities;

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
    }
}