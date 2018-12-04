using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using NLog;
using ProcessSmarterTestPackage.Processors.Common;
using SmarterTestPackage.Common.Data;
using TabulateSmarterTestPackage.Tabulators;
using TabulateSmarterTestPackage.Utilities;

namespace TabulateSmarterTestPackage
{
    internal class Program
    {
        private const string HelpMessage =
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

            -ci <file path>
            If a content tabulator item output file is specified, input test packages will
            be cross-tabulated against that csv output file and any discrepencies will
            be reported.

            -cs <file path>
            If a content tabulator stimuli output file is specified, input test packages will
            be cross-tabulated against that csv output file and any discrepencies will
            be reported.

            -sv
            If this flag is present and both cs and ci arguments are provided, the tabulator will 
            write cross tabulation errors for stimuli versions";

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static void Main(string[] args)
        {
            Logger.Info(string.Concat(Enumerable.Repeat("-", 60)));
            Logger.Info("Test Package Tabulator Initialized");
            try
            {
                var inputFilenames = new List<string>();
                string oFilename = null;

                var help = false;

                for (var i = 0; i < args.Length; ++i)
                {
                    switch (args[i])
                    {
                        case "-h":
                            help = true;
                            Logger.Info("Application run in help mode");
                            break;

                        case "-i":
                        {
                            ++i;
                            if (i >= args.Length)
                            {
                                Logger.Error("-i option must be followed by a valid string filename");
                                throw new ArgumentException(
                                    "Invalid command line. '-i' option not followed by filename.");
                            }
                            Logger.Info($"Input found: {args[i]}");
                            inputFilenames.Add(args[i]);
                        }
                            break;

                        case "-o":
                        {
                            ++i;
                            if (i >= args.Length)
                            {
                                Logger.Error("-o option must be followed by a valid string filename");
                                throw new ArgumentException(
                                    "Invalid command line. '-o' option not followed by filename.");
                            }
                            if (oFilename != null)
                            {
                                Logger.Error($"Output filename already set to: {oFilename}, cannot set to {args[i]}");
                                throw new ArgumentException("Only one item output filename may be specified.");
                            }
                            oFilename = Path.GetFullPath(args[i]);
                            if (oFilename.EndsWith(".csv"))
                            {
                                oFilename = oFilename.Substring(0, oFilename.Length - 4);
                                Logger.Info($"Output filename set to: {args[i]}");
                            }
                        }
                            break;

                        case "-ci":
                            ++i;
                            if (i >= args.Length)
                            {
                                Logger.Error(
                                    "-ci option must be followed by a valid path to the \"ItemReport.csv\" output of the ContentPackageTabulator https://github.com/smarterapp/TabulateSmarterTestContentPackage");
                                throw new ArgumentException(
                                    "Invalid command line. '-ci' option not followed by filename.");
                            }
                            if (!File.Exists(args[i]) &&
                                !new FileInfo(args[i]).Extension.Equals(".csv", StringComparison.OrdinalIgnoreCase))
                            {
                                Logger.Error(
                                    "-ci option must be followed by a valid path to the \"ItemReport.csv\" output of the ContentPackageTabulator https://github.com/smarterapp/TabulateSmarterTestContentPackage");
                                throw new ArgumentException(
                                    "Invalid command line. '-ci' argument does not refer to valid csv output file.");
                            }
                            ReportingUtility.ContentItemDirectoryPath = args[i];
                            break;
                        case "-cs":
                            ++i;
                            if (i >= args.Length)
                            {
                                Logger.Error(
                                    "-cs option must be followed by a valid path to the \"StimulusReport.csv\" output of the ContentPackageTabulator https://github.com/smarterapp/TabulateSmarterTestContentPackage");
                                throw new ArgumentException(
                                    "Invalid command line. '-cs' option not followed by filename.");
                            }
                            if (!File.Exists(args[i]) &&
                                !new FileInfo(args[i]).Extension.Equals(".csv", StringComparison.OrdinalIgnoreCase))
                            {
                                Logger.Error(
                                    "-ci option must be followed by a valid path to the \"ItemReport.csv\" output of the ContentPackageTabulator https://github.com/smarterapp/TabulateSmarterTestContentPackage");
                                throw new ArgumentException(
                                    "Invalid command line. '-cs' argument does not refer to valid csv output file.");
                            }
                            ReportingUtility.ContentStimuliDirectoryPath = args[i];
                            break;
                        case "-sv":
                            UserSettings.ValidateStimuliVersion = true;
                            break;
                        default:
                            Logger.Error($"Unknown command line option '{args[i]}'. Use '-h' for syntax help.");
                            throw new ArgumentException(
                                $"Unknown command line option '{args[i]}'. Use '-h' for syntax help.");
                    }
                }

                if (!string.IsNullOrEmpty(ReportingUtility.ContentItemDirectoryPath) &&
                    !string.IsNullOrEmpty(ReportingUtility.ContentStimuliDirectoryPath))
                {
                    Logger.Info(
                        "Found valid paths to ContentPackageTabulator outputs. Initializing cross-processing validations...");
                    ReportingUtility.InitializeCrossProcessor();
                }
                else
                {
                    Logger.Warn(
                        "Unable to find valid paths to ContentPackageTabulator outputs. No cross-processing validations will be run.");
                }

                if (help || args.Length == 0)
                {
                    Console.WriteLine(HelpMessage);
                }

                else
                {
                    if (inputFilenames.Count == 0 || oFilename == null)
                    {
                        Logger.Error(
                            "Invalid command line. One output filename and at least one input filename must be specified.");
                        throw new ArgumentException(
                            "Invalid command line. One output filename and at least one input filename must be specified.");
                    }

                    using (var tabulator = new TestPackageTabulator(oFilename))
                    {
                        var results = inputFilenames.SelectMany(x => ProcessInputFilename(x, tabulator)).ToList();
                        TabulateResults(results, tabulator);
                    }
                }
            }
            catch (Exception err)
            {
                Logger.Fatal(err);
                Console.WriteLine();
#if DEBUG
                Console.WriteLine(err.ToString());
#else
                Console.WriteLine(err.Message);
#endif
                Console.ReadKey();
            }

            Console.Write("Press any key to exit.");
            Console.ReadKey(true);
        }

