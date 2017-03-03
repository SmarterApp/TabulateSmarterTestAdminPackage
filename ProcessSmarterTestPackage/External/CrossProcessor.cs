using System.Collections.Generic;
using System.Linq;
using ProcessSmarterTestPackage.Processors.Common;
using SmarterTestPackage.Common.Data;
using ValidateSmarterTestPackage.RestrictedValues.Enums;

namespace ProcessSmarterTestPackage.External
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

        public ContentPackageCrossProcessor ContentPackageCrossProcessor { get; set; } =
            new ContentPackageCrossProcessor();

        public TestPackageCrossProcessor TestPackageCrossProcessor { get; set; } = new TestPackageCrossProcessor();

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
                    result.AddRange(ContentPackageCrossProcessor.CrossValidateContent(scoringPackage, ItemContentPackage,
                        StimuliContentPackage));
                }
                if (scoringPackage == null)
                {
                    result.AddRange(ContentPackageCrossProcessor.CrossValidateContent(adminPackage, ItemContentPackage,
                        StimuliContentPackage));
                }
                else
                {
                    result.AddRange(ContentPackageCrossProcessor.CrossValidateContent(scoringPackage, ItemContentPackage,
                        StimuliContentPackage));
                    result.AddRange(ContentPackageCrossProcessor.CrossValidateContent(adminPackage, ItemContentPackage,
                        StimuliContentPackage));
                    result.AddRange(TestPackageCrossProcessor.CrossValidatePackages(adminPackage, scoringPackage));
                }
            }
            return result;
        }
    }
}