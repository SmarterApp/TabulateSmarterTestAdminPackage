using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using TabulateSmarterTestAdminPackage.Common.Enums;

namespace TabulateSmarterTestAdminPackage
{
    class Program
    {

        static string sSyntax =
@"This tool tabulates the item metadata included in Smarter Balanced
Test Administration packages. It can also be used with Test Scoring
packages since they use the same format for the item sections.

Multiple input packages may be specified in which case the output is
the aggregation of all input files.

By default the tool processes only test administration packages as
determined by the @purpose attribute on the root <testspecification>
element in the file. All other .xml files are skipped. The -scoring
option will cause it to tabulate scoring packages and ignore all
other package types.

Command-line parameters:
 -i <input filename>
Specifies an input filename which should be the name of a test
administration or test reporting package. The Filename may include
wildcards in which case all matching files will be processed. If the
filename is a .zip then all .xml files in the .zip will be processed
regardless of their path within the .zip file. The -i parameter may be
repeated to process multiple input sources.

 -o <output filename>
Output filename prefix. The results are stored in the following files:
  filename.items.csv    Tabulation of item data
  filename.stims.csv    Tabulation of stimulus data
  filename.errors.csv   Tabulation of errors (if any)

-scoring
Causes the tabulator to tabulate scoring packages and ignore all other
package types. If this option is not specified, the tabulator tabulates
all administration packages and ignores all other package types.
";

        static void Main(string[] args)
        {
            try
            {
                List<string> inputFilenames = new List<string>();
                string oFilename = null;
                PackageType packageType = PackageType.Administration;
                
                bool help = false;

                for (int i=0; i<args.Length; ++i)
                {
                    switch (args[i])
                    {
                        case "-h":
                            help = true;
                            break;

                        case "-i":
                            {
                                ++i;
                                if (i >= args.Length) throw new ArgumentException("Invalid command line. '-i' option not followed by filename.");
                                inputFilenames.Add(args[i]);
                            }
                            break;

                        case "-o":
                            {
                                ++i;
                                if (i >= args.Length) throw new ArgumentException("Invalid command line. '-o' option not followed by filename.");
                                if (oFilename != null) throw new ArgumentException("Only one item output filename may be specified.");
                                oFilename = Path.GetFullPath(args[i]);
                                if (oFilename.EndsWith(".csv")) oFilename = oFilename.Substring(0, oFilename.Length - 4);
                            }
                            break;

                        case "-scoring":
                            packageType = PackageType.Scoring;
                            break;

                        default:
                            throw new ArgumentException(string.Format("Unknown command line option '{0}'. Use '-h' for syntax help.", args[i]));
                    }
                }

                if (help || args.Length == 0)
                {
                    Console.WriteLine(sSyntax);
                }

                else
                {
                    if (inputFilenames.Count == 0 || oFilename == null) throw new ArgumentException("Invalid command line. One output filename and at least one input filename must be specified.");

                    using (TestPackageProcessor processor = new TestPackageProcessor(oFilename))
                    {
                        processor.ExpectedPackageType = packageType;

                        foreach (string filename in inputFilenames)
                        {
                            ProcessInputFilename(filename, processor);
                        }
                    }
                }
            }
            catch (Exception err)
            {
                Console.WriteLine();
#if DEBUG
                Console.WriteLine(err.ToString());
#else
                Console.WriteLine(err.Message);
#endif
            }

            if (Win32Interop.ConsoleHelper.IsSoleConsoleOwner)
            {
                Console.Write("Press any key to exit.");
                Console.ReadKey(true);
            }
        }

        static void ProcessInputFilename(string filenamePattern, ITestResultProcessor processor)
        {
            int count = 0;
            string directory = Path.GetDirectoryName(filenamePattern);
            if (string.IsNullOrEmpty(directory)) directory = Environment.CurrentDirectory;
            string pattern = Path.GetFileName(filenamePattern);
            foreach (string filename in Directory.GetFiles(directory, pattern))
            {
                switch (Path.GetExtension(filename).ToLower())
                {
                    case ".xml":
                        ProcessInputXmlFile(filename, processor);
                        break;

                    case ".zip":
                        ProcessInputZipFile(filename, processor);
                        break;

                    default:
                        throw new ArgumentException(string.Format("Input file '{0}' is of unsupported time. Only .xml and .zip are supported.", filename));
                }
                ++count;
            }
            if (count == 0) throw new ArgumentException(string.Format("Input file '{0}' not found!", filenamePattern));
        }

        static void ProcessInputXmlFile(string filename, ITestResultProcessor processor)
        {
            Console.WriteLine("Processing: " + filename);
            using (FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                processor.ProcessResult(stream);
            }
            Console.WriteLine();
        }

        static void ProcessInputZipFile(string filename, ITestResultProcessor processor)
        {
            Console.WriteLine("Processing: " + filename);
            using (ZipArchive zip = ZipFile.Open(filename, ZipArchiveMode.Read))
            {
                foreach(ZipArchiveEntry entry in zip.Entries)
                {
                    // Must not be folder (empty name) and must have .xml extension
                    if (!string.IsNullOrEmpty(entry.Name) && Path.GetExtension(entry.Name).Equals(".xml", StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine("   Processing: " + entry.FullName);
                        using (Stream stream = entry.Open())
                        {
                            processor.ProcessResult(stream);
                        }
                    }
                }
            }
            Console.WriteLine();
        }

    }

    [Flags]
    enum DIDFlags : int
    {
        None = 0,
        Id = 1,             // Student ID
        Name = 2,           // Student Name
        Birthdate = 4,
        Demographics = 8,   // Sex, Race, Ethnicity
        School = 16          // School and districtID or ExternalSSID is unaffected
    }

    interface ITestResultProcessor : IDisposable
    {
        void ProcessResult(Stream input);
    }
}
