﻿using System.Xml.XPath;
using ProcessSmarterTestPackage.Processors.Common;
using SmarterTestPackage.Common.Data;
using ValidateSmarterTestPackage;
using ValidateSmarterTestPackage.Validators.Convenience;

namespace ProcessSmarterTestPackage.Processors.Administration.AdminSegment
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