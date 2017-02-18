using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.RestrictedValues.Enums;
using TabulateSmarterTestAdminPackage.Common.Utilities;
using TabulateSmarterTestAdminPackage.Common.Validators;
using TabulateSmarterTestAdminPackage.Common.Validators.Convenience;

namespace TabulateSmarterTestPackage.Processors.TestSpecification.Common.ItemPool.Passage
{
    public class PassageProcessor : Processor
    {
        public PassageProcessor(XPathNavigator navigator, PackageType packageType) : base(navigator, packageType)
        {
            Attributes = new AttributeValidationDictionary
            {
                {
                    "filename", StringValidator.IsValidNonEmptyWithLength(200)
                        .AddAndReturn(new FilePathValidator(ErrorSeverity.Degraded))
                }
            };

            Navigator.GenerateList("identifier").ForEach(x => Processors.Add(new IdentifierProcessor(x, packageType)));
            ReplaceAttributeValidation("identifier", new AttributeValidationDictionary
            {
                {
                    "uniqueid", StringValidator.IsValidNonEmptyWithLength(100)
                },
                {
                    "name", StringValidator.IsValidOptionalNonEmptyWithLength(100)
                }
            });
            RemoveAttributeValidation("identifier", "label");
        }
    }
}