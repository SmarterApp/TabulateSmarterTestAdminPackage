using System.Xml.XPath;
using TabulateSmarterTestPackage.Common.RestrictedValues.Enums;
using TabulateSmarterTestPackage.Common.Utilities;
using TabulateSmarterTestPackage.Common.Validators.Convenience;
using TabulateSmarterTestPackage.Processors.Administration;

namespace TabulateSmarterTestPackage.Processors.Common.TestForm
{
    public class TestFormProcessor : Processor
    {
        public TestFormProcessor(XPathNavigator navigator, PackageType packageType) : base(navigator, packageType)
        {
            Attributes = new AttributeValidationDictionary
            {
                {
                    "length", IntValidator.IsValidNonEmptyWithLengthAndMinValue(10, 1)
                }
            };

            Navigator.GenerateList("identifier")
                .ForEach(x => Processors.Add(new IdentifierProcessor(x, packageType)));
            ReplaceAttributeValidation("identifier", new AttributeValidationDictionary
            {
                {
                    "uniqueid", StringValidator.IsValidNonEmptyWithLength(200)
                }
            });
            RemoveAttributeValidation("identifier", "label");

            Navigator.GenerateList("property")
                .ForEach(x => Processors.Add(new PropertyProcessor(x, packageType)));

            Navigator.GenerateList("poolproperty")
                .ForEach(x => Processors.Add(new PoolPropertyProcessor(x, packageType)));
            ReplaceAttributeValidation("poolproperty", Attributes = new AttributeValidationDictionary
            {
                {
                    "property", StringValidator.IsValidNonEmptyWithLength(200)
                },
                {
                    "value", StringValidator.IsValidNonEmptyWithLength(200)
                },
                {
                    "label", StringValidator.IsValidNonEmptyWithLength(200)
                }
            });

            Navigator.GenerateList("formpartition")
                .ForEach(x => Processors.Add(new TestFormPartitionProcessor(x, packageType)));
        }
    }
}