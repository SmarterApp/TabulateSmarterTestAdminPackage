using System.Xml.XPath;
using ProcessSmarterTestPackage.Processors.Common;
using TabulateSmarterTestPackage.Common.RestrictedValues.Enums;
using ValidateSmarterTestPackage;
using ValidateSmarterTestPackage.Validators;
using ValidateSmarterTestPackage.Validators.Convenience;

namespace ProcessSmarterTestPackage.Processors.Administration.AdminSegment
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
                        .AddAndReturn(new RequiredRegularExpressionValidator(ErrorSeverity.Degraded, "^[A-Z]$"))
                },
                {
                    "formposition", IntValidator.IsValidOptionalNonEmptyWithLengthAndMinValue(10, 1)
                }
            };
        }
    }
}