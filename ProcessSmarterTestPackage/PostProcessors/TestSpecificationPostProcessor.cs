using System;
using System.Collections.Generic;
using System.Linq;
using ProcessSmarterTestPackage.Processors.Common;
using SmarterTestPackage.Common.Data;

namespace ProcessSmarterTestPackage.PostProcessors
{
    public class TestSpecificationPostProcessor : PostProcessor
    {
        public TestSpecificationPostProcessor(PackageType packageType, Processor processor)
            : base(packageType, processor) {}

        public override IList<ValidationError> GenerateErrors()
        {
            var result = new List<ValidationError>();
            var id = ((TestSpecificationProcessor) Processor).GetUniqueId();

            var blueprintElements = Processor.ChildNodeWithName(PackageType.ToString().ToLower())
                .ChildNodeWithName("testblueprint")
                .ChildNodesWithName("bpelement").ToList();

            var blueprintTestElement =
                blueprintElements
                    .FirstOrDefault(x => x.ValueForAttribute("elementtype")
                        .Equals("test", StringComparison.OrdinalIgnoreCase));
            var blueprintSegmentElements = blueprintElements
                .Where(x => x.ValueForAttribute("elementtype")
                    .Equals("segment", StringComparison.OrdinalIgnoreCase)).ToList();
            if (blueprintTestElement != null)
            {
                var blueprintId = blueprintTestElement.ChildNodeWithName("identifier").ValueForAttribute("uniqueid");
                if (!id.Equals(blueprintId, StringComparison.OrdinalIgnoreCase))
                {
                    result.Add(new ValidationError
                    {
                        ErrorSeverity = ErrorSeverity.Severe,
                        GeneratedMessage =
                            $"[TestBlueprint test uniqueid {blueprintId} != TestSpecification uniqueid {id}]",
                        Key = "identifier",
                        Location =
                            $"testspecification/{PackageType.ToString().ToLower()}/testblueprint/bpelement/identifier",
                        PackageType = PackageType
                    });
                }
                // There is no valid test
                if (!blueprintSegmentElements.Any())
                {
                    result.Add(new ValidationError
                    {
                        ErrorSeverity = ErrorSeverity.Severe,
                        GeneratedMessage =
                            "[TestBlueprint contains no segment elements]",
                        Key = "elementtype",
                        Location =
                            $"testspecification/{PackageType.ToString().ToLower()}/testblueprint/bpelement",
                        PackageType = PackageType
                    });
                }
                else if (blueprintSegmentElements.Count() == 1 &&
                         !blueprintSegmentElements.First()
                             .ChildNodeWithName("identifier")
                             .ValueForAttribute("uniqueid")
                             .Equals(blueprintId, StringComparison.OrdinalIgnoreCase))
                {
                    // Test with one segment ID == test ID
                    result.Add(new ValidationError
                    {
                        ErrorSeverity = ErrorSeverity.Severe,
                        GeneratedMessage =
                            $"[Segment ID must match Test ID in single-segmented assessments - SegmentId:{blueprintSegmentElements.First().ChildNodeWithName("identifier").ValueForAttribute("uniqueid")}!=TestId:{blueprintId}]",
                        Key = "uniqueid",
                        Location =
                            $"testspecification/{PackageType.ToString().ToLower()}/testblueprint/bpelement/identifier",
                        PackageType = PackageType
                    });
                }
                else if (blueprintSegmentElements.Count() > 1 &&
                         blueprintSegmentElements.Any(
                             x =>
                                 x.ChildNodeWithName("identifier")
                                     .ValueForAttribute("uniqueid")
                                     .Equals(blueprintId, StringComparison.OrdinalIgnoreCase)))
                {
                    // Test with multiple segments must not be same as test ID
                    result.Add(new ValidationError
                    {
                        ErrorSeverity = ErrorSeverity.Severe,
                        GeneratedMessage =
                            $"[Segment ID must NOT match Test ID in multi-segmented assessments - SegmentId:{blueprintId}==TestId:{blueprintId}]",
                        Key = "uniqueid",
                        Location =
                            $"testspecification/{PackageType.ToString().ToLower()}/testblueprint/bpelement/identifier",
                        PackageType = PackageType
                    });
                }
                else if (blueprintSegmentElements.Select(x =>
                             x.ChildNodeWithName("identifier")
                                 .ValueForAttribute("uniqueid")).Distinct().Count() != blueprintSegmentElements.Count)
                {
                    // segments in multi-segmented tests must have IDs unique from each other
                    result.Add(new ValidationError
                    {
                        ErrorSeverity = ErrorSeverity.Severe,
                        GeneratedMessage =
                            "[Segment ID must NOT match any other segment ID in multi-segmented assessments]",
                        Key = "uniqueid",
                        Location =
                            $"testspecification/{PackageType.ToString().ToLower()}/testblueprint/bpelement/identifier",
                        PackageType = PackageType
                    });
                }
            }
            else
            {
                result.Add(new ValidationError
                {
                    ErrorSeverity = ErrorSeverity.Severe,
                    GeneratedMessage = "[TestBlueprint contains no test root element]",
                    Key = "identifier",
                    Location =
                        $"testspecification/{PackageType.ToString().ToLower()}/testblueprint/bpelement/identifier",
                    PackageType = PackageType
                });
            }
            // Getting the identifiers of all items in the testblueprint, then all testitems
            result.AddRange(CheckBpRef(blueprintElements
                    .SelectMany(x => x.ChildNodesWithName("identifier"))
                    .Select(x => x.ValueForAttribute("uniqueid")),
                Processor.ChildNodeWithName(PackageType.ToString().ToLower())
                    .ChildNodeWithName("itempool")
                    .ChildNodesWithName("testitem")));

            var testForms = Processor.ChildNodeWithName(PackageType.ToString()).ChildNodesWithName("testform");
            var passageRefs = GetAllPassageRefs().ToList();
            var testItems =
                Processor.ChildNodeWithName(PackageType.ToString())
                    .ChildNodeWithName("itempool")
                    .ChildNodesWithName("testitem").ToList();
            foreach (var testForm in testForms)
            {
                var formPoolProperties = testForm.ChildNodesWithName("poolproperty").ToList();
                var languageProperties =
                    formPoolProperties.Where(
                            x => x.ValueForAttribute("property").Equals("Language", StringComparison.OrdinalIgnoreCase))
                        .ToList();
                var languageCounter = new Dictionary<string, int>();
                var itemType =
                    formPoolProperties.Where(
                            x =>
                                x.ValueForAttribute("property")
                                    .Equals("--ITEMTYPE--", StringComparison.OrdinalIgnoreCase))
                        .ToList();
                var itemTypeCounter = new Dictionary<string, int>();
                foreach (var formPartition in testForm.ChildNodesWithName("formpartition"))
                {
                    foreach (var itemGroup in formPartition.ChildNodesWithName("itemgroup"))
                    {
                        // Check passage references actually tie to a passage in the item pool
                        foreach (var passageref in itemGroup.ChildNodesWithName("passageref"))
                        {
                            if (!passageRefs.Contains(passageref.ValueForAttribute("passageref")))
                            {
                                result.Add(new ValidationError
                                {
                                    ErrorSeverity = ErrorSeverity.Degraded,
                                    Location =
                                        $"testspecification/{PackageType}/testform/formpartition/itemgroup/passageref",
                                    GeneratedMessage =
                                        $"[No passage ID matches test form item group {itemGroup.ChildNodeWithName("identifier").ValueForAttribute("uniqueid")} passageref {passageref.ValueForAttribute("passageref")}]",
                                    ItemId = itemGroup.ChildNodeWithName("identifier").ValueForAttribute("uniqueid"),
                                    Key = "passageref",
                                    PackageType = PackageType,
                                    Value = passageref.Navigator.OuterXml
                                });
                            }
                        }
                        // Check item references actually tie to an item in the item pool
                        foreach (var groupItem in itemGroup.ChildNodesWithName("groupitem"))
                        {
                            var itemMatch =
                                testItems.FirstOrDefault(
                                    x =>
                                        x.ChildNodeWithName("identifier")
                                            .ValueForAttribute("uniqueid")
                                            .Equals(groupItem.ValueForAttribute("itemid"),
                                                StringComparison.OrdinalIgnoreCase));
                            if (itemMatch == null)
                            {
                                result.Add(new ValidationError
                                {
                                    ErrorSeverity = ErrorSeverity.Degraded,
                                    Location =
                                        $"testspecification/{PackageType}/testform/formpartition/itemgroup/groupitem",
                                    GeneratedMessage =
                                        $"[groupitem ID {groupItem.ValueForAttribute("itemid")} does not match any item in the item pool]",
                                    ItemId = groupItem.ValueForAttribute("itemid"),
                                    Key = "itemid",
                                    PackageType = PackageType,
                                    Value = groupItem.Navigator.OuterXml
                                });
                            }
                            else
                                //Check that the pool properties of the test form actually correspond to the items in the item pool (itemcounts)
                            {
                                var itemProperties = itemMatch.ChildNodesWithName("poolproperty");
                                var itemTypeProperties =
                                    itemProperties.Where(
                                        x =>
                                            x.ValueForAttribute("property")
                                                .Equals("--ITEMTYPE--", StringComparison.OrdinalIgnoreCase));
                                var itemLanguageProperties =
                                    itemProperties.Where(
                                        x =>
                                            x.ValueForAttribute("property")
                                                .Equals("Language", StringComparison.OrdinalIgnoreCase));
                                foreach (var type in itemTypeProperties)
                                {
                                    if (itemTypeCounter.ContainsKey(type.ValueForAttribute("value")))
                                    {
                                        itemTypeCounter[type.ValueForAttribute("value")] =
                                            itemTypeCounter[type.ValueForAttribute("value")] + 1;
                                    }
                                    else
                                    {
                                        itemTypeCounter.Add(type.ValueForAttribute("value"), 1);
                                    }
                                }
                                foreach (var language in itemLanguageProperties)
                                {
                                    if (languageCounter.ContainsKey(language.ValueForAttribute("value")))
                                    {
                                        languageCounter[language.ValueForAttribute("value")] =
                                            languageCounter[language.ValueForAttribute("value")] + 1;
                                    }
                                    else
                                    {
                                        languageCounter.Add(language.ValueForAttribute("value"), 1);
                                    }
                                }
                            }
                        }
                    }
                }
                var parsedInt = 0;
                foreach (var language in languageProperties)
                {
                    if (!languageCounter.ContainsKey(language.ValueForAttribute("value")) ||
                        !int.TryParse(language.ValueForAttribute("itemcount"), out parsedInt) ||
                        languageCounter[language.ValueForAttribute("value")] != parsedInt)
                    {
                        var languageCounterValue = languageCounter.ContainsKey(language.ValueForAttribute("value"))
                            ? languageCounter[language.ValueForAttribute("value")]
                            : 0;
                        // The itemcount is off for languages
                        result.Add(new ValidationError
                        {
                            ErrorSeverity = ErrorSeverity.Degraded,
                            Location = $"testspecification/{PackageType}/testform/poolproperty",
                            GeneratedMessage =
                                $"[poolproperty itemcount {language.ValueForAttribute("itemcount")} for language {language.ValueForAttribute("value")} != groupItem count {languageCounterValue}",
                            ItemId = testForm.ChildNodeWithName("identifier").ValueForAttribute("uniqueid"),
                            Key = "itemcount",
                            PackageType = PackageType,
                            Value = language.Navigator.OuterXml
                        });
                    }
                }
                foreach (var type in itemType)
                {
                    if (!itemTypeCounter.ContainsKey(type.ValueForAttribute("value")) ||
                        !int.TryParse(type.ValueForAttribute("itemcount"), out parsedInt) ||
                        itemTypeCounter[type.ValueForAttribute("value")] != parsedInt)
                    {
                        // The itemcount is off for item types
                        var typeCounterValue = itemTypeCounter.ContainsKey(type.ValueForAttribute("value"))
                            ? itemTypeCounter[type.ValueForAttribute("value")]
                            : 0;
                        // The itemcount is off for languages
                        result.Add(new ValidationError
                        {
                            ErrorSeverity = ErrorSeverity.Degraded,
                            Location = $"testspecification/{PackageType}/testform/poolproperty",
                            GeneratedMessage =
                                $"[poolproperty itemcount {type.ValueForAttribute("itemcount")} for type {type.ValueForAttribute("value")} != groupItem count {typeCounterValue}",
                            ItemId = testForm.ChildNodeWithName("identifier").ValueForAttribute("uniqueid"),
                            Key = "itemcount",
                            PackageType = PackageType,
                            Value = type.Navigator.OuterXml
                        });
                    }
                }
            }

            // Ensure that pool property item type info is reflected in the itempool
            var itemPoolProperties = Processor.ChildNodeWithName(PackageType.ToString())
                .ChildNodesWithName("poolproperty")
                .Where(x => x.ValueForAttribute("property").Equals("--ITEMTYPE--", StringComparison.OrdinalIgnoreCase));
            foreach (var poolProperty in itemPoolProperties)
            {
                int poolItemCount;
                if (!int.TryParse(poolProperty.ValueForAttribute("itemcount"), out poolItemCount))
                {
                    continue;
                }
                var testItemCount =
                    testItems.Count(
                        x =>
                            x.ValueForAttribute("itemtype")
                                .Equals(poolProperty.ValueForAttribute("value"), StringComparison.OrdinalIgnoreCase));
                if (poolItemCount != testItemCount)
                {
                    result.Add(new ValidationError
                    {
                        ErrorSeverity = ErrorSeverity.Degraded,
                        Location = $"{PackageType}/poolproperty",
                        GeneratedMessage =
                            $"[poolproperty itemcount {poolItemCount} for type {poolProperty.ValueForAttribute("value")} != testitem count {testItemCount}]",
                        ItemId = string.Empty,
                        Key = "itemcount",
                        PackageType = PackageType,
                        Value = poolProperty.Navigator.OuterXml
                    });
                }
            }

            // Ensure that pool property language info is reflected in the itempool
            var itemPoolLanguages = Processor.ChildNodeWithName(PackageType.ToString())
                .ChildNodesWithName("poolproperty")
                .Where(x => x.ValueForAttribute("property").Equals("Language", StringComparison.OrdinalIgnoreCase));
            foreach (var poolProperty in itemPoolLanguages)
            {
                int poolItemCount;
                if (!int.TryParse(poolProperty.ValueForAttribute("itemcount"), out poolItemCount))
                {
                    continue;
                }
                var testItemCount = testItems.Count(
                    x =>
                        x.ChildNodesWithName("poolproperty")
                            .Any(y => y.ValueForAttribute("property")
                                          .Equals("Language", StringComparison.OrdinalIgnoreCase)
                                      && y.ValueForAttribute("value").Equals(poolProperty.ValueForAttribute("value"))));
                if (poolItemCount != testItemCount)
                {
                    result.Add(new ValidationError
                    {
                        ErrorSeverity = ErrorSeverity.Degraded,
                        Location = $"{PackageType}/poolproperty",
                        GeneratedMessage =
                            $"[poolproperty itemcount {poolItemCount} for type {poolProperty.ValueForAttribute("value")} != testitem count {testItemCount}]",
                        ItemId = string.Empty,
                        Key = "itemcount",
                        PackageType = PackageType,
                        Value = poolProperty.Navigator.OuterXml
                    });
                }
            }

            if (Processor.PackageType == PackageType.Scoring)
            {
                result.AddRange(ComputationRuleErrors(blueprintElements,
                    Processor.ChildNodeWithName(PackageType.ToString().ToLower())
                        .ChildNodeWithName("scoringrules")
                        .ChildNodesWithName("computationrule")));
                result.AddRange(PerformanceLevelErrors(blueprintElements,
                    Processor.ChildNodeWithName(PackageType.ToString().ToLower())
                        .ChildNodeWithName("performancelevels")
                        .ChildNodesWithName("performancelevel")));
            }

            return result;
        }

