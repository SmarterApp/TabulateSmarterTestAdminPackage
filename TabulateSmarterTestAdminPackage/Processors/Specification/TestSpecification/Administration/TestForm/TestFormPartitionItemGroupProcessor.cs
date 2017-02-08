using System.Linq;
using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.Enums;
using TabulateSmarterTestAdminPackage.Common.Validators;
using TabulateSmarterTestAdminPackage.Processors.Specification.TestSpecification.Administration.AdminSegment;
using TabulateSmarterTestAdminPackage.Processors.Specification.TestSpecification.Administration.ItemPool.TestItem;
using TabulateSmarterTestAdminPackage.Utility;

namespace TabulateSmarterTestAdminPackage.Processors.Specification.TestSpecification.Administration.TestForm
{
    internal class TestFormPartitionItemGroupProcessor : SegmentPoolItemGroupProcessor
    {
        private static readonly XPathExpression sXp_FormPosition = XPathExpression.Compile("@formposition");

        public TestFormPartitionItemGroupProcessor(XPathNavigator navigator) : base(navigator)
        {
            PassageRefProcessor = new PassageRefProcessor(navigator.SelectSingleNode("passagref"));
        }

        private string FormPosition { get; set; }

        private PassageRefProcessor PassageRefProcessor { get; }

        public new bool Process()
        {
            return IsValidMaxItems()
                   && IsValidMaxResponses()
                   && IsValidFormPosition()
                   && ItemGroupIdentifierProcessor.Process()
                   && GroupItemProcessors.All(x => x.Process())
                   && PassageRefProcessor.Process();
        }

        internal bool IsValidFormPosition()
        {
            var validators = new ValidatorCollection
            {
                new RequiredStringValidator(ErrorSeverity.Degraded),
                new MaxLengthValidator(ErrorSeverity.Degraded, 10)
            };
            FormPosition = Navigator.Eval(sXp_FormPosition);
            if (validators.IsValid(FormPosition))
            {
                return true;
            }
            AdminPackageUtility.ReportSpecificationError(Navigator.NamespaceURI, sXp_FormPosition.Expression, validators.GetMessage());
            return false;
        }
    }
}