using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.Enums;
using TabulateSmarterTestAdminPackage.Common.Validators;
using TabulateSmarterTestAdminPackage.Utility;

namespace TabulateSmarterTestAdminPackage.Processors.Specification.TestSpecification.Administration.AdminSegment
{
    internal class ItemSelectionParameterProcessor : Processor
    {
        private static readonly XPathExpression sXp_BpElementId = XPathExpression.Compile("@bpelementid");

        private readonly XPathNavigator _navigator;

        internal ItemSelectionParameterProcessor(XPathNavigator navigator)
        {
            _navigator = navigator;

            ItemSelectionParameterPropertyProcessors = new List<ItemSelectionParameterPropertyProcessor>();
            var properties = navigator.Select("property");
            foreach (XPathNavigator property in properties)
            {
                ((IList)ItemSelectionParameterPropertyProcessors).Add(new ItemSelectionParameterPropertyProcessor(property));
            }
        }

        private string BpElementId { get; set; }

        private List<ItemSelectionParameterPropertyProcessor> ItemSelectionParameterPropertyProcessors { get; }

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