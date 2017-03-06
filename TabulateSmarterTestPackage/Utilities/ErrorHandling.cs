using SmarterTestPackage.Common.Data;

namespace TabulateSmarterTestPackage.Utilities
{
    public class ErrorHandling
    {
        public static void ReportError(CsvWriter writer, string errorFileName, string testName, PackageType packageType, string path,
            ErrorSeverity severity,
            string itemId, string message, params object[] args)
        {
            if (writer == null)
            {
                writer = new CsvWriter(errorFileName, false);
            }

            var outMessage = string.Format(message, args);
            writer.Write(new[] {testName, packageType.ToString(), severity.ToString(), path, itemId, outMessage});
        }
    }
}