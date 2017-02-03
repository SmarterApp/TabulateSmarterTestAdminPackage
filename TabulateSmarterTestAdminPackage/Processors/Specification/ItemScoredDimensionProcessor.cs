using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.XPath;

namespace TabulateSmarterTestAdminPackage.Processors.Specification
{
    internal class ItemScoredDimensionProcessor : Processor
    {
        private static readonly XPathExpression sXp_MeasurementModel = XPathExpression.Compile("@measurementmodel");
        private static readonly XPathExpression sXp_ScorePoints = XPathExpression.Compile("@scorepoints");
        private static readonly XPathExpression sXp_Weight = XPathExpression.Compile("@weight");

        private readonly XPathNavigator _navigator;

        internal ItemScoredDimensionProcessor(XPathNavigator navigator)
        {
            _navigator = navigator;

            ItemScoreParameterProcessors = new List<ItemScoreParameterProcessor>();
            var itemScoreParameters = navigator.Select("itemscoreparameter");
            foreach (XPathNavigator itemScoreParameter in itemScoreParameters)
            {
                ((IList)ItemScoreParameterProcessors).Add(new PropertyProcessor(itemScoreParameter));
            }
        }

        private IList<ItemScoreParameterProcessor> ItemScoreParameterProcessors { get; }

        public override bool Process()
        {
            throw new NotImplementedException();
        }
    }
}