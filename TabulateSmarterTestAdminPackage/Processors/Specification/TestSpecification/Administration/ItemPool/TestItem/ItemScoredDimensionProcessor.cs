using System.Collections.Generic;
using System.Linq;
using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.Enums;
using TabulateSmarterTestAdminPackage.Common.Processors;
using TabulateSmarterTestAdminPackage.Common.Utilities;
using TabulateSmarterTestAdminPackage.Common.Validators;

namespace TabulateSmarterTestAdminPackage.Processors.Specification.TestSpecification.Administration.ItemPool.TestItem
{
    internal class ItemScoredDimensionProcessor : Processor
    {
        private static readonly XPathExpression sXp_MeasurementModel = XPathExpression.Compile("@measurementmodel");
        private static readonly XPathExpression sXp_ScorePoints = XPathExpression.Compile("@scorepoints");
        private static readonly XPathExpression sXp_Weight = XPathExpression.Compile("@weight");
        private static readonly XPathExpression sXp_Dimension = XPathExpression.Compile("@dimension");

        internal ItemScoredDimensionProcessor(XPathNavigator navigator) : base(navigator)
        {
            ItemScoreParameterProcessors = new List<ItemScoreParameterProcessor>();
            var itemScoreParameters = navigator.Select("itemscoreparameter");
            foreach (XPathNavigator itemScoreParameter in itemScoreParameters)
            {
                ItemScoreParameterProcessors.Add(new ItemScoreParameterProcessor(itemScoreParameter));
            }
        }

        private IList<ItemScoreParameterProcessor> ItemScoreParameterProcessors { get; }
        private string MeasurementModel { get; set; }
        private string ScorePoints { get; set; }
        private string Weight { get; set; }
        private string Dimension { get; set; }

        public override bool Process()
        {
            return IsValidMeasurementModel()
                   && IsValidScorePoints()
                   && IsValidWeight()
                   && IsValidDimension()
                   && ItemScoreParameterProcessors.All(x => x.Process());
        }

        // TODO: enum
        internal bool IsValidMeasurementModel()
        {
            var validators = new ValidatorCollection
            {
                new RequiredStringValidator(ErrorSeverity.Degraded),
                new MaxLengthValidator(ErrorSeverity.Degraded, 100)
            };
            MeasurementModel = Navigator.Eval(sXp_MeasurementModel);
            if (validators.IsValid(MeasurementModel))
            {
                return true;
            }

            ReportingUtility.ReportSpecificationError(Navigator.NamespaceURI, sXp_MeasurementModel.Expression,
                validators.GetMessage());
            return false;
        }

        internal bool IsValidScorePoints()
        {
            var validators = new ValidatorCollection
            {
                new RequiredIntValidator(ErrorSeverity.Degraded),
                new MaxLengthValidator(ErrorSeverity.Degraded, 10),
                new MinIntValueValidator(ErrorSeverity.Degraded, 0)
            };
            ScorePoints = Navigator.Eval(sXp_ScorePoints);
            if (validators.IsValid(ScorePoints))
            {
                return true;
            }

            ReportingUtility.ReportSpecificationError(Navigator.NamespaceURI, sXp_ScorePoints.Expression,
                validators.GetMessage());
            return false;
        }

        internal bool IsValidWeight()
        {
            var validators = new ValidatorCollection
            {
                new RequiredDecimalValidator(ErrorSeverity.Degraded),
                new MaxLengthValidator(ErrorSeverity.Degraded, 30),
                new MinDecimalValueValidator(ErrorSeverity.Degraded, "0")
            };
            Weight = Navigator.Eval(sXp_Weight);
            if (validators.IsValid(Weight))
            {
                return true;
            }

            ReportingUtility.ReportSpecificationError(Navigator.NamespaceURI, sXp_Weight.Expression,
                validators.GetMessage());
            return false;
        }

        // Not Required
        internal bool IsValidDimension()
        {
            var validators = new ValidatorCollection
            {
                new RequiredStringValidator(ErrorSeverity.Benign),
                new MaxLengthValidator(ErrorSeverity.Benign, 200)
            };
            Dimension = Navigator.Eval(sXp_Dimension);
            if (validators.IsValid(Dimension))
            {
                return true;
            }

            ReportingUtility.ReportSpecificationError(Navigator.NamespaceURI, sXp_Dimension.Expression,
                validators.GetMessage());
            return false;
        }
    }
}