﻿using System.Xml.XPath;
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
                    "maxopitems", IntValidator.IsValidNonEmptyWithLengthAndMinValue(4, 1)
                },
                {
                    "minftitems", IntValidator.IsValidOptionalPositiveNonEmptyWithLength(4)
                },
                {
                    "maxftitems", IntValidator.IsValidOptionalNonEmptyWithLengthAndMinValue(4, 1)
                },
                {
                    "opitemcount", IntValidator.IsValidNonEmptyWithLengthAndMinValue(4, 1)
                },
                {
                    "ftitemcount", IntValidator.IsValidPositiveNonEmptyWithLength(4)
                },
                {
                    "parentid", StringValidator.IsValidOptionalNonEmptyWithLength(150)
                }
            };

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
    }
}