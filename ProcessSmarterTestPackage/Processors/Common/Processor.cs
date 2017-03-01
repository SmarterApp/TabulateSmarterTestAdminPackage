using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.XPath;
using SmarterTestPackage.Common.Data;
using SmarterTestPackage.Common.Extensions;
using ValidateSmarterTestPackage;
using ValidateSmarterTestPackage.RestrictedValues.Enums;
using ValidateSmarterTestPackage.Validators;

namespace ProcessSmarterTestPackage.Processors.Common
{
    public abstract class Processor : IDisposable
    {
        public readonly XPathNavigator Navigator;

        protected Processor(XPathNavigator navigator, PackageType packageType)
        {
            Navigator = navigator;
            PackageType = packageType;
        }

        public IList<Processor> Processors { get; } = new List<Processor>();
        public PackageType PackageType { get; }

        public AttributeValidationDictionary Attributes { get; set; } = new AttributeValidationDictionary();

        public IDictionary<string, ValidatedAttribute> ValidatedAttributes { get; set; } =
            new Dictionary<string, ValidatedAttribute>();

        public void Dispose() {}

        public virtual bool Process()
        {
            ValidatedAttributes = Attributes.Validate(Navigator);
            var badProcessors = Processors.Count(x => !x.Process());
            return ValidatedAttributes.Values.Count(x => !x.IsValid) == 0
                   && badProcessors == 0;
        }

        public IList<ValidationError> GenerateErrorMessages()
        {
            var result = new List<ValidationError>();
            ValidatedAttributes
                .Where(x => !x.Value.IsValid)
                .ToList()
                .ForEach(x =>
                    result.Add(new ValidationError
                        {
                            ErrorSeverity = x.Value.Validator.ErrorSeverity,
                            GeneratedMessage = x.Value.Validator.GetMessage(),
                            Key = x.Key,
                            Location = Navigator.Name,
                            Path = Navigator.OuterXml
                        }
                    ));
            var childErrors = Processors.SelectMany(x => x.GenerateErrorMessages()).ToList();
            childErrors.ForEach(x => x.Location = $"{Navigator.Name}/{x.Location}");
            result.AddRange(childErrors);
            return result;
        }

        protected void ReplaceAttributeValidation(string processorName,
            AttributeValidationDictionary attributeValidationDictionary)
        {
            foreach (var processor in Processors.Where(x => x.Navigator.Name.Equals(processorName)))
            {
                foreach (var validationPairs in attributeValidationDictionary)
                {
                    processor.Attributes[validationPairs.Key] = validationPairs.Value;
                }
            }
        }

        protected void RemoveAttributeValidation(string processorName, string attributeName)
        {
            foreach (var processor in Processors.Where(x => x.Navigator.Name.Equals(processorName)))
            {
                processor.Attributes.Remove(attributeName);
            }
        }

        protected void ApplySecondaryValidation(string key, string value, string affectedProperty, Validator validation)
        {
            if (Attributes.ContainsKey(key) &&
                Navigator.Eval(XPathExpression.Compile($"@{key}")).Equals(value, StringComparison.OrdinalIgnoreCase))
            {
                if (Attributes[affectedProperty] is ValidatorCollection)
                {
                    ((ValidatorCollection) Attributes[affectedProperty]).Add(validation);
                }
                else
                {
                    Attributes[affectedProperty] = new ValidatorCollection
                    {
                        (Validator) Attributes[affectedProperty],
                        validation
                    };
                }
            }
        }

        public IEnumerable<Processor> ChildNodesWithName(string name)
        {
            return Processors.Where(x => x.Navigator.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public Processor ChildNodeWithName(string name)
        {
            return Processors.First(x => x.Navigator.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public Processor ChildNodeWithNameAndPropertyValue(string name, string propertyName, string propertyValue)
        {
            return
                ChildNodesWithName(name)
                    .First(
                        x =>
                            x.ValueForAttribute(propertyName).Equals(propertyValue,
                                StringComparison.OrdinalIgnoreCase));
        }

        public string ValueForAttribute(string attribute)
        {
            return ValidatedAttributes[attribute].Value;
        }

        public IEnumerable<CrossPackageValidationError> EqualTo(Processor processor,
            IEnumerable<string> restrictedToKeys = null)
        {
            if (restrictedToKeys == null)
            {
                restrictedToKeys = processor.ValidatedAttributes.Keys;
            }
            var result =
                new List<CrossPackageValidationError>();
            if (processor == null || ValidatedAttributes == null)
            {
                return result;
            }
            foreach (var key in processor.ValidatedAttributes.Keys.Where(x => restrictedToKeys.Contains(x)))
            {
                var errorMessage = new CrossPackageValidationError
                {
                    ErrorSeverity = ErrorSeverity.Severe,
                    GeneratedMessage = string.Empty,
                    ItemId = string.Empty,
                    Key = key,
                    Location = Navigator.InnerXml,
                    Path = Navigator.Name,
                    PrimarySource = string.Empty,
                    SecondarySource = string.Empty,
                    TestName = string.Empty
                };
                if (!ValidatedAttributes.ContainsKey(key))
                {
                    errorMessage.GeneratedMessage = $"[RequiredProperty={key}]";
                    result.Add(errorMessage);
                }
                else if (!ValidatedAttributes[key].Value.Equals(processor.ValidatedAttributes[key].Value))
                {
                    errorMessage.GeneratedMessage =
                        $"{errorMessage.GeneratedMessage}[RequiredProperty {key} Value {ValidatedAttributes[key].Value}!={processor.ValidatedAttributes[key].Value}]";
                    result.Add(errorMessage);
                }
            }
            return result;
        }
    }
}