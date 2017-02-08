using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.Enums;
using TabulateSmarterTestAdminPackage.Common.Validators;
using TabulateSmarterTestAdminPackage.Utility;

namespace TabulateSmarterTestAdminPackage.Processors.Specification.TestSpecification.Administration.AdminSegment
{
    internal class SegmentFormProcessor : Processor
    {
        internal static readonly XPathExpression sXp_FormPartitionId = XPathExpression.Compile("@formpartitionid");

        private readonly XPathNavigator _navigator;

        internal SegmentFormProcessor(XPathNavigator navigator)
        {
            _navigator = navigator;
        }

        private string FormPartitionId { get; set; }

        public override bool Process()
        {
            return IsValidFormPartitionId();
        }

        internal bool IsValidFormPartitionId()
        {
            var validators = new ValidatorCollection
            {
                new RequiredStringValidator(ErrorSeverity.Degraded),
                new MaxLengthValidator(ErrorSeverity.Degraded, 100)
            };
            FormPartitionId = _navigator.Eval(sXp_FormPartitionId);
            if (validators.IsValid(FormPartitionId))
            {
                return true;
            }
            AdminPackageUtility.ReportSpecificationError(_navigator.NamespaceURI, sXp_FormPartitionId.Expression, validators.GetMessage());
            return false;
        }
    }
}