        private IEnumerable<ValidationError> CheckBpRef(IEnumerable<string> elementIds,
            IEnumerable<Processor> testItems)
        {
            var result = new List<ValidationError>();
            foreach (var testItem in testItems)
            {
                foreach (var bpRef in testItem.ChildNodesWithName("bpref"))
                {
                    var found =
                        elementIds.Any(
                            element =>
                                element.Equals(bpRef.Navigator.InnerXml, StringComparison.OrdinalIgnoreCase));
                    if (!found)
                    {
                        result.Add(new ValidationError
                        {
                            ErrorSeverity = ErrorSeverity.Degraded,
                            GeneratedMessage =
                                $"[BpRef {bpRef.Navigator.InnerXml} is not present in TestBlueprint]",
                            Key = "bpref",
                            ItemId =
                                testItem.ChildNodeWithName("identifier").ValueForAttribute("uniqueid").Split('-').Last(),
                            Value = bpRef.Navigator.OuterXml,
                            PackageType = PackageType,
                            Location = $"testspecification/{PackageType.ToString().ToLower()}/itempool/testitem/bpref"
                        });
                    }
                }
            }
            return result;
        }

        private IEnumerable<string> GetAllPassageRefs()
        {
            return
                Processor.ChildNodeWithName(PackageType.ToString())
                    .ChildNodeWithName("itempool")
                    .ChildNodesWithName("passage")
                    .Select(x => x.ChildNodeWithName("identifier").ValueForAttribute("uniqueid")).ToList();
        }

