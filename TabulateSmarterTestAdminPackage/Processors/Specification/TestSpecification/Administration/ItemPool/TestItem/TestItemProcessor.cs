using System.Collections.Generic;
using System.Linq;
using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.Enums;
using TabulateSmarterTestAdminPackage.Common.Validators;
using TabulateSmarterTestAdminPackage.Utility;

namespace TabulateSmarterTestAdminPackage.Processors.Specification.TestSpecification.Administration.ItemPool.TestItem
{
    internal class TestItemProcessor : Processor
    {
        private static readonly XPathExpression sXp_FileName = XPathExpression.Compile("@filename");
        private static readonly XPathExpression sXp_ItemType = XPathExpression.Compile("@itemtype");
        private readonly XPathNavigator _navigator;

        internal TestItemProcessor(XPathNavigator navigator)
        {
            _navigator = navigator;
            TestItemIdentifierProcessor = new TestItemIdentifierProcessor(navigator.SelectSingleNode("identifier"));

            BPrefProcessors = new List<BPrefProcessor>();
            var bPrefs = navigator.Select("bpref");
            foreach (XPathNavigator bPref in bPrefs)
            {
                BPrefProcessors.Add(new BPrefProcessor(bPref));
            }

            TestItemPassageRefProcessors = new List<PassageRefProcessor>();
            var testItemPassageRefProcessors = navigator.Select("passageref");
            foreach (XPathNavigator testItemPassageRefProcessor in testItemPassageRefProcessors)
            {
                TestItemPassageRefProcessors.Add(new PassageRefProcessor(testItemPassageRefProcessor));
            }

            TestItemPoolPropertyProcessors = new List<TestItemPoolPropertyProcessor>();
            var testItemPoolProperties = navigator.Select("poolproperty");
            foreach (XPathNavigator testItemPoolProperty in testItemPoolProperties)
            {
                TestItemPoolPropertyProcessors.Add(new TestItemPoolPropertyProcessor(testItemPoolProperty));
            }

            ItemScoredDimensionProcessors = new List<ItemScoredDimensionProcessor>();
            var testItemScoredDimensions = navigator.Select("itemscoreddimension");
            foreach (XPathNavigator testItemScoredDimension in testItemScoredDimensions)
            {
                ItemScoredDimensionProcessors.Add(new ItemScoredDimensionProcessor(testItemScoredDimension));
            }
        }

        private TestItemIdentifierProcessor TestItemIdentifierProcessor { get; }
        private IList<BPrefProcessor> BPrefProcessors { get; }
        private IList<PassageRefProcessor> TestItemPassageRefProcessors { get; }
        private IList<TestItemPoolPropertyProcessor> TestItemPoolPropertyProcessors { get; }
        private IList<ItemScoredDimensionProcessor> ItemScoredDimensionProcessors { get; }

        private string FileName { get; set; }
        private string ItemType { get; set; }

        public override bool Process()
        {
            return IsValidFileName()
                   && IsValidItemType()
                   && TestItemIdentifierProcessor.Process()
                   && BPrefProcessors.All(x => x.Process())
                   && TestItemPassageRefProcessors.All(x => x.Process())
                   && TestItemPoolPropertyProcessors.All(x => x.Process())
                   && ItemScoredDimensionProcessors.All(x => x.Process());
        }

        internal bool IsValidFileName()
        {
            var validators = new ValidatorCollection
            {
                new RequiredStringValidator(ErrorSeverity.Degraded),
                new MaxLengthValidator(ErrorSeverity.Degraded, 200),
                new FilePathValidator(ErrorSeverity.Degraded)
            };
            FileName = _navigator.Eval(sXp_FileName);
            if (validators.IsValid(FileName))
            {
                return true;
            }

            AdminPackageUtility.ReportSpecificationError(_navigator.NamespaceURI, sXp_FileName.Expression, validators.GetMessage());
            return false;
        }

        internal bool IsValidItemType()
        {
            var validators = new ValidatorCollection
            {
                new RequiredStringValidator(ErrorSeverity.Degraded),
                new MaxLengthValidator(ErrorSeverity.Degraded, 50)
            };
            ItemType = _navigator.Eval(sXp_ItemType);
            if (validators.IsValid(ItemType))
            {
                return true;
            }

            AdminPackageUtility.ReportSpecificationError(_navigator.NamespaceURI, sXp_ItemType.Expression, validators.GetMessage());
            return false;
        }
    }
}