﻿using System.Collections.Generic;
using System.Linq;
using System.Xml.XPath;
using ProcessSmarterTestPackage.PostProcessors;
using ProcessSmarterTestPackage.Processors.Common;
using SmarterTestPackage.Common.Data;
using SmarterTestPackage.Common.Extensions;

namespace ProcessSmarterTestPackage.Processors.Scoring.PerformanceLevels
{
    public class PerformanceLevelsProcessor : Processor
    {
        public PerformanceLevelsProcessor(XPathNavigator navigator, PackageType packageType)
            : base(navigator, packageType)
        {
            Navigator.GenerateList("performancelevel")
                .ForEach(x => Processors.Add(new PerformanceLevelProcessor(x, packageType)));
        }

        protected override List<ValidationError> AdditionalValidations()
        {
            return new PerformanceLevelsPostProcessor(PackageType, this).GenerateErrors().ToList();
        }
    }
}