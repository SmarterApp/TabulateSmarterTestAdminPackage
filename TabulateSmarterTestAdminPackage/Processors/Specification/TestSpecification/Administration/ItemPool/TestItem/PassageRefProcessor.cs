using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.Enums;
using TabulateSmarterTestAdminPackage.Common.Validators;
using TabulateSmarterTestAdminPackage.Utility;

namespace TabulateSmarterTestAdminPackage.Processors.Specification.TestSpecification.Administration.ItemPool.TestItem
{
    internal class PassageRefProcessor : Processor
    {
        internal PassageRefProcessor(XPathNavigator navigator) : base(navigator) {}

        private string PassageRef { get; set; }

        public override bool Process()
        {
            return IsValidPassageRef();
        }

        // This is not required, but if the element is there, it should have a value
        internal bool IsValidPassageRef()
        {
            var validators = new ValidatorCollection
            {
                new RequiredStringValidator(ErrorSeverity.Benign),
                new MaxLengthValidator(ErrorSeverity.Benign, 100)
            };
            PassageRef = Navigator.Value;
            if (validators.IsValid(PassageRef))
            {
                return true;
            }

            AdminPackageUtility.ReportSpecificationError(Navigator.NamespaceURI, Navigator.BaseURI,
                validators.GetMessage());
            return false;
        }
    }
}