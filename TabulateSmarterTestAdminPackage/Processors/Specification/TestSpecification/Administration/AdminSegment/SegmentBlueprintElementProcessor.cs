using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.Enums;
using TabulateSmarterTestAdminPackage.Common.Processors;
using TabulateSmarterTestAdminPackage.Common.Utilities;
using TabulateSmarterTestAdminPackage.Common.Validators;

namespace TabulateSmarterTestAdminPackage.Processors.Specification.TestSpecification.Administration.AdminSegment
{
    internal class SegmentBlueprintElementProcessor : Processor
    {
        internal static readonly XPathExpression sXp_MinOpItems = XPathExpression.Compile("@minopitems");
        internal static readonly XPathExpression sXp_MaxOpItems = XPathExpression.Compile("@maxopitems");
        internal static readonly XPathExpression sXp_BpElementId = XPathExpression.Compile("@bpelementid");

        internal SegmentBlueprintElementProcessor(XPathNavigator navigator) : base(navigator) {}

        private string MinOpItems { get; set; }
        private string MaxOpItems { get; set; }
        private string BpElementId { get; set; }

        public override bool Process()
        {
            return IsValidMinOpItems()
                   && IsValidMaxOpItems()
                   && IsValidBpElementId();
        }

        internal bool IsValidMinOpItems()
        {
            var validators = new ValidatorCollection
            {
                new RequiredIntValidator(ErrorSeverity.Degraded),
                new MaxLengthValidator(ErrorSeverity.Degraded, 10),
                new MinIntValueValidator(ErrorSeverity.Degraded, 0)
            };
            MinOpItems = Navigator.Eval(sXp_MinOpItems);
            if (validators.IsValid(MinOpItems))
            {
                return true;
            }

            ReportingUtility.ReportSpecificationError(Navigator.NamespaceURI, sXp_MinOpItems.Expression,
                validators.GetMessage());
            return false;
        }

        internal bool IsValidMaxOpItems()
        {
            var validators = new ValidatorCollection
            {
                new RequiredIntValidator(ErrorSeverity.Degraded),
                new MaxLengthValidator(ErrorSeverity.Degraded, 10),
                new MinIntValueValidator(ErrorSeverity.Degraded, 1)
            };
            MaxOpItems = Navigator.Eval(sXp_MaxOpItems);
            if (validators.IsValid(MaxOpItems))
            {
                return true;
            }

            ReportingUtility.ReportSpecificationError(Navigator.NamespaceURI, sXp_MaxOpItems.Expression,
                validators.GetMessage());
            return false;
        }

        internal bool IsValidBpElementId()
        {
            var validators = new ValidatorCollection
            {
                new RequiredStringValidator(ErrorSeverity.Degraded),
                new MaxLengthValidator(ErrorSeverity.Degraded, 150)
            };
            BpElementId = Navigator.Eval(sXp_BpElementId);
            if (validators.IsValid(BpElementId))
            {
                return true;
            }
            ReportingUtility.ReportSpecificationError(Navigator.NamespaceURI, sXp_BpElementId.Expression,
                validators.GetMessage());
            return false;
        }
    }
}