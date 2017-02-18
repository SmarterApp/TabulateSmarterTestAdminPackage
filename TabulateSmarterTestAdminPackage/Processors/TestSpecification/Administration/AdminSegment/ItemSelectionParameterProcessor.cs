﻿using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.Processors;
using TabulateSmarterTestAdminPackage.Common.RestrictedValues.Enums;
using TabulateSmarterTestAdminPackage.Common.Utilities;
using TabulateSmarterTestAdminPackage.Common.Validators.Convenience;

namespace TabulateSmarterTestPackage.Processors.TestSpecification.Administration.AdminSegment
{
    public class ItemSelectionParameterProcessor : Processor
    {
        public ItemSelectionParameterProcessor(XPathNavigator navigator, PackageType packageType)
            : base(navigator, packageType)
        {
            Attributes = new AttributeValidationDictionary
            {
                {
                    "bpelementid", StringValidator.IsValidNonEmptyWithLength(150)
                }
            };

            Navigator.GenerateList("property")
                .ForEach(x => Processors.Add(new PropertyProcessor(x, packageType)));
            ReplaceAttributeValidation("property", new AttributeValidationDictionary
            {
                {
                    "value", StringValidator.IsValidNonEmptyWithLength(100)
                },
                {
                    "label", StringValidator.IsValidNonEmptyWithLength(500)
                }
            });
        }
    }
}