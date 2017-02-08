using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.Enums;
using TabulateSmarterTestAdminPackage.Common.Utilities;
using TabulateSmarterTestAdminPackage.Common.Validators;
using TabulateSmarterTestAdminPackage.Utility;

namespace TabulateSmarterTestAdminPackage.Processors.Specification.TestSpecification.Administration.AdminSegment
{
    internal class AdminSegmentProcessor : Processor
    {
        private static readonly XPathExpression sXp_SegmentId = XPathExpression.Compile("@segmentid");
        private static readonly XPathExpression sXp_Position = XPathExpression.Compile("@position");
        private static readonly XPathExpression sXp_ItemSelection = XPathExpression.Compile("@itemselection");

        internal AdminSegmentProcessor(XPathNavigator navigator) : base(navigator)
        {
            SegmentBlueprintProcessor = new SegmentBlueprintProcessor(navigator.SelectSingleNode("segmentblueprint"));

            ItemSelectorProcessor = new ItemSelectorProcessor(navigator.SelectSingleNode("itemselector"));

            SegmentPoolProcessor = new SegmentPoolProcessor(navigator.SelectSingleNode("segmentpool"));

            SegmentFormProcessor = new SegmentFormProcessor(navigator.SelectSingleNode("segmentform"));
        }

        private string SegmentId { get; set; }
        private string Position { get; set; }
        private string ItemSelection { get; set; }

        private SegmentBlueprintProcessor SegmentBlueprintProcessor { get; }
        private ItemSelectorProcessor ItemSelectorProcessor { get; }
        private SegmentPoolProcessor SegmentPoolProcessor { get; }
        private SegmentFormProcessor SegmentFormProcessor { get; }

        public override bool Process()
        {
            return IsValidSegmentId()
                   && IsValidPosition()
                   && IsValidItemSelection()
                   && SegmentBlueprintProcessor.Process()
                   && ItemSelectorProcessor.Process()
                   && SegmentPoolProcessor.Process()
                   && SegmentFormProcessor.Process();
        }

        internal bool IsValidSegmentId()
        {
            var validators = new ValidatorCollection
            {
                new RequiredStringValidator(ErrorSeverity.Degraded),
                new MaxLengthValidator(ErrorSeverity.Degraded, 250)
            };
            SegmentId = Navigator.Eval(sXp_SegmentId);
            if (validators.IsValid(SegmentId))
            {
                return true;
            }
            AdminPackageUtility.ReportSpecificationError(Navigator.NamespaceURI, sXp_SegmentId.Expression,
                validators.GetMessage());
            return false;
        }

        internal bool IsValidPosition()
        {
            var validators = new ValidatorCollection
            {
                new RequiredIntValidator(ErrorSeverity.Degraded),
                new MaxLengthValidator(ErrorSeverity.Degraded, 10),
                new MinIntValueValidator(ErrorSeverity.Degraded, 1)
            };
            Position = Navigator.Eval(sXp_Position);
            if (validators.IsValid(Position))
            {
                return true;
            }
            AdminPackageUtility.ReportSpecificationError(Navigator.NamespaceURI, sXp_Position.Expression,
                validators.GetMessage());
            return false;
        }

        //TODO: enum
        internal bool IsValidItemSelection()
        {
            var validators = new ValidatorCollection
            {
                new RequiredStringValidator(ErrorSeverity.Degraded),
                new MaxLengthValidator(ErrorSeverity.Degraded, 100)
            };
            ItemSelection = Navigator.Eval(sXp_ItemSelection);
            if (validators.IsValid(ItemSelection))
            {
                return true;
            }
            AdminPackageUtility.ReportSpecificationError(Navigator.NamespaceURI, sXp_ItemSelection.Expression,
                validators.GetMessage());
            return false;
        }
    }
}