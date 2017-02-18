using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.RestrictedValues.Enums;
using TabulateSmarterTestAdminPackage.Common.Utilities;
using TabulateSmarterTestPackage.Processors.Common;

namespace TabulateSmarterTestPackage.Processors.Scoring.PerformanceLevels
{
    public class PerformanceLevelsProcessor : Processor
    {
        public PerformanceLevelsProcessor(XPathNavigator navigator, PackageType packageType)
            : base(navigator, packageType)
        {
            Navigator.GenerateList("performancelevel")
                .ForEach(x => Processors.Add(new PerformanceLevelProcessor(x, packageType)));
        }
    }
}