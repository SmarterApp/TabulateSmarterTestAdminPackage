using System.Xml.XPath;
using ProcessSmarterTestPackage.Processors.Common;
using SmarterTestPackage.Common.Data;
using ValidateSmarterTestPackage;
using ValidateSmarterTestPackage.Validators.Convenience;

namespace ProcessSmarterTestPackage.Processors.Administration.AdminSegment
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