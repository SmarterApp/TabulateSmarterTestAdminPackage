using System.Collections.Generic;
using System.Linq;
using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.Enums;
using TabulateSmarterTestAdminPackage.Common.Processors;
using TabulateSmarterTestAdminPackage.Common.Utilities;
using TabulateSmarterTestAdminPackage.Common.Validators;

namespace TabulateSmarterTestAdminPackage.Processors.Specification.TestSpecification.Administration.AdminSegment
{
    internal class ItemSelectionParameterProcessor : Processor
    {
        private static readonly XPathExpression sXp_BpElementId = XPathExpression.Compile("@bpelementid");

        internal ItemSelectionParameterProcessor(XPathNavigator navigator) : base(navigator)
        {
            ItemSelectionParameterPropertyProcessors = new List<ItemSelectionParameterPropertyProcessor>();
            var properties = navigator.Select("property");
            foreach (XPathNavigator property in properties)
            {
                ItemSelectionParameterPropertyProcessors.Add(new ItemSelectionParameterPropertyProcessor(property));
            }
        }

        private string BpElementId { get; set; }

        private IList<ItemSelectionParameterPropertyProcessor> ItemSelectionParameterPropertyProcessors { get; }

        public override bool Process()
        {
            return IsValidBpElementId()
                   && ItemSelectionParameterPropertyProcessors.All(x => x.Process());
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