        private IEnumerable<ValidationError> ComputationRuleErrors(IList<Processor> bpElements,
            IEnumerable<Processor> computationRules)
        {
            var result = new List<ValidationError>();

            foreach (var computationRule in computationRules)
            {
                var match =
                    bpElements.FirstOrDefault(
                        x =>
                            x.ChildNodeWithName("identifier")
                                .ValueForAttribute("uniqueid")
                                .Equals(computationRule.ValueForAttribute("bpelementid")));
                if (match == null)
                {
                    result.Add(new ValidationError
                    {
                        ErrorSeverity = ErrorSeverity.Benign,
                        GeneratedMessage =
                            $"[Property bpelementid {computationRule.ValueForAttribute("bpelementid")} of computationrule {computationRule.ChildNodeWithName("identifier").ValueForAttribute("uniqueid")} does not exist in the test blueprint]",
                        Key = "bpelementid",
                        ItemId = computationRule.ValueForAttribute("bpelementid"),
                        PackageType = PackageType,
                        Location = $"testspecification/{PackageType.ToString().ToLower()}/scoringrules/computationrule"
                    });
                }
            }

            return result;
        }

        private IEnumerable<ValidationError> PerformanceLevelErrors(IList<Processor> bpElements,
            IEnumerable<Processor> performanceLevels)
        {
            var result = new List<ValidationError>();

            foreach (var performanceLevel in performanceLevels)
            {
                var match =
                    bpElements.FirstOrDefault(
                        x =>
                            x.ChildNodeWithName("identifier")
                                .ValueForAttribute("uniqueid")
                                .Equals(performanceLevel.ValueForAttribute("bpelementid")));
                if (match == null)
                {
                    result.Add(new ValidationError
                    {
                        ErrorSeverity = ErrorSeverity.Benign,
                        GeneratedMessage =
                            $"[Performancelevel bpelementid {performanceLevel.ValueForAttribute("bpelementid")} does not exist in the test blueprint]",
                        Key = "bpelementid",
                        ItemId = performanceLevel.ValueForAttribute("bpelementid"),
                        PackageType = PackageType,
                        Location =
                            $"testspecification/{PackageType.ToString().ToLower()}/performancelevels/performancelevel"
                    });
                }
            }

            return result;
        }
    }
}