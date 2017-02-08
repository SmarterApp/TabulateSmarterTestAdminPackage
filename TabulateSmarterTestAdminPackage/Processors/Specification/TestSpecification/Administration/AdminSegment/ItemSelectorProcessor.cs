using System.Collections.Generic;
using System.Linq;
using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.Enums;
using TabulateSmarterTestAdminPackage.Common.Validators;
using TabulateSmarterTestAdminPackage.Utility;

namespace TabulateSmarterTestAdminPackage.Processors.Specification.TestSpecification.Administration.AdminSegment
{
    internal class ItemSelectorProcessor : Processor
    {
        private static readonly XPathExpression sXp_Type = XPathExpression.Compile("@type");

        private readonly XPathNavigator _navigator;

        internal ItemSelectorProcessor(XPathNavigator navigator)
        {
            _navigator = navigator;

            ItemSelectorIdentifierProcessor = new ItemSelectorIdentifierProcessor(navigator.SelectSingleNode("identifier"));

            ItemSelectionParameterProcessors = new List<ItemSelectionParameterProcessor>();
            var itemSelectionParameters = navigator.Select("itemselectionparameter");
            foreach (XPathNavigator selectionParameter in itemSelectionParameters)
            {
                ItemSelectionParameterProcessors.Add(new ItemSelectionParameterProcessor(selectionParameter));
            }
        }

        private string Type { get; set; }
        private ItemSelectorIdentifierProcessor ItemSelectorIdentifierProcessor { get; }
        private IList<ItemSelectionParameterProcessor> ItemSelectionParameterProcessors { get; }

        public override bool Process()
        {
            return IsValidType()
                   && ItemSelectorIdentifierProcessor.Process()
                   && ItemSelectionParameterProcessors.All(x => x.Process());
        }

        internal bool IsValidType()
        {
            var validators = new ValidatorCollection
            {
                new RequiredStringValidator(ErrorSeverity.Degraded),
                new MaxLengthValidator(ErrorSeverity.Degraded, 100)
            };
            Type = _navigator.Eval(sXp_Type);
            if (validators.IsValid(Type))
            {
                return true;
            }

            AdminPackageUtility.ReportSpecificationError(_navigator.NamespaceURI, sXp_Type.Expression, validators.GetMessage());
            return false;
        }
    }
}