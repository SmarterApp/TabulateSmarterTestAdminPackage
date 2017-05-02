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
                    .Select(x => x.ValueForAttribute("uniqueid")).ToList(),
                Processor.ChildNodeWithName(PackageType.ToString().ToLower())
                    .ChildNodeWithName("itempool")
                    .ChildNodesWithName("testitem")));

            var testForms = Processor.ChildNodeWithName(PackageType.ToString()).ChildNodesWithName("testform").ToList();
            if (
                !testForms.Any(
                    x =>
                        x.ChildNodeWithName("property")
                            .ValueForAttribute("value")
                            .Equals("ENU", StringComparison.OrdinalIgnoreCase)))
            {
                result.Add(new ValidationError
                {
                    ErrorSeverity = ErrorSeverity.Severe,
                    Location =
                                        $"testspecification/{PackageType}/testform/property",
                    GeneratedMessage = "[No English language (ENU) test form present in assessment. Test will fail at runtime in TDS]",
                    Key = "value",
                    PackageType = PackageType
                });
            }
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
                                var itemProperties = itemMatch.ChildNodesWithName("poolproperty").ToList();
                                var itemTypeProperties =
                                    itemProperties.Where(
                                        x =>
                                            x.ValueForAttribute("property")
                                                .Equals("--ITEMTYPE--", StringComparison.OrdinalIgnoreCase)).ToList();
                                var itemLanguageProperties =
                                    itemProperties.Where(
                                        x =>
                                            x.ValueForAttribute("property")
                                                .Equals("Language", StringComparison.OrdinalIgnoreCase)).ToList();

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
                                $"[poolproperty itemcount {language.ValueForAttribute("itemcount")} for language {language.ValueForAttribute("value")} != groupItem count {languageCounterValue}]",
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
                                $"[poolproperty itemcount {type.ValueForAttribute("itemcount")} for type {type.ValueForAttribute("value")} != groupItem count {typeCounterValue}]",
                            ItemId = testForm.ChildNodeWithName("identifier").ValueForAttribute("uniqueid"),
                            Key = "itemcount",
                            PackageType = PackageType,
                            Value = type.Navigator.OuterXml
                        });
                    }
                }

                result.AddRange(TestFormPartitionPoolPropertyMismatches(testForm, testItems));
            }

            // Ensure that pool property item type info is reflected in the itempool
            var itemPoolProperties = Processor.ChildNodeWithName(PackageType.ToString())
                .ChildNodesWithName("poolproperty")
                .Where(x => x.ValueForAttribute("property").Equals("--ITEMTYPE--", StringComparison.OrdinalIgnoreCase));
            // There's an item in the pool that's not present in the pool properties
            if (PackageType == PackageType.Administration)
            {
                testItems.Where(
                        x =>
                            !itemPoolProperties.Any(y =>
                                y.ValueForAttribute("value").Equals(x.ValueForAttribute("itemtype"))))
                    .GroupBy(x => x.ValueForAttribute("itemtype")).ToList().ForEach(x =>
                        result.Add(new ValidationError
                        {
                            ErrorSeverity = ErrorSeverity.Degraded,
                            Location = $"{PackageType}/poolproperty",
                            GeneratedMessage =
                                $"[Item type {x.Key} is present in assigned testitem(s) <{x.Select(y => y.ChildNodeWithName("identifier").ValueForAttribute("uniqueid")).Aggregate((y, z) => $"{y},{z}")}> not present in poolproperties]",
                            ItemId = string.Empty,
                            Key = "itemtype",
                            PackageType = PackageType
                        }));
            }
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

            var scoringOrAdminRoot = Processor.ChildNodeWithName(PackageType.ToString().ToLower());

            result.AddRange(EnsureFormPartitionIdentifiersDoNotMatch(scoringOrAdminRoot));
            result.AddRange(GroupItemsConsistenceyErrors(scoringOrAdminRoot, testItems));

            return result;
        }

        private IEnumerable<ValidationError> TestFormPartitionPoolPropertyMismatches(Processor testForm,
            IList<Processor> testItems)
        {
            var result = new List<ValidationError>();

            var partitionPoolProperties = testForm.ChildNodesWithName("poolproperty").ToList();
            var itemTypeProperties =
                partitionPoolProperties.Where(
                        x => x.ValueForAttribute("property").Equals("--ITEMTYPE--", StringComparison.OrdinalIgnoreCase))
                    .GroupBy(x => x.ValueForAttribute("value"))
                    .ToDictionary(x => x.Key, x => x.ToList());
            var languageTypeProperties =
                partitionPoolProperties.Where(
                        x => x.ValueForAttribute("property").Equals("Language", StringComparison.OrdinalIgnoreCase))
                    .GroupBy(x => x.ValueForAttribute("value"))
                    .ToDictionary(x => x.Key, x => x.ToList());

            foreach (var key in itemTypeProperties.Keys)
            {
                if (itemTypeProperties[key].Count > 1)
                {
                    result.Add(new ValidationError
                    {
                        ErrorSeverity = ErrorSeverity.Degraded,
                        Location = $"{PackageType}/testform/poolproperty",
                        GeneratedMessage =
                            $"[Multiple poolproperties exist in testform {testForm.ChildNodeWithName("identifier").ValueForAttribute("uniqueid")} with value {key}]",
                        Key = "value",
                        PackageType = PackageType,
                        Value = itemTypeProperties[key].First().Navigator.OuterXml
                    });
                }
            }
            foreach (var key in languageTypeProperties.Keys)
            {
                if (languageTypeProperties[key].Count > 1)
                {
                    result.Add(new ValidationError
                    {
                        ErrorSeverity = ErrorSeverity.Degraded,
                        Location = $"{PackageType}/testform/poolproperty",
                        GeneratedMessage =
                            $"[Multiple poolproperties exist in testform {testForm.ChildNodeWithName("identifier").ValueForAttribute("uniqueid")} with value {key}]",
                        Key = "value",
                        PackageType = PackageType,
                        Value = itemTypeProperties[key].First().Navigator.OuterXml
                    });
                }
            }

            var testItemsInForm =
                testForm.ChildNodesWithName("formpartition")
                    .SelectMany(
                        x =>
                            x.ChildNodesWithName("itemgroup")
                                .SelectMany(
                                    y => y.ChildNodesWithName("groupitem").Select(z => z.ValueForAttribute("itemid"))));
            var testItemTypes =
                testItems.Where(
                        x => testItemsInForm.Contains(x.ChildNodeWithName("identifier").ValueForAttribute("uniqueid")))
                    .GroupBy(x => x.ValueForAttribute("itemtype"))
                    .ToDictionary(x => x.Key, x => x.ToList());

            foreach (var key in testItemTypes.Keys)
            {
                if (!itemTypeProperties.ContainsKey(key))
                {
                    result.Add(new ValidationError
                    {
                        ErrorSeverity = ErrorSeverity.Degraded,
                        Location = $"{PackageType}/testform/poolproperty",
                        GeneratedMessage =
                            $"[Missing itemtype pool property in testform {testForm.ChildNodeWithName("identifier").ValueForAttribute("uniqueid")} for key {key}]",
                        Key = "value",
                        PackageType = PackageType,
                        Value = testItemTypes[key].First().Navigator.OuterXml
                    });
                }
            }

            var testLanguageTypes =
                testItems.Where(
                        x => testItemsInForm.Contains(x.ChildNodeWithName("identifier").ValueForAttribute("uniqueid")))
                    .SelectMany(
                        x =>
                            x.ChildNodesWithName("property")
                                .Where(
                                    y =>
                                        y.ValueForAttribute("property")
                                            .Equals("Language", StringComparison.OrdinalIgnoreCase)))
                    .GroupBy(x => x.ValueForAttribute("value"))
                    .ToDictionary(x => x.Key, x => x.ToList());

            foreach (var key in testLanguageTypes.Keys)
            {
                if (!languageTypeProperties.ContainsKey(key))
                {
                    result.Add(new ValidationError
                    {
                        ErrorSeverity = ErrorSeverity.Degraded,
                        Location = $"{PackageType}/testform/poolproperty",
                        GeneratedMessage =
                            $"[Missing language pool property in testform {testForm.ChildNodeWithName("identifier").ValueForAttribute("uniqueid")} for key {key}]",
                        Key = "value",
                        PackageType = PackageType,
                        Value = testLanguageTypes[key].First().Navigator.OuterXml
                    });
                }
            }

            return result;
        }

        private IEnumerable<ValidationError> CheckBpRef(IList<string> elementIds,
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

        private IEnumerable<ValidationError> GroupItemsConsistenceyErrors(Processor processor,
            IList<Processor> testItems)
        {
            var result = new List<ValidationError>();

            var nonBrailleTestForms =
                processor.ChildNodesWithName("testform")
                    .Where(
                        x =>
                            !x.ChildNodesWithName("property")
                                .Any(
                                    y =>
                                        y.ValueForAttribute("name")
                                            .Equals("language", StringComparison.OrdinalIgnoreCase) &&
                                        y.ValueForAttribute("value").ToLower().Contains("braille")));
            var brailleTestForms = processor.ChildNodesWithName("testform")
                .Where(
                    x =>
                        x.ChildNodesWithName("property")
                            .Any(
                                y =>
                                    y.ValueForAttribute("name")
                                        .Equals("language", StringComparison.OrdinalIgnoreCase) &&
                                    y.ValueForAttribute("value").ToLower().Contains("braille"))).ToList();

            var testFormItemGroups =
                nonBrailleTestForms.SelectMany(x => x.ChildNodesWithName("formpartition"))
                    .SelectMany(x => x.ChildNodesWithName("itemgroup"));
            var brailleTestFormItemGroups = brailleTestForms.SelectMany(x => x.ChildNodesWithName("formpartition"))
                .SelectMany(x => x.ChildNodesWithName("itemgroup"));
            var segmentPoolItemGroups =
                processor.ChildNodesWithName("adminsegment")
                    .SelectMany(x => x.ChildNodesWithName("segmentpool"))
                    .SelectMany(x => x.ChildNodesWithName("itemgroup"));
            var indexGroupItemInfo = new Dictionary<string, GroupItemInfo>();

            var combinedItemGroups = new List<Processor>();

            combinedItemGroups.AddRange(testFormItemGroups);
            combinedItemGroups.AddRange(segmentPoolItemGroups);

            foreach (var itemGroup in combinedItemGroups)
            {
                foreach (var info in GetInfo(itemGroup))
                {
                    GroupItemInfo gii;
                    if (indexGroupItemInfo.TryGetValue(info.ItemId, out gii))
                    {
                        var testItem =
                            testItems.FirstOrDefault(
                                x =>
                                    x.ChildNodeWithName("identifier")
                                        .ValueForAttribute("uniqueid")
                                        .Equals(info.ItemId, StringComparison.OrdinalIgnoreCase));
                        if (testItem == null)
                        {
                            // The item doesn't exist and we have a problem
                            result.Add(new ValidationError
                            {
                                AssessmentId = ((TestSpecificationProcessor) Processor).GetUniqueId(),
                                ErrorSeverity = ErrorSeverity.Severe,
                                GeneratedMessage =
                                    $"[Item referenced in itemgroup {itemGroup.ChildNodeWithName("identifier").ValueForAttribute("uniqueid")} does not exist in item pool]",
                                Key = "itemid",
                                Location = $"{Processor.PackageType}/testform/formpartition/itemgroup/groupitem",
                                PackageType = Processor.PackageType,
                                ItemId = info.ItemId,
                                Value = itemGroup.Navigator.OuterXml
                            });
                        }
                        else
                        {
                            result.AddRange(GroupItemInfoInconsistencyErrors(gii, info, itemGroup));
                        }
                    }
                    else
                    {
                        indexGroupItemInfo.Add(info.ItemId, info);
                    }
                }
            }

            foreach (var itemGroup in brailleTestFormItemGroups)
            {
                foreach (var info in GetInfo(itemGroup))
                {
                    GroupItemInfo gii;
                    if (indexGroupItemInfo.TryGetValue(info.ItemId, out gii))
                    {
                        var testItem =
                            testItems.FirstOrDefault(
                                x =>
                                    x.ChildNodeWithName("identifier")
                                        .ValueForAttribute("uniqueid")
                                        .Equals(info.ItemId, StringComparison.OrdinalIgnoreCase));
                        if (testItem == null)
                        {
                            // The item doesn't exist and we have a problem
                            result.Add(new ValidationError
                            {
                                AssessmentId = ((TestSpecificationProcessor) Processor).GetUniqueId(),
                                ErrorSeverity = ErrorSeverity.Severe,
                                GeneratedMessage =
                                    $"[Item referenced in itemgroup {itemGroup.ChildNodeWithName("identifier").ValueForAttribute("uniqueid")} does not exist in item pool]",
                                Key = "itemid",
                                Location = $"{Processor.PackageType}/testform/formpartition/itemgroup/groupitem",
                                PackageType = Processor.PackageType,
                                ItemId = info.ItemId,
                                Value = itemGroup.Navigator.OuterXml
                            });
                        }
                        else if (testItem.ValueForAttribute("itemtype").Equals("GI", StringComparison.OrdinalIgnoreCase))
                        {
                            // This item type is illegal for braille tests
                            result.Add(new ValidationError
                            {
                                AssessmentId = ((TestSpecificationProcessor) Processor).GetUniqueId(),
                                ErrorSeverity = ErrorSeverity.Severe,
                                GeneratedMessage =
                                    $"[Item referenced in itemgroup {itemGroup.ChildNodeWithName("identifier").ValueForAttribute("uniqueid")} is of type GI and not suitable for braille tests]",
                                Key = "itemid",
                                Location = $"{Processor.PackageType}/testform/formpartition/itemgroup/groupitem",
                                PackageType = Processor.PackageType,
                                ItemId = info.ItemId,
                                Value = itemGroup.Navigator.OuterXml
                            });
                        }
                        else
                        {
                            result.AddRange(GroupItemInfoInconsistencyErrors(gii, info, itemGroup));
                        }
                    }
                    else
                    {
                        // all other forms have been processed, this form contains additional items not present in the other forms
                        result.Add(new ValidationError
                        {
                            AssessmentId = ((TestSpecificationProcessor) Processor).GetUniqueId(),
                            ErrorSeverity = ErrorSeverity.Severe,
                            GeneratedMessage =
                                $"[Item referenced in itemgroup {itemGroup.ChildNodeWithName("identifier").ValueForAttribute("uniqueid")} is not present in any other forms]",
                            Key = "itemid",
                            Location = $"{Processor.PackageType}/testform/formpartition/itemgroup/groupitem",
                            PackageType = Processor.PackageType,
                            ItemId = info.ItemId,
                            Value = itemGroup.Navigator.OuterXml
                        });
                    }
                }
            }

            return result;
        }

        private IEnumerable<ValidationError> GroupItemInfoInconsistencyErrors(GroupItemInfo gii, GroupItemInfo info,
            Processor itemGroup)
        {
            var result = new List<ValidationError>();
            // Legacy validations
            if (!string.Equals(gii.IsFieldTest, info.IsFieldTest, StringComparison.OrdinalIgnoreCase))
            {
                result.Add(GenerateGroupItemValidationError(info,
                    $"Conflicting isfieldtest info for same itemId {info.ItemId} between forms: '{info.IsFieldTest}' != '{gii.IsFieldTest}'",
                    "isfieldtest", itemGroup.PackageType, itemGroup.Navigator.OuterXml));
            }
            if (!string.Equals(gii.IsActive, info.IsActive, StringComparison.OrdinalIgnoreCase))
            {
                result.Add(GenerateGroupItemValidationError(info,
                    $"Conflicting isActive info for same itemId {info.ItemId} between forms: '{info.IsActive}' != '{gii.IsActive}'",
                    "isactive", itemGroup.PackageType, itemGroup.Navigator.OuterXml));
            }
            if (
                !string.Equals(gii.ResponseRequired, info.ResponseRequired,
                    StringComparison.OrdinalIgnoreCase))
            {
                result.Add(GenerateGroupItemValidationError(info,
                    $"Conflicting responseRequired info for same itemId {info.ItemId} between forms: '{info.ResponseRequired}' != '{gii.ResponseRequired}'",
                    "responserequired", itemGroup.PackageType, itemGroup.Navigator.OuterXml));
            }
            if (!string.Equals(gii.AdminRequired, info.AdminRequired, StringComparison.OrdinalIgnoreCase))
            {
                result.Add(GenerateGroupItemValidationError(info,
                    $"Conflicting adminRequired info for same itemId {info.ItemId} between forms: '{info.AdminRequired}' != '{gii.AdminRequired}'",
                    "adminrequired", itemGroup.PackageType, itemGroup.Navigator.OuterXml));
            }
            if (!string.Equals(gii.FormPosition, info.FormPosition, StringComparison.OrdinalIgnoreCase))
            {
                result.Add(GenerateGroupItemValidationError(info,
                    $"Conflicting formPosition info for same itemId {info.ItemId} between forms: '{info.FormPosition}' != '{gii.FormPosition}'",
                    "formposition", itemGroup.PackageType, itemGroup.Navigator.OuterXml));
            }
            if (!string.Equals(gii.GroupPosition, info.GroupPosition, StringComparison.OrdinalIgnoreCase))
            {
                result.Add(GenerateGroupItemValidationError(info,
                    $"Conflicting groupPosition info for same itemId {info.ItemId} between forms: '{info.GroupPosition}' != '{gii.GroupPosition}'",
                    "groupposition", itemGroup.PackageType, itemGroup.Navigator.OuterXml));
            }
            return result;
        }

        private IEnumerable<ValidationError> EnsureFormPartitionIdentifiersDoNotMatch(Processor processor)
        {
            var result = new List<ValidationError>();

            var formPartitions =
                processor.ChildNodesWithName("testform")
                    .SelectMany(
                        x =>
                            x.ChildNodesWithName("formpartition")
                                .Select(y => y.ChildNodeWithName("identifier").ValueForAttribute("uniqueid"))).ToList();
            if (formPartitions.Count != formPartitions.Distinct().Count())
            {
                result.Add(new ValidationError
                {
                    AssessmentId = ((TestSpecificationProcessor) Processor).GetUniqueId(),
                    ErrorSeverity = ErrorSeverity.Severe,
                    GeneratedMessage = "[Formpartition identifiers are not unique]",
                    Key = "uniqueid",
                    Location = $"{Processor.PackageType}/testform/formpartition/identifier",
                    PackageType = Processor.PackageType
                });
            }

            if (Processor.PackageType == PackageType.Administration)
            {
                processor.ChildNodesWithName("adminsegment")
                    .SelectMany(
                        x => x.ChildNodesWithName("segmentform").Select(y => y.ValueForAttribute("formpartitionid")))
                    .Where(x => !formPartitions.Contains(x))
                    .ToList()
                    .ForEach(x => result.Add(new ValidationError
                    {
                        AssessmentId = ((TestSpecificationProcessor) Processor).GetUniqueId(),
                        ErrorSeverity = ErrorSeverity.Severe,
                        GeneratedMessage = $"[segmentform formpartitionid {x} does not reference a known partition]",
                        ItemId = x,
                        Key = "formpartitionid",
                        Location = $"{Processor.PackageType}/adminsegment/segmentform",
                        PackageType = Processor.PackageType
                    }));
            }

            return result;
        }

        private static IEnumerable<GroupItemInfo> GetInfo(Processor processor)
        {
            var result = new List<GroupItemInfo>();

            var groupId = processor.ChildNodeWithName("identifier").ValueForAttribute("uniqueid");
            processor.ChildNodesWithName("groupitem").ToList().ForEach(x => result.Add(new GroupItemInfo
            {
                AdminRequired = x.ValueForAttribute("adminrequired"),
                FormPosition = x.ValueForAttribute("formposition"),
                GroupId = groupId,
                IsActive = x.ValueForAttribute("isactive"),
                IsFieldTest = x.ValueForAttribute("isfieldtest"),
                ResponseRequired = x.ValueForAttribute("responserequired"),
                ItemId = x.ValueForAttribute("itemid"),
                GroupPosition = x.ValueForAttribute("groupposition")
            }));

            return result;
        }

        private ValidationError GenerateGroupItemValidationError(GroupItemInfo info, string violation, string key,
            PackageType packageType, string value)
        {
            return new ValidationError
            {
                ErrorSeverity = ErrorSeverity.Degraded,
                GeneratedMessage = $"[GroupItem {info.ItemId} within itemgroup {info.GroupId} violated {violation}]",
                ItemId = info.ItemId,
                Key = key,
                Location =
                    $"administration/adminsegment/segmentpool/itemgroup/groupitem || testspecification/{Processor.PackageType.ToString().ToLower()}/testform/formpartition/itemgroup/groupitem",
                PackageType = packageType,
                Value = value
            };
        }
    }
}