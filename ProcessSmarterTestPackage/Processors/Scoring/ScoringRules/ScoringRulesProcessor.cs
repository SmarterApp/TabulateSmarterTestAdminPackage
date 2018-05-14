using System.Collections.Generic;
using System.Linq;
using System.Xml.XPath;
using ProcessSmarterTestPackage.PostProcessors;
using ProcessSmarterTestPackage.Processors.Common;
using SmarterTestPackage.Common.Data;
using SmarterTestPackage.Common.Extensions;

namespace ProcessSmarterTestPackage.Processors.Scoring.ScoringRules
{
    public class ScoringRulesProcessor : Processor
    {
        public ScoringRulesProcessor(XPathNavigator navigator, PackageType packageType) : base(navigator, packageType)
        {
            Navigator.GenerateList("computationrule")
                .ForEach(x => Processors.Add(new ComputationRuleProcessor(x, packageType)));
        }

        protected override List<ValidationError> AdditionalValidations()
        {
            return new ScoringRulesPostProcessor(PackageType, this).GenerateErrors().ToList();
        }
    }
}