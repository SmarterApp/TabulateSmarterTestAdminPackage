using System.Collections.Generic;
using NLog;
using SmarterTestPackage.Common.Data;
using ValidateSmarterTestPackage.Resources;
using ValidateSmarterTestPackage.RestrictedValues.Enums;

namespace ValidateSmarterTestPackage.Validators.Combined
{
    public class ItemScoreDimensionValidator : ITestPackageValidator
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly string[] Models = new string[]
        {
            "IRT3PLN",
            "IRTPCL",
            "RAW",
            "RAWSCORE",
            "IRT3PL",
            "IRTGPC"
        };
        private static HashSet<string> RECOGNIZED_MEASUREMENT_MODELS = new HashSet<string>(Models);

        public void Validate(TestPackage testPackage, List<ValidationError> errors)
        {
            List<ItemGroupItemItemScoreDimension> itemScoreDimensions = new List<ItemGroupItemItemScoreDimension>();
            foreach (var test in testPackage.Test)
            {
                foreach (var segment in test.Segments)
                {
                    if (segment.algorithmType.Equals(Algorithm.FIXEDFORM))
                    {
                        var segmentForms =
                            segment.Item is TestSegmentSegmentForms forms ? forms.SegmentForm : null;
                        if (segmentForms != null)
                            foreach (var segmentForm in segmentForms)
                            {
                                foreach (var itemGroup in segmentForm.ItemGroup)
                                {
                                    foreach (var item in itemGroup.Item)
                                    {
                                        itemScoreDimensions.Add(item.ItemScoreDimension);
                                    }
                                }
                            }
                    }
                }
            }
            foreach (var itemItemScoreDimension in itemScoreDimensions)
            {
                if (!RECOGNIZED_MEASUREMENT_MODELS.Contains(itemItemScoreDimension.measurementModel.ToUpper()))
                {
                    var errStr =
                        $"An unrecognized measurement model {itemItemScoreDimension.measurementModel} was detected in the test package.";
                    Logger.Debug(errStr);
                    errors.Add(new ValidationError
                    {
                        ErrorSeverity = ErrorSeverity.Severe,
                        Location = "ItemScoreDimension",
                        GeneratedMessage = errStr,
                        ItemId = "none",
                        Key = "measurementModel",
                        PackageType = PackageType.Combined,
                        Value = itemItemScoreDimension.measurementModel
                    });
                }
            }
        }
    }
}