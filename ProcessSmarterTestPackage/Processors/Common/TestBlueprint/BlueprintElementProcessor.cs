using System.Collections.Generic;
using System.Linq;
using System.Xml.XPath;
using ProcessSmarterTestPackage.PostProcessors;
using SmarterTestPackage.Common.Data;
using SmarterTestPackage.Common.Extensions;
using ValidateSmarterTestPackage;
using ValidateSmarterTestPackage.RestrictedValues.Enums;
using ValidateSmarterTestPackage.Validators;
using ValidateSmarterTestPackage.Validators.Convenience;

namespace ProcessSmarterTestPackage.Processors.Common.TestBlueprint
{
    public class BlueprintElementProcessor : Processor
    {
        public BlueprintElementProcessor(XPathNavigator navigator, PackageType packageType)
            : base(navigator, packageType)
        {
            Attributes = new AttributeValidationDictionary
            {
                {
                    "elementtype", StringValidator.IsValidNonEmptyWithLength(100)
                        .AddAndReturn(new RequiredEnumValidator(ErrorSeverity.Degraded,
                            RestrictedListItems.BlueprintElementType))
                },
                {
                    "minopitems", IntValidator.IsValidPositiveNonEmptyWithLength(4)
                },
                {
                    "maxopitems", IntValidator.IsValidPositiveNonEmptyWithLength(4)
                },
                {
                    "minftitems", new NoValidator(ErrorSeverity.Benign)
                },
                {
                    "maxftitems", new NoValidator(ErrorSeverity.Benign)
                },
                {
                    "opitemcount", IntValidator.IsValidNonEmptyWithLengthAndMinValue(4, 1)
                },
                {
                    "ftitemcount", IntValidator.IsValidPositiveNonEmptyWithLength(4)
                },
                {
                    "parentid", new NoValidator(ErrorSeverity.Benign)
                }
            };

            ApplySecondaryValidation("elementtype", "contentlevel", "parentid",
                StringValidator.IsValidNonEmptyWithLength(150));
            ApplySecondaryValidation("elementtype", "test", "minftitems",
                IntValidator.IsValidPositiveNonEmptyWithLength(4));
            ApplySecondaryValidation("elementtype", "test", "maxftitems",
                IntValidator.IsValidPositiveNonEmptyWithLength(4));
            ApplySecondaryValidation("elementtype", "segment", "minftitems",
                IntValidator.IsValidPositiveNonEmptyWithLength(4));
            ApplySecondaryValidation("elementtype", "segment", "maxftitems",
                IntValidator.IsValidPositiveNonEmptyWithLength(4));


            Navigator.GenerateList("identifier").ForEach(x => Processors.Add(new IdentifierProcessor(x, packageType)));
            ReplaceAttributeValidation("identifier", new AttributeValidationDictionary
            {
                {
                    "uniqueid", StringValidator.IsValidNonEmptyWithLength(150)
                },
                {
                    "name", StringValidator.IsValidNonEmptyWithLength(255)
                }
            });
            RemoveAttributeValidation("identifier", "label");
        }

        public override List<ValidationError> AdditionalValidations()
        {
            return new BlueprintElementPostProcessor(PackageType, this).GenerateErrors().ToList();
        }
    }
}