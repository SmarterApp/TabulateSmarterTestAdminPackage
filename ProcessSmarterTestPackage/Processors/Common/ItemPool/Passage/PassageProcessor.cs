using System.Xml.XPath;
using SmarterTestPackage.Common.Data;
using SmarterTestPackage.Common.Extensions;
using ValidateSmarterTestPackage;
using ValidateSmarterTestPackage.Validators;
using ValidateSmarterTestPackage.Validators.Convenience;

namespace ProcessSmarterTestPackage.Processors.Common.ItemPool.Passage
{
    public class PassageProcessor : Processor
    {
        public PassageProcessor(XPathNavigator navigator, PackageType packageType) : base(navigator, packageType)
        {
            Attributes =
                new AttributeValidationDictionary
                {
                    {
                        "filename", StringValidator.IsValidNonEmptyWithLength(200)
                            .AddAndReturn(new RequiredRegularExpressionValidator(ErrorSeverity.Degraded, @"^[\S]*[.]xml"))
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