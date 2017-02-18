﻿using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.Processors;
using TabulateSmarterTestAdminPackage.Common.RestrictedValues.Enums;
using TabulateSmarterTestAdminPackage.Common.Utilities;
using TabulateSmarterTestAdminPackage.Common.Validators.Convenience;

namespace TabulateSmarterTestPackage.Processors.TestSpecification.Administration.TestBlueprint
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