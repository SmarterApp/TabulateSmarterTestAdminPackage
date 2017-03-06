using System.Xml.XPath;
using ProcessSmarterTestPackage.Processors.Common;
using SmarterTestPackage.Common.Data;
using SmarterTestPackage.Common.Extensions;

namespace ProcessSmarterTestPackage.Processors.Administration.AdminSegment
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