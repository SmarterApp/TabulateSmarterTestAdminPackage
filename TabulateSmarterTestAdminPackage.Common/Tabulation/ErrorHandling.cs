using System.Diagnostics;
using TabulateSmarterTestAdminPackage.Common.Enums;

namespace TabulateSmarterTestAdminPackage.Common.Tabulation
{
    public class ErrorHandling
    {
        public static void ReportError(CsvWriter writer, string errorFileName, string testName, ErrorSeverity severity, string itemId, string message, params object[] args)
        {
            if (writer == null)
            {
                writer = new CsvWriter(errorFileName, false);
                writer.Write(new[] {"TestName", "Severity", "ItemId", "Message"});
            }

            var outMessage = string.Format(message, args);
            writer.Write(new[] {testName, severity.ToString(), itemId, outMessage});
            Debug.Fail(outMessage);
        }
    }
}