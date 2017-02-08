using System.Collections.Generic;
using System.Linq;
using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.Enums;
using TabulateSmarterTestAdminPackage.Common.Validators;
using TabulateSmarterTestAdminPackage.Utility;

namespace TabulateSmarterTestAdminPackage.Processors.Specification.TestSpecification.Administration.TestForm
{
    internal class TestFormProcessor : Processor
    {
        private static readonly XPathExpression sXp_Length = XPathExpression.Compile("@length");

        private readonly XPathNavigator _navigator;

        internal TestFormProcessor(XPathNavigator navigator)
        {
            _navigator = navigator;

            TestFormIdentifierProcessor = new TestFormIdentifierProcessor(navigator.SelectSingleNode("identifier"));

            TestFormPropertyProcessors = new List<PropertyProcessor>();
            var properties = navigator.Select("property");
            foreach (XPathNavigator property in properties)
            {
                TestFormPropertyProcessors.Add(new PropertyProcessor(property));
            }

            TestFormPoolPropertyProcessors = new List<TestFormPoolPropertyProcessor>();
            var poolProperties = navigator.Select("poolproperty");
            foreach (XPathNavigator poolProperty in poolProperties)
            {
                TestFormPoolPropertyProcessors.Add(new TestFormPoolPropertyProcessor(poolProperty));
            }

            TestFormPartitionProcessors = new List<TestFormPartitionProcessor>();
            var formPartitions = navigator.Select("formpartition");
            foreach (XPathNavigator formartition in formPartitions)
            {
                TestFormPartitionProcessors.Add(new TestFormPartitionProcessor(formartition));
            }
        }

        private TestFormIdentifierProcessor TestFormIdentifierProcessor { get; }
        private IList<PropertyProcessor> TestFormPropertyProcessors { get; }
        private IList<TestFormPoolPropertyProcessor> TestFormPoolPropertyProcessors { get; }
        private IList<TestFormPartitionProcessor> TestFormPartitionProcessors { get; }

        private string Length { get; set; }

        public override bool Process()
        {
            return IsValidLength()
                   && TestFormIdentifierProcessor.Process()
                   && TestFormPropertyProcessors.All(x => x.Process())
                   && TestFormPoolPropertyProcessors.All(x => x.Process())
                   && TestFormPartitionProcessors.All(x => x.Process());
        }

        internal bool IsValidLength()
        {
            var validators = new ValidatorCollection
            {
                new RequiredIntValidator(ErrorSeverity.Degraded),
                new MaxLengthValidator(ErrorSeverity.Degraded, 10),
                new MinIntValueValidator(ErrorSeverity.Degraded, 1)
            };
            Length = _navigator.Eval(sXp_Length);
            if (validators.IsValid(Length))
            {
                return true;
            }

            AdminPackageUtility.ReportSpecificationError(_navigator.NamespaceURI, sXp_Length.Expression, validators.GetMessage());
            return false;
        }
    }
}