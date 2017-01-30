using TabulateSmarterTestAdminPackage.Common.Enums;
using TabulateSmarterTestAdminPackage.Common.Tabulation;

namespace TabulateSmarterTestAdminPackage.Utility
{
    public class AdminPackageUtility
    {
        internal string errorFileName { get; set; }
        internal string itemFileName { get; set; }
        internal string stimuliFileName { get; set; }
        internal CsvWriter errorWriter { get; set; }
        internal ErrorHandling errorHandling { get; set; }

        public AdminPackageUtility(string fileName)
        {
            errorFileName = fileName + ".errors.csv";
            itemFileName = fileName + ".items.csv";
            stimuliFileName = fileName + ".stims.csv";
            errorHandling = new ErrorHandling();
        }

        public void ReportError(string testName, ErrorSeverity severity, string itemId, string message, params object[] args)
        {
            errorHandling.ReportError(errorWriter, errorFileName, testName, severity, itemId, message, args);
        }
    }
}
