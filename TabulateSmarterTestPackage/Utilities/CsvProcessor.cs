using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CsvHelper;

namespace TabulateSmarterTestPackage.Utilities
{
    public static class CsvProcessor
    {
        public static IEnumerable<T> Process<T>(Stream stream)
        {
            var csvReader = new CsvReader(new StreamReader(stream), new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true
            });
            csvReader.Read();
            csvReader.ReadHeader();
            return csvReader.GetRecords<T>();
        }
    }
}