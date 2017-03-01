using System.Collections.Generic;
using System.IO;

namespace TabulateSmarterTestPackage.Utilities
{
    public static class CsvProcessor
    {
        public static List<Dictionary<string, string>> Process(Stream stream)
        {
            var result = new List<Dictionary<string, string>>();
            using (var reader = new StreamReader(stream))
            {
                var fileRows = reader.ReadToEnd().Split('\n');
                var headers = fileRows[0].Split(',');
                for (var i = 1; i < fileRows.Length; i++)
                {
                    var rowDictionary = new Dictionary<string, string>();
                    var row = fileRows[i].Split(',');
                    for (var j = 0; j < row.Length; j++)
                    {
                        rowDictionary.Add(headers[j], row[j]);
                    }
                    result.Add(rowDictionary);
                }
            }
            return result;
        }
    }
}