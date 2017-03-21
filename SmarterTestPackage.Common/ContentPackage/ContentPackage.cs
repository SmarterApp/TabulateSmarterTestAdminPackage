using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SmarterTestPackage.Common.ContentPackage
{
    public class ContentPackage : List<Dictionary<string, string>>
    {
        public void LoadContentPackageTabulationResults(Stream tabulatedContentPackageStream)
        {
            using (var reader = new StreamReader(tabulatedContentPackageStream))
            {
                var csv = reader.ReadToEnd().Split('\n');
                var headers = csv[0].Split(',');
                for (var i = 1; i < csv.Length; i++)
                {
                    var row = csv[i].Split(',');
                    var contentEntry = new Dictionary<string, string>();
                    for (var j = 0; j < csv.First().Length; j++)
                    {
                        contentEntry.Add(headers[j], row[j]);
                    }
                    Add(contentEntry);
                }
            }
        }
    }
}