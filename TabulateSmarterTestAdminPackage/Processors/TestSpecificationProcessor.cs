using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.AttributeValidation;
using TabulateSmarterTestAdminPackage.Common.Generic;
using TabulateSmarterTestAdminPackage.Exceptions;
using TabulateSmarterTestAdminPackage.Utility;

namespace TabulateSmarterTestAdminPackage.Processors
{
    internal class TestSpecificationProcessor
    {
        private static readonly XPathExpression sXp_PackagePurpose = XPathExpression.Compile("@purpose");
        private static readonly XPathExpression sXp_Publisher = XPathExpression.Compile("@publisher");
        private static readonly XPathExpression sXp_PublishDate = XPathExpression.Compile("@publishdate");
        private static readonly XPathExpression sXp_Version = XPathExpression.Compile("@version");

        private readonly XPathNavigator _navigator;

        internal TestSpecificationProcessor(XPathNavigator navigator)
        {
            _navigator = navigator;
            IdentifierProcessor = new IdentifierProcessor(navigator.SelectSingleNode("identifier"));

            PropertyProcessors = new List<PropertyProcessor>();
            var properties = navigator.Select("property");
            foreach (XPathNavigator property in properties)
            {
                ((IList)PropertyProcessors).Add(new PropertyProcessor(property));
            }
        }

        private IdentifierProcessor IdentifierProcessor { get; }
        private IEnumerable<PropertyProcessor> PropertyProcessors { get; }
        private string Purpose { get; set; }
        public string Publisher { get; set; }
        public string PublishDate { get; set; }
        public string Version { get; set; }

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
            Purpose = _navigator.Eval(sXp_PackagePurpose);
            if (Purpose.Length < 100
                && string.Equals(Purpose, expectedPackageType.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            throw new IncorrectPackageTypeException($"  Skipping package. Type is '{Purpose}' but processing '{expectedPackageType}'.");
        }

        // One or more printable ASCII characters
        internal bool IsValidPublisher()
        {
            Publisher = _navigator.Eval(sXp_Publisher);
            if (Publisher.NonemptyStringLessThanEqual(255))
            {
                return true;
            }
            AdminPackageUtility.ReportSpecificationError(_navigator.NamespaceURI, sXp_Publisher.Expression, "string required [length<=255]");
            return false;
        }

        // Valid date and time
        internal bool IsValidPublishDate()
        {
            PublishDate = _navigator.Eval(sXp_PublishDate);
            var publishDateTime = new DateTime();
            if (PublishDate.Length < 200
                && DateTime.TryParse(PublishDate, out publishDateTime))
            {
                return true;
            }
            AdminPackageUtility.ReportSpecificationError(_navigator.NamespaceURI, sXp_PublishDate.Expression, "datetime required [length<=200]");
            return false;
        }

        // Version must be a positive decimal number
        internal bool IsValidVersion()
        {
            Version = _navigator.Eval(sXp_Version);
            if (VersionValidation.IsValidVersionDecimal(Version))
            {
                return true;
            }
            AdminPackageUtility.ReportSpecificationError(_navigator.NamespaceURI, sXp_Version.Expression, "decimal required [positive][length<=20]");
            return false;
        }
    }
}