using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.Enums;
using TabulateSmarterTestAdminPackage.Common.Validators;
using TabulateSmarterTestAdminPackage.Utility;

namespace TabulateSmarterTestAdminPackage.Processors.Specification.TestSpecification.Administration.ItemPool.TestItem
{
    internal class BPrefProcessor : Processor
    {
        internal BPrefProcessor(XPathNavigator navigator) : base(navigator) {}

        private string BPref { get; set; }

        public override bool Process()
        {
            return IsValidBPref();
        }

        internal bool IsValidBPref()
        {
            var validators = new ValidatorCollection
            {
                new RequiredStringValidator(ErrorSeverity.Degraded),
                new MaxLengthValidator(ErrorSeverity.Degraded, 150)
            };
            BPref = Navigator.Value;
            if (validators.IsValid(BPref))
            {
                return true;
            }

            AdminPackageUtility.ReportSpecificationError(Navigator.NamespaceURI, Navigator.BaseURI,
                validators.GetMessage());
            return false;
        }
    }
}