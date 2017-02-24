using System;
using System.IO;

namespace TabulateSmarterTestPackage.Tabulators
{
    public interface ITabulator : IDisposable
    {
        void ProcessResult(Stream input);
    }
}