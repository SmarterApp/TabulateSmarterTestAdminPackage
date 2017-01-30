using System;
using System.Linq;
using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Exceptions;

namespace TabulateSmarterTestAdminPackage.Processors
{
    internal class TestSpecificationProcessor
    {
        private static readonly XPathExpression sXp_PackagePurpose = XPathExpression.Compile("/testspecification/@purpose");
        private static readonly XPathExpression sXp_Publisher = XPathExpression.Compile("/testspecification/@publisher");
        private static readonly XPathExpression sXp_PublishDate = XPathExpression.Compile("/testspecification/@publishdate");
        private static readonly XPathExpression sXp_Version = XPathExpression.Compile("/testspecification/@version");

        internal TestSpecificationProcessor(XPathNavigator navigator)
        {
            this.navigator = navigator;
        }

        private XPathNavigator navigator { get; }
        internal string PackagePurpose { get; set; }

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
            var purpose = navigator.Eval(sXp_PackagePurpose);
            if (string.Equals(purpose, expectedPackageType.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            throw new IncorrectPackageTypeException($"  Skipping package. Type is '{purpose}' but processing '{expectedPackageType}'.");
        }

        // One or more printable ASCII characters
        internal bool IsValidPublisher()
        {
            var publisher = navigator.Eval(sXp_Publisher);
            return publisher.Any();
        }

        // Valid date and time
        internal bool IsValidPublishDate()
        {
            var publishDateString = navigator.Eval(sXp_PublishDate);
            var publishDate = new DateTime();
            return DateTime.TryParse(publishDateString, out publishDate);
        }

        // Version must be a positive decimal number
        internal bool IsValidVersion()
        {
            var versionString = navigator.Eval(sXp_Version);
            decimal version;
            return decimal.TryParse(versionString, out version) && version >= 0;
        }
    }
}