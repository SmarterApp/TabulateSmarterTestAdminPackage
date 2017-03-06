using SmarterTestPackage.Common.Data;

namespace TabulateSmarterTestPackage.Utilities
{
    public class ErrorHandling
    {
        public static void ReportError(CsvWriter writer, string errorFileName, string testName, string path,
            ErrorSeverity severity,
            string itemId, string message, params object[] args)
        {
            if (writer == null)
            {
                writer = new CsvWriter(errorFileName, false);
            }

            var outMessage = string.Format(message, args);
            writer.Write(new[] {testName, severity.ToString(), path, itemId, outMessage});
        }
    }
}