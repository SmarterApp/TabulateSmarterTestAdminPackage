using System.Diagnostics;
using TabulateSmarterTestAdminPackage.Common.Enums;
using TabulateSmarterTestAdminPackage.Common.Tabulation;

namespace TabulateSmarterTestAdminPackage.Utility
{
    public static class AdminPackageUtility
    {
        static AdminPackageUtility()
        {
            ErrorHandling = new ErrorHandling();
        }

        internal static string ErrorFileName { get; set; }
        internal static string ItemFileName { get; set; }
        internal static string StimuliFileName { get; set; }
        internal static CsvWriter ErrorWriter { get; set; }
        internal static CsvWriter ItemWriter { get; set; }
        internal static CsvWriter StimuliWriter { get; set; }
        internal static ErrorHandling ErrorHandling { get; set; }
        internal static string TestName { get; set; }

        internal static void SetFileName(string fileName)
        {
            ErrorFileName = fileName + ".errors.csv";
            ItemFileName = fileName + ".items.csv";
            StimuliFileName = fileName + ".stims.csv";
        }

        public static void ReportError(string testName, ErrorSeverity severity, string itemId, string message, params object[] args)
        {
            if (ErrorWriter == null)
            {
                ErrorWriter = new CsvWriter(ErrorFileName, false);
            }
            ErrorHandling.ReportError(ErrorWriter, ErrorFileName, testName, severity, itemId, message, args);
            ErrorWriter.Dispose();
        }

        public static void ReportSpecificationError(string path, string attribute, string violationMessage)
        {
            ReportError(TestName, ErrorSeverity.Degraded, string.Empty, $"{path} attribute {attribute} violates {violationMessage}");
        }

        public static void Dispose(bool disposing)
        {
            if (ItemWriter != null)
            {
#if DEBUG
                if (!disposing)
                {
                    Debug.Fail("Failed to dispose TestPackageProcessor");
                }
#endif
                ItemWriter.Dispose();
                ItemWriter = null;
            }
            if (StimuliWriter != null)
            {
                StimuliWriter.Dispose();
                StimuliWriter = null;
            }
            if (ErrorWriter != null)
            {
                ErrorWriter.Dispose();
                ErrorWriter = null;
            }
        }
    }
}