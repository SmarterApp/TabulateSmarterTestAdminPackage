using System.Collections.Generic;
using System.IO;
using System.Linq;

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
                var headers = fileRows[0].Split(',').Select(x => x.Trim('\t', '\n', '\r', ' ')).ToList();
                for (var i = 1; i < fileRows.Length; i++)
                {
                    var rowDictionary = new Dictionary<string, string>();
                    var row = fileRows[i].Split(',');

                    for (var j = 0; j < headers.Count() && j < row.Length; j++)
                    {
                        rowDictionary.Add(headers[j], row[j].Trim('\t', '\n', '\r', ' '));
                    }

                    result.Add(rowDictionary);
                }
            }
            return result;
        }
    }
}