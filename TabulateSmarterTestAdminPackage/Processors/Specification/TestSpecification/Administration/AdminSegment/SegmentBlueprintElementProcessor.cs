using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.Enums;
using TabulateSmarterTestAdminPackage.Common.Validators;
using TabulateSmarterTestAdminPackage.Utility;

namespace TabulateSmarterTestAdminPackage.Processors.Specification.TestSpecification.Administration.AdminSegment
{
    internal class SegmentBlueprintElementProcessor : Processor
    {
        internal static readonly XPathExpression sXp_MinOpItems = XPathExpression.Compile("@minopitems");
        internal static readonly XPathExpression sXp_MaxOpItems = XPathExpression.Compile("@maxopitems");
        internal static readonly XPathExpression sXp_BpElementId = XPathExpression.Compile("@bpelementid");

        private readonly XPathNavigator _navigator;

        internal SegmentBlueprintElementProcessor(XPathNavigator navigator)
        {
            _navigator = navigator;
        }

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
            MinOpItems = _navigator.Eval(sXp_MinOpItems);
            if (validators.IsValid(MinOpItems))
            {
                return true;
            }

            AdminPackageUtility.ReportSpecificationError(_navigator.NamespaceURI, sXp_MinOpItems.Expression, validators.GetMessage());
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
            MaxOpItems = _navigator.Eval(sXp_MaxOpItems);
            if (validators.IsValid(MaxOpItems))
            {
                return true;
            }

            AdminPackageUtility.ReportSpecificationError(_navigator.NamespaceURI, sXp_MaxOpItems.Expression, validators.GetMessage());
            return false;
        }

        internal bool IsValidBpElementId()
        {
            var validators = new ValidatorCollection
            {
                new RequiredStringValidator(ErrorSeverity.Degraded),
                new MaxLengthValidator(ErrorSeverity.Degraded, 150)
            };
            BpElementId = _navigator.Eval(sXp_BpElementId);
            if (validators.IsValid(BpElementId))
            {
                return true;
            }
            AdminPackageUtility.ReportSpecificationError(_navigator.NamespaceURI, sXp_BpElementId.Expression, validators.GetMessage());
            return false;
        }
    }
}