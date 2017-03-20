using System.Collections.Generic;
using System.Linq;
using System.Xml.XPath;
using ProcessSmarterTestPackage.PostProcessors;
using ProcessSmarterTestPackage.Processors.Administration;
using ProcessSmarterTestPackage.Processors.Scoring;
using SmarterTestPackage.Common.Data;
using SmarterTestPackage.Common.Extensions;
using ValidateSmarterTestPackage;
using ValidateSmarterTestPackage.Validators;
using ValidateSmarterTestPackage.Validators.Convenience;

namespace ProcessSmarterTestPackage.Processors.Common
{
    public class TestSpecificationProcessor : Processor
    {
        public TestSpecificationProcessor(XPathNavigator navigator, PackageType packageType)
            : base(navigator, packageType)
        {
            Attributes = new AttributeValidationDictionary
            {
                {
                    "purpose", StringValidator.IsValidNonEmptyWithLength(100)
                },
                {
                    "publisher", StringValidator.IsValidNonEmptyWithLength(255).AddAndReturn(
                        new RequiredStringValidator(ErrorSeverity.Severe)).AddAndReturn(
                        new MaxLengthValidator(ErrorSeverity.Severe, 100))
                },
                {
                    "publishdate", DateTimeValidator.IsValidNonEmptyWithLength(200)
                },
                {
                    "version", DecimalValidator.IsValidPositiveNonEmptyWithLength(20)
                }
            };

            Navigator.GenerateList("property").ForEach(x => Processors.Add(new PropertyProcessor(x, packageType)));
            Navigator.GenerateList("identifier").ForEach(x => Processors.Add(new IdentifierProcessor(x, packageType)));
            switch (packageType)
            {
                case PackageType.Administration:
                    Navigator.GenerateList("administration")
                        .ForEach(x => Processors.Add(new AdministrationProcessor(x, packageType)));
                    break;
                case PackageType.Scoring:
                    Navigator.GenerateList("scoring")
                        .ForEach(x => Processors.Add(new ScoringProcessor(x, packageType)));
                    break;
            }
        }

        public override List<ValidationError> AdditionalValidations()
        {
            return new TestSpecificationPostProcessor(PackageType, this).GenerateErrors().ToList();
        }

        public string GetUniqueId()
        {
            return ChildNodeWithName("identifier").ValueForAttribute("uniqueid");
        }
    }
}