        private static IEnumerable<Processor> ProcessInputFilename(string filenamePattern,
            TestPackageTabulator tabulator)
        {
            Logger.Info($"Processing input file: {filenamePattern}");

            if (Directory.Exists(filenamePattern))
            {
                return ProcessDirectory(filenamePattern, tabulator);
            }

            var processors = new List<Processor>();

            var directory = Path.GetDirectoryName(filenamePattern);
            if (string.IsNullOrEmpty(directory))
            {
                directory = Environment.CurrentDirectory;
            }
            var pattern = Path.GetFileName(filenamePattern);

            foreach (var filename in Directory.GetFiles(directory, pattern))
            {
                if (Path.GetFileName(filename).StartsWith("."))
                {
                    Logger.Warn($"Skipping hidden input file: {filenamePattern}.");
                    continue;
                }
                if (!Path.HasExtension(pattern))
                {
                    Logger.Warn($"Input file: {filenamePattern} does not have an extension. Skipping...");
                    continue;
                }
                switch (Path.GetExtension(filename).ToLower())
                {
                    case ".xml":
                        var processor = ProcessInputXmlFile(filename, tabulator);
                        if (processor != null)
                        {
                            processors.Add(processor);
                        }
                        break;

                    case ".zip":
                        processors.AddRange(ProcessInputZipFile(filename, tabulator));
                        break;

                    default:
                        Logger.Warn(
                            $"Input '{filename}' is of unsupported type. Only directories, .xml, and .zip are supported.");
                        throw new ArgumentException(
                            $"Input file '{filename}' is of unsupported type. Only directories, .xml, and .zip are supported.");
                }
            }
            return processors;
        }

