using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.Processors;
using TabulateSmarterTestAdminPackage.Common.RestrictedValues.Enums;
using TabulateSmarterTestAdminPackage.Common.Utilities;
using TabulateSmarterTestAdminPackage.Common.Validators.Convenience;

namespace TabulateSmarterTestPackage.Processors.TestSpecification.Administration.AdminSegment
{
    public class GroupItemProcessor : Processor
    {
        public GroupItemProcessor(XPathNavigator navigator, PackageType packageType)
            : base(navigator, packageType)
        {
            Attributes = new AttributeValidationDictionary
            {
                {
                    "itemid", StringValidator.IsValidNonEmptyWithLength(150)
                },
                {
                    "groupposition", IntValidator.IsValidNonEmptyWithLengthAndMinValue(10, 1)
                },
                {
                    "adminrequired", BooleanValidator.IsValidNonEmptyWithLength(5)
                },
                {
                    "responserequired", BooleanValidator.IsValidNonEmptyWithLength(5)
                },
                {
                    "isactive", BooleanValidator.IsValidNonEmptyWithLength(5)
                },
                {
                    "isfieldtest", BooleanValidator.IsValidNonEmptyWithLength(5)
                },
                {
                    "blockid", StringValidator.IsValidNonEmptyWithLength(10)
                },
                {
                    "formposition", IntValidator.IsValidOptionalNonEmptyWithLengthAndMinValue(10, 1)
                }
            };
        }
    }
}