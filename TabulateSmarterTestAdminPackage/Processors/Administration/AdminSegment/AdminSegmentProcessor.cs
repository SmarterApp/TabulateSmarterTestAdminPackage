using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.RestrictedValues.Enums;
using TabulateSmarterTestAdminPackage.Common.Utilities;
using TabulateSmarterTestAdminPackage.Common.Validators;
using TabulateSmarterTestAdminPackage.Common.Validators.Convenience;
using TabulateSmarterTestPackage.Processors.Common;

namespace TabulateSmarterTestPackage.Processors.Administration.AdminSegment
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
                        .AddAndReturn(new RequiredEnumValidator(ErrorSeverity.Degraded,
                            RestrictedListItems.ItemSelectionAlgorithm))
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