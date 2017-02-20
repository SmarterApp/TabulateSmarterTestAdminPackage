﻿using System.Xml.XPath;
using TabulateSmarterTestPackage.Common.RestrictedValues.Enums;
using TabulateSmarterTestPackage.Common.Utilities;
using TabulateSmarterTestPackage.Processors.Common;

namespace TabulateSmarterTestPackage.Processors.Administration.AdminSegment
{
    public class SegmentPoolProcessor : Processor
    {
        public SegmentPoolProcessor(XPathNavigator navigator, PackageType packageType) : base(navigator, packageType)
        {
            Navigator.GenerateList("itemgroup")
                .ForEach(x => Processors.Add(new ItemGroupProcessor(x, packageType)));
        }
    }
}