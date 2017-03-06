using System;
using System.Collections.Generic;
using System.Linq;
using ProcessSmarterTestPackage.Processors.Common;
using ProcessSmarterTestPackage.Processors.Common.ItemPool.Passage;
using ProcessSmarterTestPackage.Processors.Common.ItemPool.TestItem;
using SmarterTestPackage.Common.Data;

namespace ProcessSmarterTestPackage.External
{
    public class ContentPackageCrossProcessor
    {
        public IList<CrossPackageValidationError> CrossValidateContent(TestSpecificationProcessor primary,
            List<Dictionary<string, string>> itemContent, List<Dictionary<string, string>> stimuliContent)
        {
            var itemPool =
                primary.ChildNodeWithName(primary.PackageType == PackageType.Administration
                    ? "administration"
                    : "scoring").ChildNodeWithName("itempool");
            var items = itemPool.ChildNodesWithName("testitem");
            var passages = itemPool.ChildNodesWithName("passage");
            var stimuliErrors = CrossValidateContentStimuli(primary.ValueForAttribute("uniqueid"),
                passages.Cast<PassageProcessor>(),
                stimuliContent);
            var itemErrors = CrossValidateContentItems(primary.ValueForAttribute("uniqueid"),
                items.Cast<TestItemProcessor>(),
                itemContent);
            var result = new List<CrossPackageValidationError>();
            result.AddRange(stimuliErrors);
            result.AddRange(itemErrors);
            return result;
        }

        private IEnumerable<CrossPackageValidationError> CrossValidateContentItems(string key,
            IEnumerable<TestItemProcessor> processors, List<Dictionary<string, string>> itemContent)
        {
            var errors = new List<CrossPackageValidationError>();
            foreach (var processor in processors)
            {
                var itemId = processor.ChildNodeWithName("identifier").ValueForAttribute("uniqueid").Split('-').Last();
                var item =
                    itemContent.FirstOrDefault(x => x.ContainsKey("ItemId") && x["ItemId"].Equals(itemId));
                if (item == null)
                {
                    errors.Add(GenerateItemError($"[Item {itemId} doesn't exist in content package]", itemId, processor,
                        key));
                    continue;
                }
                var poolPropertyProcessors = processor.ChildNodesWithName("poolproperty").ToList();
                var itemType = poolPropertyProcessors.FirstOrDefault(
                    x => x.ValueForAttribute("property").Equals("--ITEMTYPE--", StringComparison.OrdinalIgnoreCase));
                if (itemType != null &&
                    !item["ItemType"].Equals(itemType.ValueForAttribute("value"), StringComparison.OrdinalIgnoreCase))
                {
                    errors.Add(
                        GenerateItemError(
                            $"[ContentPackageItemType:{item["ItemType"]}!=TestPackageItemType{itemType.ValueForAttribute("value")}]",
                            itemId, processor, key));
                }
                var dok = poolPropertyProcessors.FirstOrDefault(
                    x =>
                        x.ValueForAttribute("property")
                            .Trim()
                            .Equals("Depth of Knowledge", StringComparison.OrdinalIgnoreCase));
                if (dok != null &&
                    !item["DOK"].Equals(dok.ValueForAttribute("value"), StringComparison.OrdinalIgnoreCase))
                {
                    errors.Add(
                        GenerateItemError(
                            $"[ContentPackageItemDOK:{item["DOK"]}!=TestPackageItemType{dok.ValueForAttribute("value")}]",
                            itemId, processor, key));
                }
                var grade = poolPropertyProcessors.FirstOrDefault(
                    x => x.ValueForAttribute("property").Equals("Grade", StringComparison.OrdinalIgnoreCase));
                if (grade != null &&
                    !item["Grade"].Equals(grade.ValueForAttribute("value"), StringComparison.OrdinalIgnoreCase))
                {
                    errors.Add(
                        GenerateItemError(
                            $"[ContentPackageItemGrade:{item["Grade"]}!=TestPackageItemType{grade.ValueForAttribute("value")}]",
                            itemId, processor, key));
                }
            }
            return errors;
        }

        private IList<CrossPackageValidationError> CrossValidateContentStimuli(string key,
            IEnumerable<PassageProcessor> processors, List<Dictionary<string, string>> stimuliContent)
        {
            var errors = new List<CrossPackageValidationError>();
            foreach (var processor in processors)
            {
                var stimuliId =
                    processor.ChildNodeWithName("identifier").ValueForAttribute("uniqueid").Split('-').Last();
                var stimuli =
                    stimuliContent.FirstOrDefault(x => x.ContainsKey("StimulusId") && x["StimulusId"].Equals(stimuliId));
                if (stimuli == null)
                {
                    var error = GenerateStimuliError($"[Stimuli:{stimuliId} doesn't exist in content package]",
                        stimuliId,
                        processor,
                        key);
                    error.ErrorSeverity = ErrorSeverity.Severe;
                    errors.Add(error);
                }
                else
                {
                    var version = processor.ChildNodeWithName("identifier").ValueForAttribute("version");
                    if (!version.Equals(stimuli["Version"]))
                    {
                        errors.Add(
                            GenerateStimuliError(
                                $"[StimuliVersion:{version}!=ContentPackageStimuliVersion:{stimuli["Version"]}]",
                                stimuliId,
                                processor, key));
                    }
                }
            }
            return errors;
        }

        private static CrossPackageValidationError GenerateStimuliError(string message, string id, Processor processor,
            string key)
        {
            return new CrossPackageValidationError
            {
                ErrorSeverity = ErrorSeverity.Benign,
                GeneratedMessage = message,
                ItemId = id,
                Key = "StimuliId",
                Location = $"testspecification/{processor.PackageType.ToString().ToLower()}/itempool/passage",
                Value = processor.Navigator.OuterXml,
                PrimarySource = $"{key} - {processor.PackageType}",
                SecondarySource = "Passage Content Package",
                AssessmentId = key
            };
        }

        private static CrossPackageValidationError GenerateItemError(string message, string id, Processor processor,
            string key)
        {
            return new CrossPackageValidationError
            {
                ErrorSeverity = ErrorSeverity.Severe,
                GeneratedMessage = message,
                ItemId = id,
                Key = "ItemId",
                Location = $"testspecification/{processor.PackageType.ToString().ToLower()}/itempool/testitem",
                Value = processor.Navigator.OuterXml,
                PrimarySource = $"{key} - {processor.PackageType}",
                SecondarySource = "Item Content Package",
                AssessmentId = key
            };
        }
    }
}