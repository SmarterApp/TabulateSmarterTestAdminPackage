using System.Collections.Generic;
using System.Linq;
using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.Enums;
using TabulateSmarterTestAdminPackage.Common.Processors;
using TabulateSmarterTestAdminPackage.Common.Utilities;
using TabulateSmarterTestAdminPackage.Common.Validators;

namespace TabulateSmarterTestAdminPackage.Processors.Specification.TestSpecification.Administration.AdminSegment
{
    internal class SegmentPoolItemGroupProcessor : Processor
    {
        internal static readonly XPathExpression sXp_MaxItems = XPathExpression.Compile("@maxitems");
        internal static readonly XPathExpression sXp_MaxResponses = XPathExpression.Compile("@maxresponses");

        internal SegmentPoolItemGroupProcessor(XPathNavigator navigator) : base(navigator)
        {
            GroupItemProcessors = new List<GroupItemProcessor>();
            var groupItems = navigator.Select("groupitem");
            foreach (XPathNavigator groupItem in groupItems)
            {
                GroupItemProcessors.Add(new GroupItemProcessor(groupItem));
            }

            ItemGroupIdentifierProcessor = new ItemGroupIdentifierProcessor(navigator.SelectSingleNode("identifier"));
        }

        internal ItemGroupIdentifierProcessor ItemGroupIdentifierProcessor { get; }
        internal IList<GroupItemProcessor> GroupItemProcessors { get; }
        internal string MaxItems { get; set; }
        internal string MaxResponses { get; set; }

        public override bool Process()
        {
            return IsValidMaxItems()
                   && IsValidMaxResponses()
                   && ItemGroupIdentifierProcessor.Process()
                   && GroupItemProcessors.All(x => x.Process());
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
            ReportingUtility.ReportSpecificationError(Navigator.NamespaceURI, sXp_MaxItems.Expression,
                validators.GetMessage());
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
            ReportingUtility.ReportSpecificationError(Navigator.NamespaceURI, sXp_MaxResponses.Expression,
                validators.GetMessage());
            return false;
        }
    }
}