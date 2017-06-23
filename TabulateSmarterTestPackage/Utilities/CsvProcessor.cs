﻿using System.Collections.Generic;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;

namespace TabulateSmarterTestPackage.Utilities
{
    public static class CsvProcessor
    {
        public static IEnumerable<T> Process<T>(Stream stream)
        {
            var csvReader = new CsvReader(new StreamReader(stream), new CsvConfiguration
            {
                HasHeaderRecord = true
            });
            csvReader.ReadHeader();
            return csvReader.GetRecords<T>();
        }
    }
}