using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using ProcessSmarterTestPackage.Processors.Combined;
using ProcessSmarterTestPackage.Processors.Common;
using ProcessSmarterTestPackage.Processors.Common.ItemPool.Passage;
using ProcessSmarterTestPackage.Processors.Common.ItemPool.TestItem;
using SmarterTestPackage.Common.Data;
using ValidateSmarterTestPackage;
using ValidateSmarterTestPackage.Validators;
using ValidateSmarterTestPackage.Validators.Convenience;

namespace ProcessSmarterTestPackage.External
{
    public class ContentPackageCrossProcessor
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public IList<CrossPackageValidationError> CrossValidateContent(Processor primary,
            IList<ContentPackageItemRow> itemContent, IList<ContentPackageStimRow> stimuliContent)
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
                items.Cast<TestItemProcessor>().ToList(),
                itemContent);
            var result = new List<CrossPackageValidationError>();
            result.AddRange(stimuliErrors);
            result.AddRange(itemErrors);
            return result;
        }

        public IList<CrossPackageValidationError> CrossValidateCombinedContent(Processor primary,
            IList<ContentPackageItemRow> itemContent, IList<ContentPackageStimRow> stimuliContent)
        {
            var result = new List<CrossPackageValidationError>();
            var combinedTestProcessor = (CombinedTestProcessor) primary;
            var items = combinedTestProcessor.GetItems();
            var processors = combinedTestProcessor.GetItemsAsProcessors();
            foreach (var proc in processors)
            {
                primary.Processors.Add(proc);
            }
            
            var itemErrors = CrossValidateCombinedContentItems(primary.ValueForAttribute("uniqueid"),
                items, itemContent, primary, processors);
            result.AddRange(itemErrors);
            return result;
        }

        private static IEnumerable<CrossPackageValidationError> CrossValidateCombinedContentItems(string key,
            IList<ItemGroupItem> items, IList<ContentPackageItemRow> itemContent, Processor processor, List<Processor> processors)
        {
            var errors = new List<CrossPackageValidationError>();
            foreach (var testItem in items)
            {
                var myProcessor = processors.Find(x =>
                    x.GetUniqueId().Equals(testItem.id, StringComparison.CurrentCultureIgnoreCase));


                // verify that each item id in the testpackage has a corresponding item in the cross content
                var itemId = testItem.id;
                var contentItem = itemContent.FirstOrDefault(x => x.ItemId.Equals(itemId));
                if (contentItem == null)
                {
                    Logger.Debug($"Found cross-tab error with Item id {itemId}. Item {itemId} doesn't exist in content package.");
                    errors.Add(GenerateCombinedItemError($"[Item {itemId} doesn't exist in content package]", itemId,
                        key, "ItemId"));
                    continue;
                }

                //verify the Item type matches what's in the cross content
                if (!testItem.type.Equals(contentItem.ItemType, StringComparison.OrdinalIgnoreCase))
                {
                    Logger.Debug($"Found cross-tab error with Item {testItem.id} type {testItem.type} does not match content item type {contentItem.ItemType}");
                    errors.Add(
                        GenerateCombinedItemError(
                            $"[ContentPackageItemType:{contentItem.ItemType}!=TestPackageItemType:{testItem.type}]",
                            itemId, key, "ItemType"));
                }

                var poolProperties = testItem.PoolProperties;

                //verify Item/PoolProperties/PoolProperty @name="Depth of Knowledge" == contentItem.DOK
                var dok = poolProperties.FirstOrDefault(
                    x =>
                        x.name
                            .Trim()
                            .Equals("Depth of Knowledge", StringComparison.OrdinalIgnoreCase));
                if (dok != null && !dok.value.Equals(contentItem.DOK, StringComparison.OrdinalIgnoreCase))
                {
                    Logger.Debug($"Found cross-tab error with Item {testItem.id} PoolProperty DOK {dok.value}");
                    errors.Add(
                        GenerateCombinedItemError(
                            $"[ContentPackageItemDOK:{contentItem.DOK}!=TestPackage PoolProperty{dok.value}]",
                            itemId, key, "ItemType"));
                }

                //verify Item/PoolProperties/PoolProperty @name="Grade" == contentItem.Grade
                var grade = poolProperties.FirstOrDefault(x => x.name.Trim().Equals("Grade", StringComparison.OrdinalIgnoreCase));
                if (grade != null && !grade.value.Equals(contentItem.Grade, StringComparison.OrdinalIgnoreCase))
                {
                    Logger.Debug($"Found cross-tab error with Item {testItem.id} PoolProperty Grade {grade.value}");
                    errors.Add(
                        GenerateCombinedItemError(
                            $"[ContentPackageItemGrade:{contentItem.Grade}!=TestPackage PoolProperty Grade{grade.value}]",
                            itemId, key, "Grade"));
                }

                //if contentItem has MathematicalPractice then validate that it is a Positive non empty with Length 1
                if (contentItem.MathematicalPractice.Length > 0)
                {
                     myProcessor.ValidatedAttributes.Add("MathematicalPractice", GenerateFromValidationCollection("MathematicalPractice", 
                           contentItem.MathematicalPractice, IntValidator.IsValidPositiveNonEmptyWithLength(1)));
                }

                // validate MaxPoints IsValidPositiveNonEmptyWithLength(10)
                if (contentItem.MaxPoints.Length > 0)
                {
                    myProcessor.ValidatedAttributes.Add("MaxPoints", GenerateFromValidationCollection("MaxPoints",
                        contentItem.MaxPoints, IntValidator.IsValidPositiveNonEmptyWithLength(10)));
                }

                // validate xml doc item and cross tab item match, that it is not empty, has length of 1 character, and that it is of value {Y y N n}
                var allowCalc = poolProperties.FirstOrDefault(x =>
                    x.name.Trim().Equals("Allow Calculator", StringComparison.OrdinalIgnoreCase));
                if (allowCalc != null && !allowCalc.value.Equals(contentItem.AllowCalculator) &&
                    !processors.First(x => x.Equals(myProcessor)).ValidatedAttributes.ContainsKey("AllowCalculator"))
                {
                    Logger.Debug($"Found cross-tab error with Item {testItem.id} PoolProperty AllowCalculator {allowCalc.value}");
                    myProcessor.ValidatedAttributes.Add("AllowCalculator",
                        GenerateFromValidationCollection("AllowCalculator", contentItem.AllowCalculator,
                            StringValidator.IsValidNonEmptyWithLength(1)
                                .AddAndReturn(new RequiredRegularExpressionValidator(ErrorSeverity.Degraded,
                                    @"^[YyNn]$"))
                        ));
                }

            }

            return errors;
        }

        private static IEnumerable<CrossPackageValidationError> CrossValidateContentItems(string key,
            IList<TestItemProcessor> processors, IList<ContentPackageItemRow> itemContent)
        {
            var errors = new List<CrossPackageValidationError>();
            foreach (var processor in processors)
            {
                var itemId = processor.ChildNodeWithName("identifier").ValueForAttribute("uniqueid").Split('-').Last();
                var item = itemContent.FirstOrDefault(x => x.ItemId.Equals(itemId));
                if (item == null)
                {
                    errors.Add(GenerateItemError($"[Item {itemId} doesn't exist in content package]", itemId, processor,
                        key, "ItemId"));
                    continue;
                }
                var poolPropertyProcessors = processor.ChildNodesWithName("poolproperty").ToList();
                var itemType = poolPropertyProcessors.FirstOrDefault(
                    x => x.ValueForAttribute("property").Equals("--ITEMTYPE--", StringComparison.OrdinalIgnoreCase));
                if (itemType != null &&
                    !item.ItemType.Equals(itemType.ValueForAttribute("value"), StringComparison.OrdinalIgnoreCase))
                {
                    errors.Add(
                        GenerateItemError(
                            $"[ContentPackageItemType:{item.ItemType}!=TestPackageItemType:{itemType.ValueForAttribute("value")}]",
                            itemId, processor, key, "ItemType"));
                }
                var dok = poolPropertyProcessors.FirstOrDefault(
                    x =>
                        x.ValueForAttribute("property")
                            .Trim()
                            .Equals("Depth of Knowledge", StringComparison.OrdinalIgnoreCase));
                if (dok != null &&
                    !item.DOK.Equals(dok.ValueForAttribute("value"), StringComparison.OrdinalIgnoreCase))
                {
                    errors.Add(
                        GenerateItemError(
                            $"[ContentPackageItemDOK:{item.DOK}!=TestPackageItemType{dok.ValueForAttribute("value")}]",
                            itemId, processor, key, "DOK"));
                }
                var grade = poolPropertyProcessors.FirstOrDefault(
                    x => x.ValueForAttribute("property").Equals("Grade", StringComparison.OrdinalIgnoreCase));
                if (grade != null &&
                    !item.Grade.Equals(grade.ValueForAttribute("value"), StringComparison.OrdinalIgnoreCase))
                {
                    errors.Add(
                        GenerateItemError(
                            $"[ContentPackageItemGrade:{item.Grade}!=TestPackageItemType{grade.ValueForAttribute("value")}]",
                            itemId, processor, key, "Grade"));
                }
                
                if (!string.IsNullOrEmpty(item.MathematicalPractice) &&
                    !processors.First(x => x.Equals(processor))
                        .ValidatedAttributes.ContainsKey("MathematicalPractice"))
                {
                    processors.First(x => x.Equals(processor))
                        .ValidatedAttributes.Add("MathematicalPractice",
                            GenerateFromValidationCollection("MathematicalPractice", item.MathematicalPractice,
                                IntValidator.IsValidPositiveNonEmptyWithLength(1)));
                }
                
                if (!string.IsNullOrEmpty(item.MaxPoints) &&
                    !processors.First(x => x.Equals(processor))
                        .ValidatedAttributes.ContainsKey("MaxPoints"))
                {
                    processors.First(x => x.Equals(processor))
                        .ValidatedAttributes.Add("MaxPoints",
                            GenerateFromValidationCollection("MaxPoints", item.MaxPoints,
                                IntValidator.IsValidPositiveNonEmptyWithLength(10)));
                }
                if (!string.IsNullOrEmpty(item.AllowCalculator) &&
                    !processors.First(x => x.Equals(processor))
                        .ChildNodesWithName("poolproperty")
                        .Any(
                            x =>
                                x.ValueForAttribute("property")
                                    .Equals("Allow Calculator", StringComparison.OrdinalIgnoreCase)) &&
                    !processors.First(x => x.Equals(processor))
                        .ValidatedAttributes.ContainsKey("AllowCalculator"))
                {
                    processors.First(x => x.Equals(processor))
                        .ValidatedAttributes.Add("AllowCalculator",
                            GenerateFromValidationCollection("AllowCalculator", item.AllowCalculator,
                                StringValidator.IsValidNonEmptyWithLength(1)
                                    .AddAndReturn(new RequiredRegularExpressionValidator(ErrorSeverity.Degraded,
                                        @"^[YyNn]$"))
                            ));
                }
            }
            return errors;
        }

        private static ValidatedAttribute GenerateFromValidationCollection(string name, string value,
            IValidator validators)
        {
            return new ValidatedAttribute
            {
                IsValid = validators.IsValid(value),
                Name = name,
                Validator = validators,
                Value = value
            };
        }

        private static IEnumerable<CrossPackageValidationError> CrossValidateContentStimuli(string key,
            IEnumerable<PassageProcessor> processors, IList<ContentPackageStimRow> stimuliContent)
        {
            var errors = new List<CrossPackageValidationError>();
            foreach (var processor in processors)
            {
                var stimuliId =
                    processor.ChildNodeWithName("identifier").ValueForAttribute("uniqueid").Split('-').Last();
                var stimuli =
                    stimuliContent.FirstOrDefault(x => x.StimulusId.Equals(stimuliId));
                if (stimuli == null)
                {
                    var error = GenerateStimuliError($"[Stimuli:{stimuliId} doesn't exist in content package]",
                        stimuliId,
                        processor,
                        key, "StimulusId");
                    error.ErrorSeverity = ErrorSeverity.Severe;
                    errors.Add(error);
                }
                else
                {
                    var version = processor.ChildNodeWithName("identifier").ValueForAttribute("version");
                    if (!version.Equals(stimuli.Version) && UserSettings.ValidateStimuliVersion)
                    {
                        errors.Add(
                            GenerateStimuliError(
                                $"[StimuliVersion:{version}!=ContentPackageStimuliVersion:{stimuli.Version}]",
                                stimuliId,
                                processor, key, "Version"));
                    }
                }
            }
            return errors;
        }

        private static CrossPackageValidationError GenerateStimuliError(string message, string id, Processor processor,
            string assessmentId, string key)
        {
            return new CrossPackageValidationError
            {
                ErrorSeverity = ErrorSeverity.Benign,
                GeneratedMessage = message,
                ItemId = id,
                Key = key,
                Location = $"testspecification/{processor.PackageType.ToString().ToLower()}/itempool/passage",
                Value = processor.Navigator.OuterXml,
                PrimarySource = $"{assessmentId} - {processor.PackageType}",
                SecondarySource = "Passage Content Package",
                AssessmentId = assessmentId,
                PackageType = processor.PackageType
            };
        }

        private static CrossPackageValidationError GenerateItemError(string message, string id, Processor processor,
            string assessmentId, string key)
        {
            return new CrossPackageValidationError
            {
                ErrorSeverity = ErrorSeverity.Severe,
                GeneratedMessage = message,
                ItemId = id,
                Key = key,
                Location = $"testspecification/{processor.PackageType.ToString().ToLower()}/itempool/testitem",
                Value = processor.Navigator.OuterXml,
                PrimarySource = $"{assessmentId} - {processor.PackageType}",
                SecondarySource = "Item Content Package",
                AssessmentId = assessmentId,
                PackageType = processor.PackageType
            };
        }

        private static CrossPackageValidationError GenerateCombinedItemError(string message, string id,
            string assessmentId, string key)
        {
            return new CrossPackageValidationError
            {
                ErrorSeverity = ErrorSeverity.Severe,
                GeneratedMessage = message,
                ItemId = id,
                Key = key,
                Location = $"TestPackage/Test/Segments/Segment/SegmentForms/Segment/ItemGroup/Item or TestPackage/Test/Segments/Segment/Pool/ItemGroup/Item",
                Value = id,
                PrimarySource = $"{assessmentId} - {PackageType.Combined}",
                SecondarySource = "Item Content Package",
                AssessmentId = assessmentId,
                PackageType = PackageType.Combined
            };
        }
    }
}