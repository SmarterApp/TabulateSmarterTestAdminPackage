﻿using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.Enums;
using TabulateSmarterTestAdminPackage.Common.Processors;
using TabulateSmarterTestAdminPackage.Common.Utilities;

namespace TabulateSmarterTestAdminPackage.Processors.Specification.TestSpecification.Administration.TestBlueprint
{
    public class TestBlueprintProcessor : Processor
    {
        public TestBlueprintProcessor(XPathNavigator navigator, PackageType packageType) : base(navigator, packageType)
        {
            Navigator.GenerateList("bpelement")
                .ForEach(x => Processors.Add(new BlueprintElementProcessor(x, packageType)));
        }
    }
}