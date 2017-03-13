﻿using System;
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
            var blueprintTestElement =
                Processor.ChildNodeWithName(PackageType.ToString().ToLower())
                    .ChildNodeWithName("testblueprint")
                    .ChildNodesWithName("bpelement")
                    .FirstOrDefault(x => x.ValueForAttribute("elementtype")
                        .Equals("test", StringComparison.OrdinalIgnoreCase));
            var blueprintSegmentElements = Processor.ChildNodeWithName(PackageType.ToString().ToLower())
                .ChildNodeWithName("testblueprint")
                .ChildNodesWithName("bpelement")
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
            result.AddRange(CheckBpRef(Processor
                    .ChildNodeWithName(PackageType.ToString().ToLower())
                    .ChildNodeWithName("testblueprint")
                    .ChildNodesWithName("bpelement")
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
            var itemRefs =
                testItems.Select(x => x.ChildNodeWithName("identifier").ValueForAttribute("uniqueid")).ToList();
            foreach (var testForm in testForms)
            {
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
                                    Location = $"{PackageType}/testform/formpartition/itemgroup/passageref",
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
                            if (!itemRefs.Contains(groupItem.ValueForAttribute("itemid")))
                            {
                                result.Add(new ValidationError
                                {
                                    ErrorSeverity = ErrorSeverity.Degraded,
                                    Location = $"{PackageType}/testform/formpartition/itemgroup/groupitem",
                                    GeneratedMessage =
                                        $"[groupitem ID {groupItem.ValueForAttribute("itemid")} does not match any item in the item pool]",
                                    ItemId = groupItem.ValueForAttribute("itemid"),
                                    Key = "itemid",
                                    PackageType = PackageType,
                                    Value = groupItem.Navigator.OuterXml
                                });
                            }
                        }
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
    }
}