using System.Collections.Generic;
using ProcessSmarterTestPackage.Processors.Common;
using SmarterTestPackage.Common.Data;
using ValidateSmarterTestPackage.Validators;

namespace ProcessSmarterTestPackage.PostProcessors
{
    public class ComputationRuleParameterPostProcessor : PostProcessor
    {
        public ComputationRuleParameterPostProcessor(PackageType packageType, Processor processor)
            : base(packageType, processor) {}

        public override IList<ValidationError> GenerateErrors()
        {
            var result = new List<ValidationError>();

            var computationRuleParameterValues = Processor.ChildNodesWithName("computationruleparametervalue");
            foreach (var computationRuleParameterValue in computationRuleParameterValues)
            {
                var parameterType = Processor.ValueForAttribute("parametertype");
                switch (parameterType)
                {
                    case "int":
                        var intValidator = new RequiredIntValidator(ErrorSeverity.Degraded);
                        if (!intValidator.IsValid(computationRuleParameterValue.ValueForAttribute("value")))
                        {
                            result.Add(new ValidationError
                            {
                                ErrorSeverity = ErrorSeverity.Degraded,
                                GeneratedMessage = intValidator.GetMessage(),
                                Key = "value",
                                PackageType = PackageType,
                                ItemId = Processor.ChildNodeWithName("identifier").ValueForAttribute("uniqueid"),
                                Location = "computationruleparametervalue",
                                Value = computationRuleParameterValue.Navigator.OuterXml
                            });
                        }
                        break;
                    case "double":
                        var doubleValidator = new RequiredDoubleValidator(ErrorSeverity.Degraded);
                        if (!doubleValidator.IsValid(computationRuleParameterValue.ValueForAttribute("value")))
                        {
                            result.Add(new ValidationError
                            {
                                ErrorSeverity = ErrorSeverity.Degraded,
                                GeneratedMessage = doubleValidator.GetMessage(),
                                Key = "value",
                                PackageType = PackageType,
                                ItemId = Processor.ChildNodeWithName("identifier").ValueForAttribute("uniqueid"),
                                Location = "computationruleparametervalue",
                                Value = computationRuleParameterValue.Navigator.OuterXml
                            });
                        }
                        break;
                    default:
                        continue;
                }
            }

            return result;
        }
    }
}