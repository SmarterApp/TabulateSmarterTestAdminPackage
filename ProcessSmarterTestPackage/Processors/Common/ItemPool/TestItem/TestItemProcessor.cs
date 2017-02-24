using System.Xml.XPath;
using ProcessSmarterTestPackage.Processors.Administration;
using TabulateSmarterTestPackage.Common.RestrictedValues.Enums;
using TabulateSmarterTestPackage.Common.Utilities;
using ValidateSmarterTestPackage;
using ValidateSmarterTestPackage.Validators;
using ValidateSmarterTestPackage.Validators.Convenience;
using ValidateSmarterTestPackage.Validators.CrossTabulation;

namespace ProcessSmarterTestPackage.Processors.Common.ItemPool.TestItem
{
    public class TestItemProcessor : Processor
    {
        public TestItemProcessor(XPathNavigator navigator, PackageType packageType) : base(navigator, packageType)
        {
            if (string.IsNullOrEmpty(ReportingUtility.ContentDirectoryPath))
            {
                Attributes = new AttributeValidationDictionary
                {
                    {
                        "filename", StringValidator.IsValidNonEmptyWithLength(200)
                    },
                    {
                        "itemtype", StringValidator.IsValidNonEmptyWithLength(50)
                            .AddAndReturn(new RequiredEnumValidator(ErrorSeverity.Degraded,
                                RestrictedListItems.ItemType))
                    }
                };
            }
            else
            {
                Attributes = new AttributeValidationDictionary
                {
                    {
                        "filename", StringValidator.IsValidNonEmptyWithLength(200)
                            .AddAndReturn(new ItemExistsValidator(ErrorSeverity.Degraded,
                                ReportingUtility.ContentDirectoryPath))
                    },
                    {
                        "itemtype", StringValidator.IsValidNonEmptyWithLength(50)
                            .AddAndReturn(new RequiredEnumValidator(ErrorSeverity.Degraded,
                                RestrictedListItems.ItemType))
                    }
                };
            }
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

            Navigator.GenerateList("bpref").ForEach(x => Processors.Add(new BpRefProcessor(x, packageType)));
            Navigator.GenerateList("passageref").ForEach(x => Processors.Add(new PassageRefProcessor(x, packageType)));

            Navigator.GenerateList("poolproperty")
                .ForEach(x => Processors.Add(new PoolPropertyProcessor(x, packageType)));
            RemoveAttributeValidation("poolproperty", "itemcount");

            Navigator.GenerateList("itemscoreddimension")
                .ForEach(x => Processors.Add(new ItemScoredDimensionProcessor(x, packageType)));
        }
    }
}