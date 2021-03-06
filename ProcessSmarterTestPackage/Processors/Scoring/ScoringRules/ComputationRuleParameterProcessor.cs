﻿using System.Collections.Generic;
using System.Linq;
using System.Xml.XPath;
using ProcessSmarterTestPackage.PostProcessors;
using ProcessSmarterTestPackage.Processors.Common;
using SmarterTestPackage.Common.Data;
using SmarterTestPackage.Common.Extensions;
using ValidateSmarterTestPackage;
using ValidateSmarterTestPackage.RestrictedValues.Enums;
using ValidateSmarterTestPackage.Validators;
using ValidateSmarterTestPackage.Validators.Convenience;

namespace ProcessSmarterTestPackage.Processors.Scoring.ScoringRules
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
                },
                {
                    "version", DecimalValidator.IsValidPositiveNonEmptyWithLength(10)
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

            Navigator.GenerateList("computationruleparametervalue")
                .ForEach(x => Processors.Add(new ComputationRuleParameterValueProcessor(x, packageType)));
        }

        protected override List<ValidationError> AdditionalValidations()
        {
            return new ComputationRuleParameterPostProcessor(PackageType, this).GenerateErrors().ToList();
        }
    }
}