using System.Collections.Generic;
using System.Linq;
using System.Xml.XPath;
using ProcessSmarterTestPackage.PostProcessors;
using ProcessSmarterTestPackage.Processors.Administration.AdminSegment;
using ProcessSmarterTestPackage.Processors.Common.ItemPool.TestItem;
using SmarterTestPackage.Common.Data;
using SmarterTestPackage.Common.Extensions;
using ValidateSmarterTestPackage;
using ValidateSmarterTestPackage.Validators.Convenience;

namespace ProcessSmarterTestPackage.Processors.Common.TestForm
{
    public class FormPartitionProcessor : Processor
    {
        public FormPartitionProcessor(XPathNavigator navigator, PackageType packageType)
            : base(navigator, packageType)
        {
            Navigator.GenerateList("identifier")
                .ForEach(x => Processors.Add(new IdentifierProcessor(x, packageType)));
            ReplaceAttributeValidation("identifier", new AttributeValidationDictionary
            {
                {
                    "uniqueid", StringValidator.IsValidNonEmptyWithLength(100)
                }
            });
            RemoveAttributeValidation("identifier", "label");

            Navigator.GenerateList("itemgroup")
                .ForEach(x => Processors.Add(new ItemGroupProcessor(x, packageType)));

            Processors.Where(x => x.Navigator.Name.Equals("itemgroup")).ToList()
                .ForEach(x => x.Navigator.GenerateList("passageref")
                    .ForEach(y => x.Processors.Add(new PassageRefProcessor(y, packageType))));
        }

        public override List<ValidationError> AdditionalValidations()
        {
            return new FormPartitionPostProcessor(PackageType, this).GenerateErrors().ToList();
        }
    }
}