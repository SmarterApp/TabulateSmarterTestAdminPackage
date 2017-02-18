using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.RestrictedValues.Enums;
using TabulateSmarterTestAdminPackage.Common.Utilities;
using TabulateSmarterTestAdminPackage.Common.Validators;
using TabulateSmarterTestAdminPackage.Common.Validators.Convenience;
using TabulateSmarterTestPackage.Processors.TestSpecification.Common;

namespace TabulateSmarterTestPackage.Processors.TestSpecification.Scoring.ScoringRules
{
    public class ComputationRuleParameterProcessor : Processor
    {
        public ComputationRuleParameterProcessor(XPathNavigator navigator, PackageType packageType)
            : base(navigator, packageType)
        {
            Attributes = new AttributeValidationDictionary
            {
                {
                    "position", IntValidator.IsValidNonEmptyWithLengthAndMinValue(10, 1)
                },
                {
                    "parametertype",
                    StringValidator.IsValidNonEmptyWithLength(16)
                        .AddAndReturn(new RequiredEnumValidator(ErrorSeverity.Degraded,
                            RestrictedListItems.ParameterType))
                }
            };

            Navigator.GenerateList("identifier")
                .ForEach(x => Processors.Add(new IdentifierProcessor(x, packageType)));
            RemoveAttributeValidation("identifier", "label");
            ReplaceAttributeValidation("identifier", new AttributeValidationDictionary
            {
                {
                    "name", StringValidator.IsValidNonEmptyWithLength(128)
                }
            });

            Navigator.GenerateList("property")
                .ForEach(x => Processors.Add(new PropertyProcessor(x, packageType)));
            RemoveAttributeValidation("property", "label");
            ReplaceAttributeValidation("property", new AttributeValidationDictionary
            {
                {
                    "value",
                    StringValidator.IsValidNonEmptyWithLength(200)
                        .AddAndReturn(new RequiredEnumValidator(ErrorSeverity.Degraded,
                            RestrictedListItems.ParameterType))
                }
            });
        }
    }
}