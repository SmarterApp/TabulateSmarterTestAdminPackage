using System.Xml.XPath;
using TabulateSmarterTestPackage.Common.RestrictedValues.Enums;
using TabulateSmarterTestPackage.Common.Utilities;
using TabulateSmarterTestPackage.Common.Validators.Convenience;
using TabulateSmarterTestPackage.Common.Validators.CrossTabulation;

namespace TabulateSmarterTestPackage.Processors.Common.ItemPool.Passage
{
    public class PassageProcessor : Processor
    {
        public PassageProcessor(XPathNavigator navigator, PackageType packageType) : base(navigator, packageType)
        {
            Attributes = string.IsNullOrEmpty(ReportingUtility.ContentDirectoryPath)
                ? new AttributeValidationDictionary
                {
                    {
                        "filename", StringValidator.IsValidNonEmptyWithLength(200)
                    }
                }
                : new AttributeValidationDictionary
                {
                    {
                        "filename",
                        StringValidator.IsValidNonEmptyWithLength(200)
                            .AddAndReturn(new StimuliExistsValidator(ErrorSeverity.Degraded,
                                ReportingUtility.ContentDirectoryPath))
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