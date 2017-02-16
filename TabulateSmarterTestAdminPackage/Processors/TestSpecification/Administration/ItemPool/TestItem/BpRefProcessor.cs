using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.Enums;
using TabulateSmarterTestAdminPackage.Common.Processors;
using TabulateSmarterTestAdminPackage.Common.Utilities;
using TabulateSmarterTestAdminPackage.Common.Validators.Convenience;

namespace TabulateSmarterTestPackage.Processors.TestSpecification.Administration.ItemPool.TestItem
{
    public class BpRefProcessor : Processor
    {
        public BpRefProcessor(XPathNavigator navigator, PackageType packageType) : base(navigator, packageType) {}

        // This processor is a special case because the value is in the element instead of an attribute
        public new bool Process()
        {
            var validators = StringValidator.IsValidNonEmptyWithLength(150);
            var bpref = Navigator.Value;

            ValidatedAttributes.Add("bpref", new ValidatedAttribute
            {
                IsValid = validators.IsValid(bpref),
                Name = "bpref",
                Value = Navigator.Value,
                Validator = validators
            });

            if (ValidatedAttributes["bpref"].IsValid)
            {
                return true;
            }

            ReportingUtility.ReportSpecificationError(Navigator.NamespaceURI, Navigator.BaseURI,
                validators.GetMessage());
            return false;
        }
    }
}