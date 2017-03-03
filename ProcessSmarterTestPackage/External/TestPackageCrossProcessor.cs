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
        public List<CrossPackageValidationError> CrossValidatePackages(TestSpecificationProcessor primary,
            TestSpecificationProcessor secondary)
        {
            var result = new List<CrossPackageValidationError>();
            var administrationItemPool =
                primary.ChildNodeWithName("administration").ChildNodeWithName("itempool");
            var scoringItemPool =
                primary.ChildNodeWithName("scoring").ChildNodeWithName("itempool");
            result.AddRange(administrationItemPool.EqualTo(scoringItemPool));
            result.AddRange(CrossValidatePackageItems(
                administrationItemPool.ChildNodesWithName("testitem").Cast<TestItemProcessor>(),
                scoringItemPool.ChildNodesWithName("testitem").Cast<TestItemProcessor>()));
            return result;
        }

        private List<CrossPackageValidationError> CrossValidatePackageItems(
            IEnumerable<TestItemProcessor> adminItemProcessors, IEnumerable<TestItemProcessor> scoringItemProcessors)
        {
            var result = new List<CrossPackageValidationError>();
            foreach (var adminItemProcessor in adminItemProcessors)
            {
                var id = adminItemProcessor.ChildNodeWithName("identifier").ValueForAttribute("uniqueid");
                var scoringProcessor =
                    scoringItemProcessors.FirstOrDefault(
                        x =>
                            x.ChildNodeWithName("identifier")
                                .ValueForAttribute("uniqueid")
                                .Equals(id, StringComparison.OrdinalIgnoreCase));
                if (scoringProcessor == null)
                {
                    result.Add(new CrossPackageValidationError
                    {
                        ErrorSeverity = ErrorSeverity.Severe,
                        GeneratedMessage = "[Item does not exist in scoring package]",
                        ItemId = id,
                        Key = "uniqueid",
                        Location = "Item Cross-Tabulation (Scoring Package)",
                        Path = "testspecification/scoring/itempool/testitem/identifier",
                        PrimarySource = string.Empty,
                        SecondarySource = string.Empty,
                        TestName = string.Empty
                    });
                    continue;
                }
                result.AddRange(
                    adminItemProcessor.ChildNodeWithName("identifier")
                        .EqualTo(scoringProcessor.ChildNodeWithName("identifier"), new[] {"version"}));
                foreach (var poolProperty in adminItemProcessor.ChildNodesWithName("poolproperty"))
                {
                    var scoringPoolProperty =
                        scoringProcessor.ChildNodesWithName("poolproperty")
                            .FirstOrDefault(
                                x => x.ValueForAttribute("property").Equals(poolProperty.ValueForAttribute("property")));
                    if (scoringPoolProperty == null)
                    {
                        result.Add(new CrossPackageValidationError
                        {
                            ErrorSeverity = ErrorSeverity.Severe,
                            GeneratedMessage =
                                $"[Pool property {poolProperty.ValueForAttribute("property")} does not exist in item {id}]",
                            ItemId = id,
                            Key = poolProperty.ValueForAttribute("property"),
                            Location = "Item Cross-Tabulation (Scoring Package)",
                            Path = "testspecification/scoring/itempool/testitem/poolproperty",
                            PrimarySource = string.Empty,
                            SecondarySource = string.Empty,
                            TestName = string.Empty
                        });
                    }
                }
            }
            return result;
        }

        private List<CrossPackageValidationError> CrossValidatePackagePassages(
            IEnumerable<PassageProcessor> adminPassageProcessors, IEnumerable<PassageProcessor> scoringPassageProcessors)
        {
            var result = new List<CrossPackageValidationError>();
            return result;
        }
    }
}