using System.Collections.Generic;
using System.Linq;
using ProcessSmarterTestPackage.Processors.Common;
using SmarterTestPackage.Common.Data;
using ValidateSmarterTestPackage.RestrictedValues.Enums;

namespace ProcessSmarterTestPackage
{
    public class CrossProcessor
    {
        public CrossProcessor(Dictionary<string, string> contentPackage)
        {
            ContentPackage = contentPackage;
        }

        public Dictionary<string, string> ContentPackage { get; set; }

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

        public List<CrossPackageValidationError> CrossValidate(TestSpecificationProcessor primary,
            TestSpecificationProcessor secondary = null)
        {
            return null;
        }
    }
}