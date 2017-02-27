using SmarterTestPackage.Common.Data;

namespace TabulateSmarterTestPackage.Utilities
{
    public class ErrorHandling
    {
        private static bool _printHeaders = true;

        public static void ReportError(CsvWriter writer, string errorFileName, string testName, string path,
            ErrorSeverity severity,
            string itemId, string message, params object[] args)
        {
            if (writer == null)
            {
                writer = new CsvWriter(errorFileName, false);
            }

            if (_printHeaders)
            {
                writer.Write(new[] {"TestName", "Path", "Severity", "ItemId", "Message"});
                _printHeaders = false;
            }

            var outMessage = string.Format(message, args);
            writer.Write(new[] {testName, path, severity.ToString(), itemId, outMessage});
        }
    }
}