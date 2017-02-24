using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using TabulateSmarterTestPackage.Common.RestrictedValues.Enums;
using TabulateSmarterTestPackage.Common.Utilities;
using TabulateSmarterTestPackage.Tabulators;

namespace TabulateSmarterTestPackage
{
    internal class Program
    {
        private static readonly string sSyntax =
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

            -c <file path>
            If a content tabulator output file is specified, input test packages will
            be cross-tabulated against that csv output file and any discrepencies will
            be reported.";

        private static void Main(string[] args)
        {
            try
            {
                var inputFilenames = new List<string>();
                string oFilename = null;
                var packageType = PackageType.Administration;

                var help = false;

                for (var i = 0; i < args.Length; ++i)
                {
                    switch (args[i])
                    {
                        case "-h":
                            help = true;
                            break;

                        case "-i":
                        {
                            ++i;
                            if (i >= args.Length)
                            {
                                throw new ArgumentException(
                                    "Invalid command line. '-i' option not followed by filename.");
                            }
                            inputFilenames.Add(args[i]);
                        }
                            break;

                        case "-o":
                        {
                            ++i;
                            if (i >= args.Length)
                            {
                                throw new ArgumentException(
                                    "Invalid command line. '-o' option not followed by filename.");
                            }
                            if (oFilename != null)
                            {
                                throw new ArgumentException("Only one item output filename may be specified.");
                            }
                            oFilename = Path.GetFullPath(args[i]);
                            if (oFilename.EndsWith(".csv"))
                            {
                                oFilename = oFilename.Substring(0, oFilename.Length - 4);
                            }
                        }
                            break;

                        case "-c":
                            ++i;
                            if (i >= args.Length)
                            {
                                throw new ArgumentException(
                                    "Invalid command line. '-c' option not followed by filename.");
                            }
                            if (!File.Exists(args[i]) &&
                                !new FileInfo(args[i]).Extension.Equals(".csv", StringComparison.OrdinalIgnoreCase))
                            {
                                throw new ArgumentException(
                                    "Invalid command line. '-c' argument does not refer to valid csv output file.");
                            }
                            ReportingUtility.ContentDirectoryPath = args[i];
                            break;
                        default:
                            throw new ArgumentException(
                                $"Unknown command line option '{args[i]}'. Use '-h' for syntax help.");
                    }
                }

                if (help || args.Length == 0)
                {
                    Console.WriteLine(sSyntax);
                }

                else
                {
                    if (inputFilenames.Count == 0 || oFilename == null)
                    {
                        throw new ArgumentException(
                            "Invalid command line. One output filename and at least one input filename must be specified.");
                    }

                    using (var tabulator = new TestPackageTabulator(oFilename))
                    {
                        tabulator.ExpectedPackageType = packageType;

                        inputFilenames.AddRange(
                            inputFilenames.Where(Directory.Exists)
                                .SelectMany(x => new DirectoryInfo(x).GetFiles())
                                .Select(x => x.Name));

                        inputFilenames.ToList().ForEach(x => ProcessInputFilename(x, tabulator));
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

            if (ConsoleHelper.IsSoleConsoleOwner)
            {
                Console.Write("Press any key to exit.");
                Console.ReadKey(true);
            }
        }

        private static void ProcessInputFilename(string filenamePattern, ITabulator tabulator)
        {
            var count = 0;
            var directory = Path.GetDirectoryName(filenamePattern);
            if (string.IsNullOrEmpty(directory))
            {
                directory = Environment.CurrentDirectory;
            }
            var pattern = Path.GetFileName(filenamePattern);
            foreach (var filename in Directory.GetFiles(directory, pattern))
            {
                switch (Path.GetExtension(filename).ToLower())
                {
                    case ".xml":
                        ProcessInputXmlFile(filename, tabulator);
                        break;

                    case ".zip":
                        ProcessInputZipFile(filename, tabulator);
                        break;

                    default:
                        throw new ArgumentException(
                            $"Input file '{filename}' is of unsupported type. Only .xml and .zip are supported.");
                }
                ++count;
            }
            if (count == 0)
            {
                throw new ArgumentException($"Input file '{filenamePattern}' not found!");
            }
        }

        private static void ProcessInputXmlFile(string filename, ITabulator tabulator)
        {
            Console.WriteLine("Processing: " + filename);
            using (var stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                tabulator.ProcessResult(stream);
            }
            Console.WriteLine();
        }

        private static void ProcessInputZipFile(string filename, ITabulator tabulator)
        {
            Console.WriteLine("Processing: " + filename);
            using (var zip = ZipFile.Open(filename, ZipArchiveMode.Read))
            {
                foreach (var entry in zip.Entries)
                {
                    // Must not be folder (empty name) and must have .xml extension
                    if (!string.IsNullOrEmpty(entry.Name) &&
                        Path.GetExtension(entry.Name).Equals(".xml", StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine("   Processing: " + entry.FullName);
                        using (var stream = entry.Open())
                        {
                            tabulator.ProcessResult(stream);
                        }
                    }
                }
            }
            Console.WriteLine();
        }
    }
}