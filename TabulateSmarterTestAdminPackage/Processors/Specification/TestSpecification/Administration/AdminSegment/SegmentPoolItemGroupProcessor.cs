using System.Collections.Generic;
using System.Linq;
using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.Enums;
using TabulateSmarterTestAdminPackage.Common.Validators;
using TabulateSmarterTestAdminPackage.Utility;

namespace TabulateSmarterTestAdminPackage.Processors.Specification.TestSpecification.Administration.AdminSegment
{
    internal class SegmentPoolItemGroupProcessor : Processor
    {
        internal static readonly XPathExpression sXp_MaxItems = XPathExpression.Compile("@maxitems");
        internal static readonly XPathExpression sXp_MaxResponses = XPathExpression.Compile("@maxresponses");

        private readonly XPathNavigator _navigator;

        internal SegmentPoolItemGroupProcessor(XPathNavigator navigator)
        {
            _navigator = navigator;

            SegmentGroupItemProcessors = new List<SegmentGroupItemProcessor>();
            var groupItems = navigator.Select("groupitem");
            foreach (XPathNavigator groupItem in groupItems)
            {
                SegmentGroupItemProcessors.Add(new SegmentGroupItemProcessor(groupItem));
            }

            SegmentItemGroupIdentifierProcessor = new SegmentItemGroupIdentifierProcessor(navigator.SelectSingleNode("identifier"));
        }

        private SegmentItemGroupIdentifierProcessor SegmentItemGroupIdentifierProcessor { get; }
        private IList<SegmentGroupItemProcessor> SegmentGroupItemProcessors { get; }
        private string MaxItems { get; set; }
        private string MaxResponses { get; set; }

        public override bool Process()
        {
            return IsValidMaxItems()
                   && IsValidMaxResponses()
                   && SegmentItemGroupIdentifierProcessor.Process()
                   && SegmentGroupItemProcessors.All(x => x.Process());
        }

        internal bool IsValidMaxItems()
        {
            var validators = new ValidatorCollection
            {
                new RequiredIntValidator(ErrorSeverity.Degraded),
                new MaxLengthValidator(ErrorSeverity.Degraded, 10),
                new MinIntValueValidator(ErrorSeverity.Degraded, 1)
            };
            MaxItems = _navigator.Eval(sXp_MaxItems);
            if (validators.IsValid(MaxItems))
            {
                return true;
            }
            AdminPackageUtility.ReportSpecificationError(_navigator.NamespaceURI, sXp_MaxItems.Expression, validators.GetMessage());
            return false;
        }

        internal bool IsValidMaxResponses()
        {
            var validators = new ValidatorCollection
            {
                new RequiredIntValidator(ErrorSeverity.Degraded),
                new MaxLengthValidator(ErrorSeverity.Degraded, 10),
                new MinIntValueValidator(ErrorSeverity.Degraded, 1)
            };
            MaxResponses = _navigator.Eval(sXp_MaxResponses);
            if (validators.IsValid(MaxResponses))
            {
                return true;
            }
            AdminPackageUtility.ReportSpecificationError(_navigator.NamespaceURI, sXp_MaxResponses.Expression, validators.GetMessage());
            return false;
        }
    }
}