using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.Enums;
using TabulateSmarterTestAdminPackage.Common.Processors;
using TabulateSmarterTestAdminPackage.Common.Utilities;
using TabulateSmarterTestAdminPackage.Common.Validators;

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

            ReportingUtility.ReportSpecificationError(Navigator.NamespaceURI, Navigator.BaseURI,
                validators.GetMessage());
            return false;
        }
    }
}