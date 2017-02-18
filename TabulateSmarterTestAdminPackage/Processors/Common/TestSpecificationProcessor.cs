using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.RestrictedValues.Enums;
using TabulateSmarterTestAdminPackage.Common.Utilities;
using TabulateSmarterTestAdminPackage.Common.Validators;
using TabulateSmarterTestAdminPackage.Common.Validators.Convenience;
using TabulateSmarterTestPackage.Processors.Administration;
using TabulateSmarterTestPackage.Processors.Scoring;

namespace TabulateSmarterTestPackage.Processors.Common
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
                case PackageType.Reporting:
                case PackageType.Registration:
                    break;
                default:
                    break;
            }
        }
    }
}