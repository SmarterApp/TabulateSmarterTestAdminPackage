using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.Enums;
using TabulateSmarterTestAdminPackage.Common.Validators;
using TabulateSmarterTestAdminPackage.Exceptions;
using TabulateSmarterTestAdminPackage.Utility;

namespace TabulateSmarterTestAdminPackage.Processors.Specification
{
    internal class TestSpecificationProcessor : Processor
    {
        internal static readonly XPathExpression sXp_PackagePurpose = XPathExpression.Compile("@purpose");
        internal static readonly XPathExpression sXp_Publisher = XPathExpression.Compile("@publisher");
        internal static readonly XPathExpression sXp_PublishDate = XPathExpression.Compile("@publishdate");
        internal static readonly XPathExpression sXp_Version = XPathExpression.Compile("@version");

        internal readonly XPathNavigator Navigator;

        internal TestSpecificationProcessor(XPathNavigator navigator, PackageType expectedPackageType)
        {
            Navigator = navigator;
            IdentifierProcessor = new IdentifierProcessor(navigator.SelectSingleNode("identifier"));

            PropertyProcessors = new List<PropertyProcessor>();
            var properties = navigator.Select("property");
            foreach (XPathNavigator property in properties)
            {
                ((IList)PropertyProcessors).Add(new PropertyProcessor(property));
            }
            ExpectedPackageType = expectedPackageType;
        }

        private IdentifierProcessor IdentifierProcessor { get; }
        private IEnumerable<PropertyProcessor> PropertyProcessors { get; }
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
                   && PropertyProcessors.All(x => x.Process());
        }

        // Must match explicit package purpose as defined by enum
        internal bool IsExpectedPackagePurpose()
        {
            Purpose = Navigator.Eval(sXp_PackagePurpose);
            if (Purpose.Length < 100
                && string.Equals(Purpose, ExpectedPackageType.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            throw new IncorrectPackageTypeException($"  Skipping package. Type is '{Purpose}' but processing '{ExpectedPackageType}'.");
        }

        // One or more printable ASCII characters
        internal bool IsValidPublisher()
        {
            var validators = new ValidatorCollection(new List<Validator>
            {
                new RequiredStringValidator(ErrorSeverity.Degraded, null),
                new MaxLengthValidator(ErrorSeverity.Degraded, 255)
            });
            Publisher = Navigator.Eval(sXp_Publisher);
            if (validators.ObjectPassesValidation(Publisher))
            {
                return true;
            }
            AdminPackageUtility.ReportSpecificationError(Navigator.NamespaceURI, sXp_Publisher.Expression, validators.ObjectValidationErrors(Publisher));
            return false;
        }

        // Valid date and time
        internal bool IsValidPublishDate()
        {
            var validators = new ValidatorCollection(new List<Validator>
            {
                new RequiredDateTimeValidator(ErrorSeverity.Degraded, null),
                new MaxLengthValidator(ErrorSeverity.Degraded, 200)
            });
            PublishDate = Navigator.Eval(sXp_PublishDate);
            if (validators.ObjectPassesValidation(PublishDate))
            {
                return true;
            }
            AdminPackageUtility.ReportSpecificationError(Navigator.NamespaceURI, sXp_PublishDate.Expression, validators.ObjectValidationErrors(PublishDate));
            return false;
        }

        // Version must be a positive decimal number
        internal bool IsValidVersion()
        {
            var validators = new ValidatorCollection(new List<Validator>
            {
                new RequiredDecimalValidator(ErrorSeverity.Degraded, null),
                new MaxLengthValidator(ErrorSeverity.Degraded, 10),
                new MinDecimalValueValidator(ErrorSeverity.Degraded, 0)
            });
            Version = Navigator.Eval(sXp_Version);
            if (validators.ObjectPassesValidation(Version))
            {
                return true;
            }
            AdminPackageUtility.ReportSpecificationError(Navigator.NamespaceURI, sXp_Version.Expression, validators.ObjectValidationErrors(Version));
            return false;
        }
    }
}