using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.Enums;
using TabulateSmarterTestAdminPackage.Common.Validators;
using TabulateSmarterTestAdminPackage.Utility;

namespace TabulateSmarterTestAdminPackage.Processors.Specification.TestSpecification.Administration.ItemPool.TestItem
{
    internal class BPrefProcessor : Processor
    {
        private readonly XPathNavigator _navigator;

        internal BPrefProcessor(XPathNavigator navigator)
        {
            _navigator = navigator;
        }

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
            BPref = _navigator.Value;
            if (validators.IsValid(BPref))
            {
                return true;
            }

            AdminPackageUtility.ReportSpecificationError(_navigator.NamespaceURI, _navigator.BaseURI, validators.GetMessage());
            return false;
        }
    }
}