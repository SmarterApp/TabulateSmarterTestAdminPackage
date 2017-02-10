using System.Linq;
using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.Enums;
using TabulateSmarterTestAdminPackage.Common.Processors;
using TabulateSmarterTestAdminPackage.Common.Utilities;
using TabulateSmarterTestAdminPackage.Common.Validators;
using TabulateSmarterTestAdminPackage.Common.Validators.Convenience;
using TabulateSmarterTestAdminPackage.Processors.Specification.TestSpecification.Administration;

namespace TabulateSmarterTestAdminPackage.Processors.Specification.TestSpecification
{
    internal class TestSpecificationProcessor : Processor
    {
        internal TestSpecificationProcessor(XPathNavigator navigator, PackageType expectedPackageType) : base(navigator)
        {
            ExpectedPackageType = expectedPackageType;

            Attributes = new AttributeValidationDictionary
            {
                {
                    "purpose", StringValidator.IsValidNonEmptyWithLength(100).AddAndReturn(
                        new StringMatchValidator(ErrorSeverity.Degraded, ExpectedPackageType.ToString())
                    )
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

            Navigator.GenerateList("property").ForEach(x => Processors.Add(new PropertyProcessor(x)));
            Navigator.GenerateList("identifier").ForEach(x => Processors.Add(new IdentifierProcessor(x)));
            Navigator.GenerateList("administration").ForEach(x => Processors.Add(new AdministrationProcessor(x)));
        }

        private string Purpose { get; set; }
        public string Publisher { get; set; }
        public string PublishDate { get; set; }
        public string Version { get; set; }
        public PackageType ExpectedPackageType { get; set; }

        public override bool Process()
        {
            var validationResults = Attributes.Validate(Navigator);
            Purpose = validationResults["purpose"].Value;
            Publisher = validationResults["publisher"].Value;
            PublishDate = validationResults["publishdate"].Value;
            Version = validationResults["version"].Value;
            validationResults
                .Where(x => !x.Value.IsValid)
                .ToList()
                .ForEach(x =>
                    ReportingUtility.ReportError(ReportingUtility.TestName, x.Value.Validator.ErrorSeverity, x.Key,
                        $"{Navigator.NamespaceURI} attribute {x.Key} violates {x.Value.Validator.GetMessage()}"));

            var badProcessors = Processors.Count(x => !x.Process());
            return validationResults.Values.Count(x => !x.IsValid) == 0
                   && badProcessors == 0;
        }
    }
}