using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.RestrictedValues.Enums;
using TabulateSmarterTestAdminPackage.Common.Utilities;
using TabulateSmarterTestAdminPackage.Common.Validators.Convenience;
using TabulateSmarterTestPackage.Processors.Common;

namespace TabulateSmarterTestPackage.Processors.Administration.AdminSegment
{
    public class SegmentBlueprintElementProcessor : Processor
    {
        public SegmentBlueprintElementProcessor(XPathNavigator navigator, PackageType packageType)
            : base(navigator, packageType)
        {
            Attributes = new AttributeValidationDictionary
            {
                {
                    "bpelementid", StringValidator.IsValidNonEmptyWithLength(150)
                },
                {
                    "minopitems", IntValidator.IsValidPositiveNonEmptyWithLength(10)
                },
                {
                    "maxopitems", IntValidator.IsValidNonEmptyWithLengthAndMinValue(10, 1)
                }
            };
        }
    }
}