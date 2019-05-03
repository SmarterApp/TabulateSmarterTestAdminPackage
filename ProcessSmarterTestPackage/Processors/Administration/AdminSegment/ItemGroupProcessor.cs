using System.Collections.Generic;
using System.Linq;
using System.Xml.XPath;
using ProcessSmarterTestPackage.PostProcessors;
using ProcessSmarterTestPackage.Processors.Common;
using SmarterTestPackage.Common.Data;
using SmarterTestPackage.Common.Extensions;
using ValidateSmarterTestPackage;
using ValidateSmarterTestPackage.Validators;
using ValidateSmarterTestPackage.Validators.Convenience;

namespace ProcessSmarterTestPackage.Processors.Administration.AdminSegment
{
    public class ItemGroupProcessor : Processor
    {
        public ItemGroupProcessor(XPathNavigator navigator, PackageType packageType)
            : base(navigator, packageType)
        {
            Attributes = new AttributeValidationDictionary
            {
                {
                    "maxitems", new ValidatorCollection
                    {
                        new RequiredStringValidator(ErrorSeverity.Degraded),
                        new MaxLengthValidator(ErrorSeverity.Degraded, 10),
                        new RequiredRegularExpressionValidator(ErrorSeverity.Degraded, @"^(\d+|ALL)$")
                    }
                },
                {
                    "maxresponses", new ValidatorCollection
                    {
                        new RequiredStringValidator(ErrorSeverity.Degraded),
                        new MaxLengthValidator(ErrorSeverity.Degraded, 10),
                        new RequiredRegularExpressionValidator(ErrorSeverity.Degraded, @"^(\d+|ALL)$")
                    }
                },
                {
                    "formposition", IntValidator.IsValidNonEmptyWithLengthAndMinValue(10, 1)
                }
            };

            Navigator.GenerateList("groupitem")
                .ForEach(x => Processors.Add(new GroupItemProcessor(x, packageType)));

            Navigator.GenerateList("identifier")
                .ForEach(x => Processors.Add(new IdentifierProcessor(x, packageType)));
            ReplaceAttributeValidation("identifier", new AttributeValidationDictionary
            {
                {
                    "uniqueid", StringValidator.IsValidNonEmptyWithLength(200)
                }
            });
            RemoveAttributeValidation("identifier", "label");
        }

        protected override List<ValidationError> AdditionalValidations()
        {
            return new ItemGroupPostProcessor(PackageType, this).GenerateErrors().ToList();
        }
    }
}