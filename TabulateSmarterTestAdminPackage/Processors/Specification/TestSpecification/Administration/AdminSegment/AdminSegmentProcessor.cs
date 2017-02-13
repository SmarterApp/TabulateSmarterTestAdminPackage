using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.Enums;
using TabulateSmarterTestAdminPackage.Common.Processors;
using TabulateSmarterTestAdminPackage.Common.Utilities;
using TabulateSmarterTestAdminPackage.Common.Validators.Convenience;

namespace TabulateSmarterTestAdminPackage.Processors.Specification.TestSpecification.Administration.AdminSegment
{
    public class AdminSegmentProcessor : Processor
    {
        public AdminSegmentProcessor(XPathNavigator navigator, PackageType packageType) : base(navigator, packageType)
        {
            Attributes = new AttributeValidationDictionary
            {
                {
                    "segmentid", StringValidator.IsValidNonEmptyWithLength(250)
                },
                {
                    "position", IntValidator.IsValidNonEmptyWithLengthAndMinValue(10, 1)
                },
                {
                    "itemselection", StringValidator.IsValidNonEmptyWithLength(100)
                }
            };

            Navigator.GenerateList("segmentblueprint")
                .ForEach(x => Processors.Add(new SegmentBlueprintProcessor(x, packageType)));
            Navigator.GenerateList("itemselector")
                .ForEach(x => Processors.Add(new ItemSelectorProcessor(x, packageType)));
            Navigator.GenerateList("segmentpool").ForEach(x => Processors.Add(new SegmentPoolProcessor(x, packageType)));
            Navigator.GenerateList("segmentform").ForEach(x => Processors.Add(new SegmentFormProcessor(x, packageType)));
        }
    }
}