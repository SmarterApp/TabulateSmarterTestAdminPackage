using System;
using System.Xml.XPath;

namespace TabulateSmarterTestAdminPackage.Processors
{
    public abstract class Processor : IDisposable
    {
        public readonly XPathNavigator Navigator;

        public Processor(XPathNavigator navigator)
        {
            Navigator = navigator;
        }

        public void Dispose() {}

        public abstract bool Process();
    }
}