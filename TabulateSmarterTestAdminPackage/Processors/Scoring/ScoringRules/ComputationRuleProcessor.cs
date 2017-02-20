using System.Xml.XPath;
using TabulateSmarterTestPackage.Common.RestrictedValues.Enums;
using TabulateSmarterTestPackage.Common.Utilities;
using TabulateSmarterTestPackage.Common.Validators.Convenience;
using TabulateSmarterTestPackage.Processors.Common;

namespace TabulateSmarterTestPackage.Processors.Scoring.ScoringRules
{
    public class ComputationRuleProcessor : Processor
    {
        public ComputationRuleProcessor(XPathNavigator navigator, PackageType packageType)
            : base(navigator, packageType)
        {
            Attributes = new AttributeValidationDictionary
            {
                {
                    "computationorder", IntValidator.IsValidNonEmptyWithLengthAndMinValue(10, 1)
                },
                {
                    "bpelementid", StringValidator.IsValidNonEmptyWithLength(150)
                }
            };

            Navigator.GenerateList("identifier")
                .ForEach(x => Processors.Add(new IdentifierProcessor(x, packageType)));
            Navigator.GenerateList("computationruleparameter")
                .ForEach(x => Processors.Add(new ComputationRuleParameterProcessor(x, packageType)));
        }
    }
}