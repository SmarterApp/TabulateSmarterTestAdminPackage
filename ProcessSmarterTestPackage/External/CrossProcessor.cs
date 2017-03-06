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

        public Dictionary<string, List<CrossPackageValidationError>> Errors { get; set; } = new Dictionary<string,List<CrossPackageValidationError>>();

        public ContentPackageCrossProcessor ContentPackageCrossProcessor { get; set; } =
            new ContentPackageCrossProcessor();

        public TestPackageCrossProcessor TestPackageCrossProcessor { get; set; } = new TestPackageCrossProcessor();

        public Dictionary<string, List<TestSpecificationProcessor>> TestPackages { get; set; } =
            new Dictionary<string, List<TestSpecificationProcessor>>();

        public void AddProcessedTestPackage(TestSpecificationProcessor processor)
        {
            var uniqueId = processor.GetUniqueId();
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

        public void AddCrossProcessingErrors(TestSpecificationProcessor processor, IEnumerable<CrossPackageValidationError> errors)
        {
            var uniqueId = processor.GetUniqueId();
            if (!Errors.ContainsKey(uniqueId))
            {
                Errors.Add(uniqueId, new List<CrossPackageValidationError>());
            }
            Errors[uniqueId].AddRange(errors);
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

                if (adminPackage == null && scoringPackage == null)
                {
                    continue;
                }

                if (adminPackage == null)
                {
                    result.AddRange(ContentPackageCrossProcessor.CrossValidateContent(scoringPackage, ItemContentPackage,
                        StimuliContentPackage));
                }
                else if (scoringPackage == null)
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