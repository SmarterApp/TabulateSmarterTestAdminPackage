using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.RestrictedValues.Enums;
using TabulateSmarterTestAdminPackage.Common.Utilities;
using TabulateSmarterTestPackage.Processors.TestSpecification.Common;

namespace TabulateSmarterTestPackage.Processors.TestSpecification.Administration.AdminSegment
{
    public class SegmentPoolProcessor : Processor
    {
        public SegmentPoolProcessor(XPathNavigator navigator, PackageType packageType) : base(navigator, packageType)
        {
            Navigator.GenerateList("itemgroup")
                .ForEach(x => Processors.Add(new ItemGroupProcessor(x, packageType)));
        }
    }
}