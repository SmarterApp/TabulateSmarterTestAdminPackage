using System.IO;

namespace TabulateSmarterTestPackage.Tabulators
{
    public abstract class Tabulator : ITabulator
    {
        public void Dispose() {}

        public abstract void ProcessResult(Stream input);
    }
}