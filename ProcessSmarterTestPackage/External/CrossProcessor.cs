using System.Collections.Generic;
using System.Linq;
using NLog;
using ProcessSmarterTestPackage.Processors.Common;
using SmarterTestPackage.Common.Data;

namespace ProcessSmarterTestPackage.External
{
    public class CrossProcessor
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public CrossProcessor(IList<ContentPackageItemRow> itemContentPackage,
            IList<ContentPackageStimRow> stimuliContentPackage)
        {
            ItemContentPackage = itemContentPackage;
            StimuliContentPackage = stimuliContentPackage;
        }

        public IList<ContentPackageItemRow> ItemContentPackage { get; set; }
        public IList<ContentPackageStimRow> StimuliContentPackage { get; set; }

        public Dictionary<string, List<CrossPackageValidationError>> Errors { get; set; } =
            new Dictionary<string, List<CrossPackageValidationError>>();

        public ContentPackageCrossProcessor ContentPackageCrossProcessor { get; set; } =
            new ContentPackageCrossProcessor();

        public TestPackageCrossProcessor TestPackageCrossProcessor { get; set; } = new TestPackageCrossProcessor();

        public Dictionary<string, List<Processor>> TestPackages { get; set; } =
            new Dictionary<string, List<Processor>>();

        public void AddProcessedTestPackage(Processor processor)
        {
            var uniqueId = processor.GetUniqueId();
            if (TestPackages.ContainsKey(uniqueId))
            {
                TestPackages[uniqueId].Add(processor);
            }
            else
            {
                TestPackages.Add(uniqueId, new List<Processor>
                {
                    processor
                });
            }
        }

        public void AddCrossProcessingErrors(Processor processor,
            IEnumerable<CrossPackageValidationError> errors)
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
                var combinedPackage = TestPackages[key].FirstOrDefault(x => x.PackageType == PackageType.Combined);

                if (adminPackage == null && scoringPackage == null && combinedPackage == null)
                {
                    continue;
                }

                if (adminPackage == null && combinedPackage == null)
                {
                    result.AddRange(ContentPackageCrossProcessor.CrossValidateContent(scoringPackage, ItemContentPackage,
                        StimuliContentPackage));
                }
                else if (scoringPackage == null && combinedPackage == null)
                {
                    result.AddRange(ContentPackageCrossProcessor.CrossValidateContent(adminPackage, ItemContentPackage,
                        StimuliContentPackage));
                } else if (combinedPackage != null)
                {
                    result.AddRange(ContentPackageCrossProcessor.CrossValidateCombinedContent(combinedPackage, ItemContentPackage));
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