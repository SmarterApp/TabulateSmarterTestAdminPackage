using System.Collections.Generic;
using System.IO;
using System.Linq;
using ProcessSmarterTestPackage.External;
using SmarterTestPackage.Common.Data;

namespace TabulateSmarterTestPackage.Utilities
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
        public static string ContentItemDirectoryPath { get; set; }
        public static string ContentStimuliDirectoryPath { get; set; }
        public static CrossProcessor CrossProcessor { get; set; }

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

        public static void InitializeCrossProcessor()
        {
            if (!string.IsNullOrEmpty(ContentItemDirectoryPath) && !string.IsNullOrEmpty(ContentStimuliDirectoryPath))
            {
                var items = new List<Dictionary<string, string>>();
                using (
                    var itemStream = new FileStream(ContentItemDirectoryPath, FileMode.Open, FileAccess.Read,
                        FileShare.Read))
                {
                    items = CsvProcessor.Process(itemStream);
                }

                var stimuli = new List<Dictionary<string, string>>();
                using (
                    var stimuliStream = new FileStream(ContentStimuliDirectoryPath, FileMode.Open, FileAccess.Read,
                        FileShare.Read))
                {
                    stimuli = CsvProcessor.Process(stimuliStream);
                }

                CrossProcessor = new CrossProcessor(items, stimuli);
            }
        }

        public static void ReportError(string testName, PackageType packageType, string path, ErrorSeverity severity,
            string itemId,
            string message, params object[] args)
        {
            ErrorHandling.ReportError(GetErrorWriter(), ErrorFileName, testName, path, severity,
                itemId.Split('-').Last(), message, args);
        }

        public static void Dispose(bool disposing)
        {
            if (ItemWriter != null)
            {
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