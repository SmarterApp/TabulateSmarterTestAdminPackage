using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.RestrictedValues.Enums;
using TabulateSmarterTestAdminPackage.Common.Utilities;
using TabulateSmarterTestPackage.Processors.TestSpecification.Common;

namespace TabulateSmarterTestPackage.Processors.TestSpecification.Administration.AdminSegment
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