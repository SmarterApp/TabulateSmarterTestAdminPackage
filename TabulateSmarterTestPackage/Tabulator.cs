using System.IO;
using TabulateSmarterTestPackage.Tabulators;

namespace TabulateSmarterTestPackage
{
    public abstract class Tabulator : ITabulator
    {
        public void Dispose() {}

        public abstract void ProcessResult(Stream input);
    }
}