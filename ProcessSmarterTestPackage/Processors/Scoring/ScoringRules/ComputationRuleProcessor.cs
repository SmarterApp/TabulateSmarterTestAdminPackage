using System.Xml.XPath;
using ProcessSmarterTestPackage.Processors.Common;
using SmarterTestPackage.Common.Extensions;
using ValidateSmarterTestPackage;
using ValidateSmarterTestPackage.RestrictedValues.Enums;
using ValidateSmarterTestPackage.Validators.Convenience;

namespace ProcessSmarterTestPackage.Processors.Scoring.ScoringRules
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