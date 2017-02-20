using System.Diagnostics;
using TabulateSmarterTestAdminPackage.Common.RestrictedValues.Enums;
using TabulateSmarterTestAdminPackage.Common.Tabulation;

namespace TabulateSmarterTestAdminPackage.Common.Utilities
{
    public static class ReportingUtility
    {
        internal static CsvWriter ErrorWriter;
        internal static CsvWriter ItemWriter;
        internal static CsvWriter StimuliWriter;

        static ReportingUtility()
        {
            ErrorHandling = new ErrorHandling();
        }

        public static string ErrorFileName { get; set; }
        public static string ItemFileName { get; set; }
        public static string StimuliFileName { get; set; }
        public static ErrorHandling ErrorHandling { get; set; }
        public static string TestName { get; set; }
        public static string ContentDirectoryPath { get; set; }

        public static CsvWriter GetItemWriter()
        {
            return ItemWriter ?? (ItemWriter = new CsvWriter(ItemFileName, false));
        }

        public static CsvWriter GetStimuliWriter()
        {
            return StimuliWriter ?? (StimuliWriter = new CsvWriter(StimuliFileName, false));
        }

        public static CsvWriter GetErrorWriter()
        {
            return ErrorWriter ?? (ErrorWriter = new CsvWriter(ErrorFileName, false));
        }

        public static void SetFileName(string fileName)
        {
            ErrorFileName = fileName + ".errors.csv";
            ItemFileName = fileName + ".items.csv";
            StimuliFileName = fileName + ".stims.csv";
        }

        public static void ReportError(string testName, string path, ErrorSeverity severity, string itemId,
            string message, params object[] args)
        {
            ErrorHandling.ReportError(GetErrorWriter(), ErrorFileName, testName, path, severity, itemId, message, args);
        }

        public static void ReportSpecificationError(string path, string attribute, string violationMessage)
        {
            ReportError(TestName, path, ErrorSeverity.Degraded, string.Empty,
                $"{path} attribute {attribute} violates {violationMessage}");
        }

        public static void ReportLoadError(string path, string attribute, string violationMessage)
        {
            ReportError(TestName, path, ErrorSeverity.Severe, string.Empty,
                $"{path} attribute {attribute} violates {violationMessage}");
        }

        public static void Dispose(bool disposing)
        {
            if (ItemWriter != null)
            {
                if (!disposing)
                {
                    Debug.Fail("Failed to dispose TestPackageProcessor");
                }
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