﻿using System;
using System.Xml.XPath;

namespace TabulateSmarterTestAdminPackage.Processors.Specification
{
    internal class ItemPoolProcessor : Processor
    {
        private readonly XPathNavigator _navigator;

        internal ItemPoolProcessor(XPathNavigator navigator)
        {
            _navigator = navigator;
        }

        public override bool Process()
        {
            throw new NotImplementedException();
        }
    }
}