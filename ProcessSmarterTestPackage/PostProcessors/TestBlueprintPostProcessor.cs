using System;
using System.Collections.Generic;
using System.Linq;
using ProcessSmarterTestPackage.Processors.Common;
using SmarterTestPackage.Common.Data;

namespace ProcessSmarterTestPackage.PostProcessors
{
    public class TestBlueprintPostProcessor : PostProcessor
    {
        public TestBlueprintPostProcessor(PackageType packageType, Processor processor) : base(packageType, processor) {}

        public override IList<ValidationError> GenerateErrors()
        {
            var validationErrors = new List<ValidationError>();
            var elementTypes =
                Processor.ChildNodesWithName("bpelement").GroupBy(x => x.ValueForAttribute("elementtype")).ToList();

            var test = elementTypes.Where(x => x.Key.Equals("test", StringComparison.OrdinalIgnoreCase)).ToList();
            if (!test.Any())
            {
                validationErrors.Add(new ValidationError
                {
                    ErrorSeverity = ErrorSeverity.Severe,
                    GeneratedMessage = "[TestBlueprint element does not contain \"test\" elementtype]",
                    Key = "bpelement",
                    Location = "testblueprint/bpelement",
                    PackageType = PackageType
                });
            }
            else if (test.Count() > 1)
            {
                validationErrors.Add(new ValidationError
                {
                    ErrorSeverity = ErrorSeverity.Severe,
                    GeneratedMessage = "[TestBlueprint \"test\" elementtype count > 1]",
                    Key = "bpelement",
                    Location = "testblueprint/bpelement",
                    PackageType = PackageType,
                    Value = test.First().ToList().First().Navigator.OuterXml
                });
            }
            else
            {
                var segments =
                    elementTypes.Where(x => x.Key.Equals("segment", StringComparison.OrdinalIgnoreCase)).ToList();

                validationErrors.AddRange(CheckTestValues(test.SelectMany(x => x).First(), segments.SelectMany(x => x),
                    "minopitems"));
                validationErrors.AddRange(CheckTestValues(test.SelectMany(x => x).First(), segments.SelectMany(x => x),
                    "maxopitems"));
                validationErrors.AddRange(CheckTestValues(test.SelectMany(x => x).First(), segments.SelectMany(x => x),
                    "minftitems"));
                validationErrors.AddRange(CheckTestValues(test.SelectMany(x => x).First(), segments.SelectMany(x => x),
                    "maxftitems"));
                validationErrors.AddRange(CheckTestValues(test.SelectMany(x => x).First(), segments.SelectMany(x => x),
                    "opitemcount"));
                validationErrors.AddRange(CheckTestValues(test.SelectMany(x => x).First(), segments.SelectMany(x => x),
                    "ftitemcount"));
            }

            var contentLevelStrands =
                elementTypes.Where(
                    x =>
                        x.Key.Equals("contentlevel", StringComparison.OrdinalIgnoreCase) ||
                        x.Key.Equals("strand", StringComparison.OrdinalIgnoreCase)).ToList();
            var subContent =
                contentLevelStrands.SelectMany(x => x)
                    .Where(x => !string.IsNullOrEmpty(x.ValueForAttribute("parentid"))).ToList();
            foreach (var element in subContent)
            {
                var match = contentLevelStrands
                    .SelectMany(x => x)
                    .Any(parentCandidate => element.ValueForAttribute("parentid")
                        .Equals(parentCandidate.ChildNodeWithName("identifier")
                            .ValueForAttribute("uniqueid"), StringComparison.OrdinalIgnoreCase));
                if (!match)
                {
                    validationErrors.Add(new ValidationError
                    {
                        ErrorSeverity = ErrorSeverity.Degraded,
                        GeneratedMessage =
                            $"[TestBlueprint contentlevel element's parentid {element.ValueForAttribute("parentid")} does not exist]",
                        Key = "bpelement",
                        Location = "testblueprint/bpelement",
                        PackageType = PackageType,
                        Value = element.Navigator.OuterXml
                    });
                }
            }

            return validationErrors;
        }

        private IEnumerable<ValidationError> CheckTestValues(Processor test, IEnumerable<Processor> segments,
            string propertyName)
        {
            var result = new List<ValidationError>();
            if (segments.Sum(x => int.Parse(x.ValueForAttribute(propertyName))) !=
                int.Parse(test.ValueForAttribute(propertyName)))
            {
                result.Add(new ValidationError
                {
                    ErrorSeverity = ErrorSeverity.Degraded,
                    GeneratedMessage = $"[TestBlueprint test {propertyName} != sum of segment properties]",
                    Key = "bpelement",
                    Location = "testblueprint/bpelement",
                    PackageType = PackageType,
                    Value = test.Navigator.OuterXml
                });
            }
            return result;
        }
    }
}