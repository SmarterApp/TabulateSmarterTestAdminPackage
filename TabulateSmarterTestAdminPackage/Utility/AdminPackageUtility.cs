using TabulateSmarterTestAdminPackage.Common.Enums;
using TabulateSmarterTestAdminPackage.Common.Tabulation;

namespace TabulateSmarterTestAdminPackage.Utility
{
    public static class AdminPackageUtility
    {
        internal static string ErrorFileName { get; set; }
        internal static string ItemFileName { get; set; }
        internal static string StimuliFileName { get; set; }
        internal static CsvWriter ErrorWriter { get; set; }
        internal static CsvWriter ItemWriter { get; set; }
        internal static CsvWriter StimuliWriter { get; set; }
        internal static ErrorHandling ErrorHandling { get; set; }
        internal static string TestName { get; set; }

        static AdminPackageUtility()
        {
            ErrorHandling = new ErrorHandling();
        }

        internal static void SetFileName(string fileName)
        {
            ErrorFileName = fileName + ".errors.csv";
            ItemFileName = fileName + ".items.csv";
            StimuliFileName = fileName + ".stims.csv";
            ErrorWriter = new CsvWriter(ErrorFileName, false);
            ItemWriter = new CsvWriter(ItemFileName, false);
            StimuliWriter = new CsvWriter(StimuliFileName, false);
        }

        public static void ReportError(string testName, ErrorSeverity severity, string itemId, string message, params object[] args)
        {
            ErrorHandling.ReportError(ErrorWriter, ErrorFileName, testName, severity, itemId, message, args);
        }

        public static void ReportSpecificationError(string path, string attribute, string violationMessage)
        {
            ReportError(TestName, ErrorSeverity.Degraded, string.Empty, $"{path} attribute {attribute} violates {violationMessage}");
        }
    }
}
