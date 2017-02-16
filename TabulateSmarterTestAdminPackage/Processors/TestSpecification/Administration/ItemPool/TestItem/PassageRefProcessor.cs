using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.Enums;
using TabulateSmarterTestAdminPackage.Common.Processors;
using TabulateSmarterTestAdminPackage.Common.Utilities;
using TabulateSmarterTestAdminPackage.Common.Validators.Convenience;

namespace TabulateSmarterTestPackage.Processors.TestSpecification.Administration.ItemPool.TestItem
{
    public class PassageRefProcessor : Processor
    {
        public PassageRefProcessor(XPathNavigator navigator, PackageType packageType) : base(navigator, packageType) {}

        // This processor is a special case because the value is in the element instead of an attribute
        public new bool Process()
        {
            var validators = StringValidator.IsValidOptionalNonEmptyWithLength(100);
            var passageref = Navigator.Value;

            ValidatedAttributes.Add("passageref", new ValidatedAttribute
            {
                IsValid = validators.IsValid(passageref),
                Name = "passageref",
                Value = Navigator.Value,
                Validator = validators
            });

            if (ValidatedAttributes["passageref"].IsValid)
            {
                return true;
            }

            ReportingUtility.ReportSpecificationError(Navigator.NamespaceURI, Navigator.BaseURI,
                validators.GetMessage());
            return false;
        }
    }
}