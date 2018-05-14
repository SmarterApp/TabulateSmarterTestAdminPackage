using System.Collections.Generic;
using System.Linq;
using System.Xml.XPath;
using ProcessSmarterTestPackage.PostProcessors;
using ProcessSmarterTestPackage.Processors.Common;
using SmarterTestPackage.Common.Data;
using SmarterTestPackage.Common.Extensions;
using ValidateSmarterTestPackage;
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
            ReplaceAttributeValidation("identifier", new AttributeValidationDictionary
            {
                {
                    "version", DecimalValidator.IsValidPositiveNonEmptyWithLength(10)
                }
            });
            Navigator.GenerateList("computationruleparameter")
                .ForEach(x => Processors.Add(new ComputationRuleParameterProcessor(x, packageType)));
        }

        protected override List<ValidationError> AdditionalValidations()
        {
            return new ComputationRulePostProcessor(PackageType, this).GenerateErrors().ToList();
        }
    }
}