        private static void TabulateResults(List<Processor> processors, TestPackageTabulator tabulator)
        {
            tabulator.AddTabulationHeaders();
            if (ReportingUtility.CrossProcessor == null)
            {
                tabulator.TabulateResults(processors, new List<ProcessingError>());
            }
            else
            {
                ReportingUtility.CrossProcessor.TestPackages.Values.Where(x => x.Count == 1)
                    .SelectMany(x => x)
                    .ToList()
                    .ForEach(
                        x =>
                            tabulator.TabulateResults(new List<Processor> {x},
                                ReportingUtility.CrossProcessor.Errors[x.GetUniqueId()].Cast<ProcessingError>().ToList()));
                var crossTabulatedPackages =
                    ReportingUtility.CrossProcessor.TestPackages.Values.Where(x => x.Count > 1);
                foreach (var packageSets in crossTabulatedPackages)
                {
                    var scoringPackage = packageSets.FirstOrDefault(x => x.PackageType == PackageType.Scoring);
                    if (scoringPackage != null)
                    {
                        tabulator.TabulateResults(new List<Processor> {scoringPackage},
                            ReportingUtility.CrossProcessor.Errors[scoringPackage.GetUniqueId()].Cast<ProcessingError>()
                                .Where(x => x.PackageType == PackageType.Scoring).ToList());
                        continue;
                    }
                    var adminPackage = packageSets.FirstOrDefault(x => x.PackageType == PackageType.Administration);
                    if (adminPackage != null)
                    {
                        tabulator.TabulateResults(new List<Processor> {adminPackage},
                            ReportingUtility.CrossProcessor.Errors[adminPackage.GetUniqueId()].Cast<ProcessingError>()
                                .Where(x => x.PackageType == PackageType.Administration).ToList());
                    }
                }
            }
        }

        private static Processor ProcessInputXmlFile(string filename, TestPackageTabulator tabulator)
        {
            Logger.Info($"Processing: {filename}");
            using (var stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return ProcessStream(stream, tabulator);
            }
        }

        private static Processor ProcessStream(Stream stream, TestPackageTabulator tabulator)
        {
            Processor processor;
            try
            {
                 processor = tabulator.ProcessResult(stream);
            }
            catch (ArgumentException ex)
            {
                Logger.Error(ex.Message);
                return null;
            }

            if (ReportingUtility.CrossProcessor != null)
            {
                ReportingUtility.CrossProcessor.AddProcessedTestPackage(processor);
                ReportingUtility.CrossProcessor.AddCrossProcessingErrors(processor,
                ReportingUtility.CrossProcessor.ExecuteValidation());
            }
            return processor;
        }

        private static IEnumerable<Processor> ProcessInputZipFile(string filename,
            TestPackageTabulator tabulator)
        {
            Logger.Info($"Processing input zip file: {filename}");
            var processors = new List<Processor>();
            using (var zip = ZipFile.Open(filename, ZipArchiveMode.Read))
            {
                foreach (var entry in zip.Entries)
                {
                    if (string.IsNullOrEmpty(entry.Name) ||
                        !Path.GetExtension(entry.Name).Equals(".xml", StringComparison.OrdinalIgnoreCase))
                    {
                        Logger.Info($"File: {entry.Name} in zip file: {filename} is not a valid .xml file. Skipping...");
                        continue;
                    }
                    Logger.Info($"   Processing: {entry.FullName}");
                    using (var stream = entry.Open())
                    {
                        var processor = ProcessStream(stream, tabulator);
                        if (processor != null)
                        {
                            processors.Add(processor);
                        }
                    }
                }
            }
            Console.WriteLine();
            return processors;
        }

        private static IEnumerable<Processor> ProcessDirectory(string directoryName,
            TestPackageTabulator tabulator)
        {
            Logger.Info($"Processing input directory: {directoryName}");
            return new DirectoryInfo(directoryName)
                .EnumerateFiles()
                .Select(x => Path.Combine(directoryName, x.Name))
                .SelectMany(x => ProcessInputFilename(x, tabulator));
        }
    }
}