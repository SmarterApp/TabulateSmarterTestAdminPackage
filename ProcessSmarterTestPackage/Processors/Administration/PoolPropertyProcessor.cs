﻿using System.Xml.XPath;
using ProcessSmarterTestPackage.Processors.Common;
using SmarterTestPackage.Common.Data;
using ValidateSmarterTestPackage;
using ValidateSmarterTestPackage.RestrictedValues.Enums;
using ValidateSmarterTestPackage.Validators;
using ValidateSmarterTestPackage.Validators.Convenience;

namespace ProcessSmarterTestPackage.Processors.Administration
{
    public class PoolPropertyProcessor : Processor
    {
        public PoolPropertyProcessor(XPathNavigator navigator, PackageType packageType) : base(navigator, packageType)
        {
            Attributes = new AttributeValidationDictionary
            {
                {
                    "property", StringValidator.IsValidNonEmptyWithLength(50)
                },
                {
                    "value", StringValidator.IsValidNonEmptyWithLength(128)
                },
                {
                    "label", StringValidator.IsValidNonEmptyWithLength(150)
                },
                {
                    "itemcount", IntValidator.IsValidPositiveNonEmptyWithLength(10)
                }
            };
            ApplySecondaryValidation("property", "--ITEMTYPE--", "value", new RequiredEnumValidator(
                ErrorSeverity.Severe,
                RestrictedListItems.ItemType
            ));
        }
    }
}