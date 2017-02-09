using System.Linq;
using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.Enums;
using TabulateSmarterTestAdminPackage.Common.Processors;
using TabulateSmarterTestAdminPackage.Common.Utilities;
using TabulateSmarterTestAdminPackage.Common.Validators;
using TabulateSmarterTestAdminPackage.Common.Validators.Convenience;

namespace TabulateSmarterTestScoringPackage.Processors
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
                    // Added a method to the ValidatorCollection that returns the collection after an add so convenience methods can be used as a base
                },
                {
                    "publisher", StringValidator.IsValidNonEmptyWithLength(255)
                    // This is the most common type of validation by far and should get a convenience method.
                },
                {
                    "publishdate", new ValidatorCollection
                    {
                        new RequiredDateTimeValidator(ErrorSeverity.Degraded),
                        new MaxLengthValidator(ErrorSeverity.Degraded, 200)
                    }
                },
                {
                    "version", new ValidatorCollection
                    {
                        new RequiredDecimalValidator(ErrorSeverity.Degraded),
                        new MaxLengthValidator(ErrorSeverity.Degraded, 20),
                        new MinDecimalValueValidator(ErrorSeverity.Degraded, "0")
                    }
                }
            };

            Navigator.GenerateList("property").ForEach(x => Processors.Add(new PropertyProcessor(x)));
            Navigator.GenerateList("identifier").ForEach(x => Processors.Add(new PropertyProcessor(x)));
            Navigator.GenerateList("scoring").ForEach(x => Processors.Add(new PropertyProcessor(x)));
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
            return validationResults.Values.All(x => x.IsValid);
        }
    }
}