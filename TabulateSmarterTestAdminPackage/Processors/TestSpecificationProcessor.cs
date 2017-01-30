using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.Generic;
using TabulateSmarterTestAdminPackage.Exceptions;
using Version = TabulateSmarterTestAdminPackage.Common.AttributeValidation.Version;

namespace TabulateSmarterTestAdminPackage.Processors
{
    internal class TestSpecificationProcessor
    {
        private static readonly XPathExpression sXp_PackagePurpose = XPathExpression.Compile("@purpose");
        private static readonly XPathExpression sXp_Publisher = XPathExpression.Compile("@publisher");
        private static readonly XPathExpression sXp_PublishDate = XPathExpression.Compile("@publishdate");
        private static readonly XPathExpression sXp_Version = XPathExpression.Compile("@version");

        private readonly XPathNavigator navigator;

        internal TestSpecificationProcessor(XPathNavigator navigator)
        {
            this.navigator = navigator;
            identifierProcessor = new IdentifierProcessor(navigator.SelectSingleNode("identifier"));

            propertyProcessors = new List<PropertyProcessor>();
            var properties = navigator.Select("property");
            foreach (XPathNavigator property in properties)
            {
                ((IList)propertyProcessors).Add(new PropertyProcessor(property));
            }
        }

        private IdentifierProcessor identifierProcessor { get; }
        private IEnumerable<PropertyProcessor> propertyProcessors { get; }
        private string purpose { get; set; }
        public string publisher { get; set; }
        public string publishDate { get; set; }
        public string version { get; set; }

        internal bool IsTestSpecificationValid(PackageType expectedPackageType)
        {
            return IsExpectedPackagePurpose(expectedPackageType)
                   && IsValidPublisher()
                   && IsValidPublishDate()
                   && IsValidVersion();
        }

        // Must match explicit package purpose as defined by enum
        internal bool IsExpectedPackagePurpose(PackageType expectedPackageType)
        {
            purpose = navigator.Eval(sXp_PackagePurpose);
            if (purpose.Length < 100
                && string.Equals(purpose, expectedPackageType.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            throw new IncorrectPackageTypeException($"  Skipping package. Type is '{purpose}' but processing '{expectedPackageType}'.");
        }

        // One or more printable ASCII characters
        internal bool IsValidPublisher()
        {
            publisher = navigator.Eval(sXp_Publisher);
            return publisher.NonemptyStringLessThanEqual(255);
        }

        // Valid date and time
        internal bool IsValidPublishDate()
        {
            publishDate = navigator.Eval(sXp_PublishDate);
            var publishDateTime = new DateTime();
            return publishDate.Length < 200
                   && DateTime.TryParse(publishDate, out publishDateTime);
        }

        // Version must be a positive decimal number
        internal bool IsValidVersion()
        {
            version = navigator.Eval(sXp_Version);
            return Version.IsValidVersionDecimal(version);
        }
    }
}