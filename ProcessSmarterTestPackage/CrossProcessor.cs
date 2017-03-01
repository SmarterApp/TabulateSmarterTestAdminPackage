using System.Collections.Generic;
using System.Linq;
using ProcessSmarterTestPackage.Processors.Common;
using ProcessSmarterTestPackage.Processors.Common.ItemPool.TestItem;
using SmarterTestPackage.Common.Data;
using ValidateSmarterTestPackage.RestrictedValues.Enums;

namespace ProcessSmarterTestPackage
{
    public class CrossProcessor
    {
        public CrossProcessor(List<Dictionary<string, string>> itemContentPackage,
            List<Dictionary<string, string>> stimuliContentPackage)
        {
            ItemContentPackage = itemContentPackage;
            StimuliContentPackage = stimuliContentPackage;
        }

        public List<Dictionary<string, string>> ItemContentPackage { get; set; }
        public List<Dictionary<string, string>> StimuliContentPackage { get; set; }

        public Dictionary<string, List<TestSpecificationProcessor>> TestPackages { get; set; } =
            new Dictionary<string, List<TestSpecificationProcessor>>();

        public void AddProcessedTestPackage(TestSpecificationProcessor processor)
        {
            var uniqueId = processor.ValueForAttribute("uniqueid");
            if (TestPackages.ContainsKey(uniqueId))
            {
                TestPackages[uniqueId].Add(processor);
            }
            else
            {
                TestPackages.Add(uniqueId, new List<TestSpecificationProcessor>
                {
                    processor
                });
            }
        }

        public List<CrossPackageValidationError> ExecuteValidation()
        {
            var result = new List<CrossPackageValidationError>();
            foreach (var key in TestPackages.Keys)
            {
                if (TestPackages[key] == null || !TestPackages[key].Any())
                {
                    continue;
                }
                var adminPackage = TestPackages[key].FirstOrDefault(x => x.PackageType == PackageType.Administration);
                var scoringPackage = TestPackages[key].FirstOrDefault(x => x.PackageType == PackageType.Scoring);

                if (adminPackage == null)
                {
                    result.AddRange(CrossValidate(scoringPackage));
                }
                if (scoringPackage == null)
                {
                    result.AddRange(CrossValidate(adminPackage));
                }
                else
                {
                    result.AddRange(CrossValidate(adminPackage, scoringPackage));
                }
            }
            return result;
        }

        private List<CrossPackageValidationError> CrossValidate(TestSpecificationProcessor primary)
        {
            var itemPool =
                primary.ChildNodeWithName(primary.PackageType == PackageType.Administration
                    ? "administration"
                    : "scoring").ChildNodeWithName("itempool");
            var items = itemPool.ChildNodesWithName("testitem");
            var passages = itemPool.ChildNodesWithName("passage");
            return ValidateItems(primary.ValueForAttribute("uniqueid"), items.Cast<TestItemProcessor>());
        }

        private List<CrossPackageValidationError> CrossValidate(TestSpecificationProcessor primary,
            TestSpecificationProcessor secondary)
        {
            return null;
        }

        private List<CrossPackageValidationError> ValidateItems(string key, IEnumerable<TestItemProcessor> processors)
        {
            var errors = new List<CrossPackageValidationError>();
            foreach (var processor in processors)
            {
                var item =
                    ItemContentPackage.FirstOrDefault(x => x["ItemId"].Equals(processor.ValueForAttribute("ItemId")));
                if (item == null)
                {
                    errors.Add(new CrossPackageValidationError
                    {
                        ErrorSeverity = ErrorSeverity.Severe,
                        GeneratedMessage = "[Item Does not exist in content package]",
                        ItemId = processor.ValueForAttribute("ItemId"),
                        Key = "ItemId",
                        Location = "Item Cross-Tabulation (Item Content Package)",
                        Path = $"testspecification/{processor.PackageType.ToString().ToLower()}/itempool/testitem",
                        PrimarySource = $"{key} - {processor.PackageType}",
                        SecondarySource = "Item Content Package",
                        TestName = key
                    });
                }
            }
            return errors;
        }

        private List<CrossPackageValidationError> ValidateSubject(Dictionary<string, string> item,
            TestItemProcessor processor)
        {
            return new List<CrossPackageValidationError>();
        }
    }
}