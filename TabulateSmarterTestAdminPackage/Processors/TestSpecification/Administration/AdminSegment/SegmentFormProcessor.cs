﻿using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.Processors;
using TabulateSmarterTestAdminPackage.Common.RestrictedValues.Enums;
using TabulateSmarterTestAdminPackage.Common.Utilities;
using TabulateSmarterTestAdminPackage.Common.Validators.Convenience;

namespace TabulateSmarterTestPackage.Processors.TestSpecification.Administration.AdminSegment
{
    public class SegmentFormProcessor : Processor
    {
        public SegmentFormProcessor(XPathNavigator navigator, PackageType packageType) : base(navigator, packageType)
        {
            Attributes = new AttributeValidationDictionary
            {
                {
                    "formpartitionid", StringValidator.IsValidNonEmptyWithLength(100)
                }
            };
        }
    }
}