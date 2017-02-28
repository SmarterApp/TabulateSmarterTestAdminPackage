using System.Collections.Generic;
using ProcessSmarterTestPackage.Processors.Common;

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
    }
}