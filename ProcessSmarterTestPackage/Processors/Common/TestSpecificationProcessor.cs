using System.Collections.Generic;
using System.Linq;
using System.Xml.XPath;
using NLog;
using ProcessSmarterTestPackage.PostProcessors;
using ProcessSmarterTestPackage.Processors.Administration;
using ProcessSmarterTestPackage.Processors.Combined;
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
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

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

            
            switch (packageType)
            {
                case PackageType.Administration:
                    Navigator.GenerateList("property").ForEach(x => Processors.Add(new PropertyProcessor(x, packageType)));
                    Navigator.GenerateList("identifier").ForEach(x => Processors.Add(new IdentifierProcessor(x, packageType)));
                    Navigator.GenerateList("administration")
                        .ForEach(x => Processors.Add(new AdministrationProcessor(x, packageType)));
                    break;
                case PackageType.Scoring:
                    Navigator.GenerateList("property").ForEach(x => Processors.Add(new PropertyProcessor(x, packageType)));
                    Navigator.GenerateList("identifier").ForEach(x => Processors.Add(new IdentifierProcessor(x, packageType)));
                    Navigator.GenerateList("scoring")
                        .ForEach(x => Processors.Add(new ScoringProcessor(x, packageType)));
                    break;
                case PackageType.Combined:
                    Logger.Debug("ADDING CombinedTestProcessor to Processors");
                    Processors.Add(new CombinedTestProcessor(Navigator, packageType));
                    break;
            }
        }

        public override List<ValidationError> AdditionalValidations()
        {
            return new TestSpecificationPostProcessor(PackageType, this).GenerateErrors().ToList();
        }

    }
}