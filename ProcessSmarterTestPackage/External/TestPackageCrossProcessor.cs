using System;
using System.Collections.Generic;
using System.Linq;
using ProcessSmarterTestPackage.Processors.Common;
using ProcessSmarterTestPackage.Processors.Common.ItemPool.Passage;
using ProcessSmarterTestPackage.Processors.Common.ItemPool.TestItem;
using SmarterTestPackage.Common.Data;

namespace ProcessSmarterTestPackage.External
{
    public class TestPackageCrossProcessor
    {
        public List<CrossPackageValidationError> CrossValidatePackages(Processor primary,
            Processor secondary)
        {
            var result = new List<CrossPackageValidationError>();
            var administrationItemPool =
                primary.ChildNodeWithName("administration").ChildNodeWithName("itempool");
            var scoringItemPool =
                secondary.ChildNodeWithName("scoring").ChildNodeWithName("itempool");
            result.AddRange(administrationItemPool.CheckEqualTo(scoringItemPool));
            result.AddRange(
                CrossValidatePackageItems(
                    administrationItemPool.ChildNodesWithName("testitem").Cast<TestItemProcessor>(),
                    scoringItemPool.ChildNodesWithName("testitem").Cast<TestItemProcessor>()));
            result.AddRange(
                CrossValidatePackagePassages(
                    administrationItemPool.ChildNodesWithName("passage").Cast<PassageProcessor>(),
                    scoringItemPool.ChildNodesWithName("passage").Cast<PassageProcessor>()));
            return result;
        }

        private static IEnumerable<CrossPackageValidationError> CrossValidatePackageItems(
            IEnumerable<TestItemProcessor> adminItemProcessors, IEnumerable<TestItemProcessor> scoringItemProcessors)
        {
            var result = new List<CrossPackageValidationError>();
            foreach (var adminItemProcessor in adminItemProcessors)
            {
                var id = adminItemProcessor.ChildNodeWithName("identifier").ValueForAttribute("uniqueid");
                var scoringItemProcessor =
                    scoringItemProcessors.FirstOrDefault(
                        x =>
                            x.ChildNodeWithName("identifier")
                                .ValueForAttribute("uniqueid")
                                .Equals(id, StringComparison.OrdinalIgnoreCase));
                if (scoringItemProcessor == null)
                {
                    result.Add(GenerateError("[Item does not exist in scoring package]", id, adminItemProcessor,
                        "uniqueid", "testspecification/scoring/itempool/testitem/identifier"));
                    continue;
                }
                result.AddRange(adminItemProcessor.CheckEqualTo(scoringItemProcessor));
            }
            return result;
        }

        private static IEnumerable<CrossPackageValidationError> CrossValidatePackagePassages(
            IEnumerable<PassageProcessor> adminItemProcessors, IEnumerable<PassageProcessor> scoringItemProcessors)
        {
            var result = new List<CrossPackageValidationError>();
            return result;
        }

        public IEnumerable<CrossPackageValidationError> GetCrossPackageValidationErrors(string identifier,
            Processor primary,
            Processor secondary)
        {
            return GetProcessorsForIdentifier(primary, identifier).SelectMany(x => x.CheckEqualTo(secondary));
        }

        public IEnumerable<string> GetChildIdentifiers(Processor processor)
        {
            return processor.Processors.Select(x => x.Navigator.Name).Distinct();
        }

        public IEnumerable<Processor> GetProcessorsForIdentifier(Processor processor, string identifier)
        {
            return processor.ChildNodesWithName(identifier);
        }

        private static CrossPackageValidationError GenerateError(string message, string id, Processor processor,
            string key, string location)
        {
            return new CrossPackageValidationError
            {
                ErrorSeverity = ErrorSeverity.Severe,
                GeneratedMessage = message,
                ItemId = id,
                Key = processor.Navigator.Name,
                Location = location,
                Value = processor.Navigator.OuterXml,
                PrimarySource = $"{key} - {processor.PackageType}",
                SecondarySource = "Scoring Package",
                AssessmentId = key
            };
        }
    }
}