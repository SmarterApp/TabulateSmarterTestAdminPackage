using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.Enums;
using TabulateSmarterTestAdminPackage.Common.Utilities;

namespace TabulateSmarterTestAdminPackage.Common.Processors
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
        public IDictionary<string, ValidatedAttribute> ValidatedAttributes { get; set; } = new Dictionary<string, ValidatedAttribute>();

        public void Dispose() {}

        public bool Process()
        {
            ValidatedAttributes = Attributes.Validate(Navigator);
            ValidatedAttributes
                .Where(x => !x.Value.IsValid)
                .ToList()
                .ForEach(x =>
                    ReportingUtility.ReportError(ReportingUtility.TestName, x.Value.Validator.ErrorSeverity, x.Key,
                        $"{Navigator.NamespaceURI} attribute {x.Key} violates {x.Value.Validator.GetMessage()}"));

            var badProcessors = Processors.Count(x => !x.Process());
            return ValidatedAttributes.Values.Count(x => !x.IsValid) == 0
                   && badProcessors == 0;
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
    }
}