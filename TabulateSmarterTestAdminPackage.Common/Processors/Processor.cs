using System;
using System.Collections.Generic;
using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.Utilities;

namespace TabulateSmarterTestAdminPackage.Common.Processors
{
    public abstract class Processor : IDisposable
    {
        protected readonly XPathNavigator Navigator;
        protected IList<Processor> Processors { get; } = new List<Processor>();

        protected AttributeValidationDictionary Attributes { get; set; }

        protected Processor(XPathNavigator navigator)
        {
            Navigator = navigator;
        }

        public void Dispose() {}

        public abstract bool Process();
    }
}