using System.Collections.Generic;
using System.Linq;
using ProcessSmarterTestPackage.Processors.Common;
using ProcessSmarterTestPackage.Processors.Common.ItemPool.Passage;
using ProcessSmarterTestPackage.Processors.Common.ItemPool.TestItem;
using SmarterTestPackage.Common.Data;
using ValidateSmarterTestPackage.RestrictedValues.Enums;

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
            CrossValidateContentStimuli(primary.ValueForAttribute("uniqueid"), passages.Cast<PassageProcessor>(),
                stimuliContent);
            return CrossValidateContentItems(primary.ValueForAttribute("uniqueid"), items.Cast<TestItemProcessor>(),
                itemContent);
        }

        private IList<CrossPackageValidationError> CrossValidateContentItems(string key,
            IEnumerable<TestItemProcessor> processors, List<Dictionary<string, string>> itemContent)
        {
            var errors = new List<CrossPackageValidationError>();
            foreach (var processor in processors)
            {
                var itemId = processor.ChildNodeWithName("identifier").ValueForAttribute("uniqueid");
                var item =
                    itemContent.FirstOrDefault(x => x["ItemId"].Equals(itemId));
                if (item == null)
                {
                    errors.Add(new CrossPackageValidationError
                    {
                        ErrorSeverity = ErrorSeverity.Severe,
                        GeneratedMessage = "[Item Does not exist in content package]",
                        ItemId = itemId,
                        Key = "ItemId",
                        Location = "Item Cross-Tabulation (Item Content Package)",
                        Path = $"testspecification/{processor.PackageType.ToString().ToLower()}/itempool/testitem",
                        PrimarySource = $"{key} - {processor.PackageType}",
                        SecondarySource = "Item Content Package",
                        TestName = key
                    });
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
                var stimuliId = processor.ChildNodeWithName("identifier").ValueForAttribute("uniqueid");
                var stimuli =
                    stimuliContent.FirstOrDefault(x => x["StimulusId"].Equals(stimuliId));
                if (stimuli == null)
                {
                    errors.Add(GenerateStimuliError("[Stimuli does not exist in content package]", stimuliId, processor,
                        key));
                }
                else
                {
                    var version = processor.ChildNodeWithName("identifier").ValueForAttribute("version");
                    if (!version.Equals(stimuli["Version"]))
                    {
                        errors.Add(GenerateStimuliError("[Stimuli version does not match content package]", stimuliId,
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
                ErrorSeverity = ErrorSeverity.Severe,
                GeneratedMessage = message,
                ItemId = id,
                Key = "StimuliId",
                Location = "Stimuli Cross-Tabulation (Stimuli Content Package)",
                Path = $"testspecification/{processor.PackageType.ToString().ToLower()}/itempool/passage",
                PrimarySource = $"{key} - {processor.PackageType}",
                SecondarySource = "Passage Content Package",
                TestName = key
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
                Location = "Item Cross-Tabulation (Item Content Package)",
                Path = $"testspecification/{processor.PackageType.ToString().ToLower()}/itempool/testitem",
                PrimarySource = $"{key} - {processor.PackageType}",
                SecondarySource = "Item Content Package",
                TestName = key
            };
        }
    }
}