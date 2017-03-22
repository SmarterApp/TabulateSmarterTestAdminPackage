using System.Collections.Generic;
using System.Linq;
using System.Xml.XPath;
using ProcessSmarterTestPackage.PostProcessors;
using SmarterTestPackage.Common.Data;
using SmarterTestPackage.Common.Extensions;

namespace ProcessSmarterTestPackage.Processors.Common.TestBlueprint
{
    public class TestBlueprintProcessor : Processor
    {
        public TestBlueprintProcessor(XPathNavigator navigator, PackageType packageType) : base(navigator, packageType)
        {
            Navigator.GenerateList("bpelement")
                .ForEach(x => Processors.Add(new BlueprintElementProcessor(x, packageType)));
        }

        public override List<ValidationError> AdditionalValidations()
        {
            return new TestBlueprintPostProcessor(PackageType, this).GenerateErrors().ToList();
        }
    }
}