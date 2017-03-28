using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SmarterTestPackage.Common.Data;

namespace TabulateSmarterTestPackage.Utilities
{
    public static class MathHelper
    {
        public static OptionalStringOrErrorList CalculateAverageB(string measurementModel, string a, string b0,
            string b1, string b2, string b3,
            string scorePoints)
        {
            var result = new OptionalStringOrErrorList();

            int scorePointsInt;
            if (!int.TryParse(scorePoints, out scorePointsInt) || scorePointsInt == 0)
            {
                result.Errors.Add("[scorepoints parameter of itemscoredimension is invalid. Cannot calcualte avg_b]");
                return result;
            }

            if (measurementModel.Equals("IRT3PLn") && scorePointsInt > 1)
            {
                result.Errors.Add(
                    $"[scorepoints parameter of itemscoredimension {scorePointsInt} > # of b arguments for model IRT3PLn. Cannot calcualte avg_b]");
            }

            var bStrings = new List<string>();
            var bDoubles = new List<double>();
            if (!string.IsNullOrEmpty(b0))
            {
                double b0Double;
                if (!double.TryParse(b0, out b0Double))
                {
                    result.Errors.Add($"[b0 value {b0} is present, but invalid. Cannot calcualte avg_b]");
                }
                else
                {
                    bDoubles.Add(b0Double);
                }
                bStrings.Add(b0);
                if (!string.IsNullOrEmpty(b1))
                {
                    double b1Double;
                    if (!double.TryParse(b1, out b1Double) && scorePointsInt > 1)
                    {
                        result.Errors.Add($"[b1 value {b1} is present, but invalid. Cannot calcualte avg_b]");
                    }
                    else
                    {
                        bDoubles.Add(b1Double);
                    }
                    bStrings.Add(b1);
                    if (!string.IsNullOrEmpty(b2))
                    {
                        double b2Double;
                        if (!double.TryParse(b2, out b2Double) && scorePointsInt > 2)
                        {
                            result.Errors.Add($"[b2 value {b2} is present, but invalid. Cannot calcualte avg_b]");
                        }
                        else
                        {
                            bDoubles.Add(b2Double);
                        }
                        bStrings.Add(b2);
                        if (!string.IsNullOrEmpty(b3))
                        {
                            double b3Double;
                            if (!double.TryParse(b3, out b3Double) && scorePointsInt > 3)
                            {
                                result.Errors.Add($"[b3 value {b3} is present, but invalid. Cannot calcualte avg_b]");
                            }
                            else
                            {
                                bDoubles.Add(b3Double);
                            }
                            bStrings.Add(b3);
                        }
                    }
                }
                else
                {
                    result.Errors.Add("[There are no valid b values. Cannot calcualte avg_b]");
                }
            }

            if (a.Equals("1") && bStrings.All(b => b.Equals("1E-15")))
            {
                result.Errors.Add("[Uncalibrated item detected. Not calculating avg_b]");
            }

            if (scorePointsInt > bDoubles.Count)
            {
                result.Errors.Add(
                    $"[scorepoints parameter of itemscoredimension {scorePointsInt} > # of b arguments. Cannot calcualte avg_b]");
            }

            if (!result.Errors.Any())
            {
                result.Value = FormatHelper.FormatDouble(
                    (bDoubles.Take(scorePointsInt).Sum() / scorePointsInt)
                    .ToString(CultureInfo.InvariantCulture));
            }
            return result;
        }
    }
}