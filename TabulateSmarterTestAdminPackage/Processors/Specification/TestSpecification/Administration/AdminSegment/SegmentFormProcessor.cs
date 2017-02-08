using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.Enums;
using TabulateSmarterTestAdminPackage.Common.Utilities;
using TabulateSmarterTestAdminPackage.Common.Validators;
using TabulateSmarterTestAdminPackage.Utility;

namespace TabulateSmarterTestAdminPackage.Processors.Specification.TestSpecification.Administration.AdminSegment
{
    internal class SegmentFormProcessor : Processor
    {
        internal static readonly XPathExpression sXp_FormPartitionId = XPathExpression.Compile("@formpartitionid");

        internal SegmentFormProcessor(XPathNavigator navigator) : base(navigator) {}

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
            FormPartitionId = Navigator.Eval(sXp_FormPartitionId);
            if (validators.IsValid(FormPartitionId))
            {
                return true;
            }
            AdminPackageUtility.ReportSpecificationError(Navigator.NamespaceURI, sXp_FormPartitionId.Expression,
                validators.GetMessage());
            return false;
        }
    }
}