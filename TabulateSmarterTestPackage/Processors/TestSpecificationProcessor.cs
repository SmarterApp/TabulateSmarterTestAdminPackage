using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.Enums;
using TabulateSmarterTestAdminPackage.Common.Processors;
using TabulateSmarterTestAdminPackage.Common.Utilities;
using TabulateSmarterTestAdminPackage.Common.Validators;
using TabulateSmarterTestAdminPackage.Common.Validators.Convenience;
using TabulateSmarterTestAdminPackage.Processors.Specification.TestSpecification.Administration;
using TabulateSmarterTestScoringPackage.Processors.Scoring;

namespace TabulateSmarterTestPackage.Tabulators
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
                    Attributes["publisher"] = StringValidator.IsValidNonEmptyWithLength(255);
                    Attributes["publishdate"] = DateTimeValidator.IsValidNonEmptyWithLength(30);
                    Attributes["version"] = DecimalValidator.IsValidPositiveNonEmptyWithLength(10);
                    Navigator.GenerateList("scoring")
                        .ForEach(x => Processors.Add(new ScoringProcessor(x, packageType)));
                    break;
                case PackageType.Reporting:
                    break;
                default:
                    break;
            }
        }
    }
}