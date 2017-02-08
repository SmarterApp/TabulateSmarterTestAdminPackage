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

        internal readonly XPathNavigator Navigator;

        internal SegmentPoolItemGroupProcessor(XPathNavigator navigator)
        {
            Navigator = navigator;

            SegmentGroupItemProcessors = new List<SegmentGroupItemProcessor>();
            var groupItems = navigator.Select("groupitem");
            foreach (XPathNavigator groupItem in groupItems)
            {
                SegmentGroupItemProcessors.Add(new SegmentGroupItemProcessor(groupItem));
            }

            SegmentItemGroupIdentifierProcessor = new ItemGroupIdentifierProcessor(navigator.SelectSingleNode("identifier"));
        }

        private ItemGroupIdentifierProcessor SegmentItemGroupIdentifierProcessor { get; }
        private IList<SegmentGroupItemProcessor> SegmentGroupItemProcessors { get; }
        internal string MaxItems { get; set; }
        internal string MaxResponses { get; set; }

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
                new RequiredStringValidator(ErrorSeverity.Degraded),
                new MaxLengthValidator(ErrorSeverity.Degraded, 10)
            };
            MaxItems = Navigator.Eval(sXp_MaxItems);
            if (validators.IsValid(MaxItems))
            {
                return true;
            }
            AdminPackageUtility.ReportSpecificationError(Navigator.NamespaceURI, sXp_MaxItems.Expression, validators.GetMessage());
            return false;
        }

        internal bool IsValidMaxResponses()
        {
            var validators = new ValidatorCollection
            {
                new RequiredStringValidator(ErrorSeverity.Degraded),
                new MaxLengthValidator(ErrorSeverity.Degraded, 10)
            };
            MaxResponses = Navigator.Eval(sXp_MaxResponses);
            if (validators.IsValid(MaxResponses))
            {
                return true;
            }
            AdminPackageUtility.ReportSpecificationError(Navigator.NamespaceURI, sXp_MaxResponses.Expression, validators.GetMessage());
            return false;
        }
    }
}