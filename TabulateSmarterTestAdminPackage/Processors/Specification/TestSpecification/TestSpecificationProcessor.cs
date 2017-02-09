using System.Collections.Generic;
using System.Linq;
using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.Enums;
using TabulateSmarterTestAdminPackage.Common.Processors;
using TabulateSmarterTestAdminPackage.Common.Utilities;
using TabulateSmarterTestAdminPackage.Common.Validators;
using TabulateSmarterTestAdminPackage.Exceptions;
using TabulateSmarterTestAdminPackage.Processors.Specification.TestSpecification.Administration;

namespace TabulateSmarterTestAdminPackage.Processors.Specification.TestSpecification
{
    internal class TestSpecificationProcessor : Processor
    {
        internal static readonly XPathExpression sXp_PackagePurpose = XPathExpression.Compile("@purpose");
        internal static readonly XPathExpression sXp_Publisher = XPathExpression.Compile("@publisher");
        internal static readonly XPathExpression sXp_PublishDate = XPathExpression.Compile("@publishdate");
        internal static readonly XPathExpression sXp_Version = XPathExpression.Compile("@version");

        internal TestSpecificationProcessor(XPathNavigator navigator, PackageType expectedPackageType) : base(navigator)
        {
            IdentifierProcessor = new IdentifierProcessor(navigator.SelectSingleNode("identifier"));

            PropertyProcessors = new List<PropertyProcessor>();
            var properties = navigator.Select("property");
            foreach (XPathNavigator property in properties)
            {
                PropertyProcessors.Add(new PropertyProcessor(property));
            }

            AdministrationProcessor = new AdministrationProcessor(navigator.SelectSingleNode("administration"));

            ExpectedPackageType = expectedPackageType;
        }

        private IdentifierProcessor IdentifierProcessor { get; }
        private IList<PropertyProcessor> PropertyProcessors { get; }
        private AdministrationProcessor AdministrationProcessor { get; }
        private string Purpose { get; set; }
        public string Publisher { get; set; }
        public string PublishDate { get; set; }
        public string Version { get; set; }
        public PackageType ExpectedPackageType { get; set; }

        public override bool Process()
        {
            return IsExpectedPackagePurpose()
                   && IsValidPublisher()
                   && IsValidPublishDate()
                   && IsValidVersion()
                   && IdentifierProcessor.Process()
                   && PropertyProcessors.All(x => x.Process())
                   && AdministrationProcessor.Process();
        }

        // Must match explicit package purpose as defined by enum
        internal bool IsExpectedPackagePurpose()
        {
            var validators = new ValidatorCollection
            {
                new RequiredStringValidator(ErrorSeverity.Degraded),
                new MaxLengthValidator(ErrorSeverity.Degraded, 100),
                new StringMatchValidator(ErrorSeverity.Degraded, ExpectedPackageType.ToString())
            };
            Purpose = Navigator.Eval(sXp_PackagePurpose);
            if (validators.IsValid(Purpose))
            {
                return true;
            }
            throw new IncorrectPackageTypeException(
                $"  Skipping package. Type is '{Purpose}' but processing '{ExpectedPackageType}'.");
        }

        // One or more printable ASCII characters
        internal bool IsValidPublisher()
        {
            var validators = new ValidatorCollection
            {
                new RequiredStringValidator(ErrorSeverity.Degraded),
                new MaxLengthValidator(ErrorSeverity.Degraded, 255)
            };
            Publisher = Navigator.Eval(sXp_Publisher);
            if (validators.IsValid(Publisher))
            {
                return true;
            }
            ReportingUtility.ReportSpecificationError(Navigator.NamespaceURI, sXp_Publisher.Expression,
                validators.GetMessage());
            return false;
        }

        // Valid date and time
        internal bool IsValidPublishDate()
        {
            var validators = new ValidatorCollection
            {
                new RequiredDateTimeValidator(ErrorSeverity.Degraded),
                new MaxLengthValidator(ErrorSeverity.Degraded, 200)
            };
            PublishDate = Navigator.Eval(sXp_PublishDate);
            if (validators.IsValid(PublishDate))
            {
                return true;
            }
            ReportingUtility.ReportSpecificationError(Navigator.NamespaceURI, sXp_PublishDate.Expression,
                validators.GetMessage());
            return false;
        }

        // Version must be a positive decimal number
        internal bool IsValidVersion()
        {
            var validators = new ValidatorCollection
            {
                new RequiredDecimalValidator(ErrorSeverity.Degraded),
                new MaxLengthValidator(ErrorSeverity.Degraded, 20),
                new MinDecimalValueValidator(ErrorSeverity.Degraded, "0")
            };
            Version = Navigator.Eval(sXp_Version);
            if (validators.IsValid(Version))
            {
                return true;
            }
            ReportingUtility.ReportSpecificationError(Navigator.NamespaceURI, sXp_Version.Expression,
                validators.GetMessage());
            return false;
        }
    }
}