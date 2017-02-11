using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.Enums;
using TabulateSmarterTestAdminPackage.Common.Processors;
using TabulateSmarterTestAdminPackage.Common.Utilities;
using TabulateSmarterTestAdminPackage.Common.Validators;
using TabulateSmarterTestAdminPackage.Common.Validators.Convenience;

namespace TabulateSmarterTestAdminPackage.Processors.Specification.TestSpecification.Administration.ItemPool.TestItem
{
    internal class TestItemProcessor : Processor
    {
        internal TestItemProcessor(XPathNavigator navigator, PackageType packageType) : base(navigator, packageType)
        {
            Attributes = new AttributeValidationDictionary
            {
                {
                    "filename", StringValidator.IsValidNonEmptyWithLength(200)
                        .AddAndReturn(new FilePathValidator(ErrorSeverity.Degraded))
                },
                {
                    "itemtype", StringValidator.IsValidNonEmptyWithLength(50)
                }
            };
            Navigator.GenerateList("identifier").ForEach(x => Processors.Add(new IdentifierProcessor(x, packageType)));
            ReplaceAttributeValidation("identifier", new AttributeValidationDictionary
            {
                {
                    "uniqueid", StringValidator.IsValidNonEmptyWithLength(150)
                },
                {
                    "name", StringValidator.IsValidOptionalNonEmptyWithLength(80)
                }
            });
            RemoveAttributeValidation("identifier", "label");

            Navigator.GenerateList("bpref").ForEach(x => Processors.Add(new BPrefProcessor(x, packageType)));
            Navigator.GenerateList("passageref").ForEach(x => Processors.Add(new PassageRefProcessor(x, packageType)));
            Navigator.GenerateList("testitempool").ForEach(x => Processors.Add(new TestItemProcessor(x, packageType)));
            Navigator.GenerateList("itemscoreddimension")
                .ForEach(x => Processors.Add(new ItemScoredDimensionProcessor(x, packageType)));
        }
    }
}