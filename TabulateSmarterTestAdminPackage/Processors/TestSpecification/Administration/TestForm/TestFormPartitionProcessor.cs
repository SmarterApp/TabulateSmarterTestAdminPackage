using System.Linq;
using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.Enums;
using TabulateSmarterTestAdminPackage.Common.Processors;
using TabulateSmarterTestAdminPackage.Common.Utilities;
using TabulateSmarterTestAdminPackage.Common.Validators.Convenience;
using TabulateSmarterTestPackage.Processors.TestSpecification.Administration.AdminSegment;
using TabulateSmarterTestPackage.Processors.TestSpecification.Administration.ItemPool.TestItem;

namespace TabulateSmarterTestPackage.Processors.TestSpecification.Administration.TestForm
{
    public class TestFormPartitionProcessor : Processor
    {
        public TestFormPartitionProcessor(XPathNavigator navigator, PackageType packageType)
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
                .ForEach(x => x.Attributes.Add("formposition", IntValidator.IsValidNonEmptyWithLengthAndMinValue(10, 1)));

            Processors.Where(x => x.Navigator.Name.Equals("itemgroup")).ToList()
                .ForEach(x => x.Navigator.GenerateList("passageref")
                    .ForEach(y => x.Processors.Add(new PassageRefProcessor(y, packageType))));
        }
    }
}