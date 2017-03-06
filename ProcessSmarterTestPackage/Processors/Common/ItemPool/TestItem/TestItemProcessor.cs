using System.Collections.Generic;
using System.Linq;
using System.Xml.XPath;
using ProcessSmarterTestPackage.PostProcessors;
using ProcessSmarterTestPackage.Processors.Administration;
using SmarterTestPackage.Common.Data;
using SmarterTestPackage.Common.Extensions;
using ValidateSmarterTestPackage;
using ValidateSmarterTestPackage.RestrictedValues.Enums;
using ValidateSmarterTestPackage.Validators;
using ValidateSmarterTestPackage.Validators.Convenience;

namespace ProcessSmarterTestPackage.Processors.Common.ItemPool.TestItem
{
    public class TestItemProcessor : Processor
    {
        public TestItemProcessor(XPathNavigator navigator, PackageType packageType) : base(navigator, packageType)
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

        public override List<ValidationError> AdditionalValidations()
        {
            return new TestItemPostProcessor(PackageType, this).GenerateErrors().ToList();
        }
    }
}