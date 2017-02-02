using System;

namespace TabulateSmarterTestAdminPackage.Processors
{
    public abstract class Processor : IDisposable
    {
        public void Dispose() {}

        public abstract bool Process();
    }
}