using System;
using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.AttributeValidation;
using TabulateSmarterTestAdminPackage.Common.Generic;
using TabulateSmarterTestAdminPackage.Utility;

namespace TabulateSmarterTestAdminPackage.Processors.Specification
{
    internal class IdentifierProcessor : IDisposable
    {
        private static readonly XPathExpression sXp_Name = XPathExpression.Compile("@name");
        private static readonly XPathExpression sXp_UniqueId = XPathExpression.Compile("@uniqueid");
        private static readonly XPathExpression sXp_Label = XPathExpression.Compile("@label");
        private static readonly XPathExpression sXp_Version = XPathExpression.Compile("@version");

        private readonly XPathNavigator _navigator;

        internal IdentifierProcessor(XPathNavigator navigator)
        {
            _navigator = navigator;
        }

        private string Name { get; set; }
        private string Version { get; set; }
        private string Label { get; set; }
        private string UniqueId { get; set; }

        public void Dispose()
        {}

        internal bool IsIdentifierValid()
        {
            return IsValidUniqueId()
                   && IsValidName()
                   && IsValidLabel()
                   && IsValidVersion();
        }

        internal bool IsValidUniqueId()
        {
            UniqueId = _navigator.Eval(sXp_UniqueId);
            if (UniqueId.NonemptyStringLessThanEqual(255))
            {
                return true;
            }

            AdminPackageUtility.ReportSpecificationError(_navigator.NamespaceURI, sXp_UniqueId.Expression, "string required [length<=255]");
            return false;
        }

        internal bool IsValidName()
        {
            Name = _navigator.Eval(sXp_Name);
            if (Name.NonemptyStringLessThanEqual(200))
            {
                return true;
            }

            AdminPackageUtility.ReportSpecificationError(_navigator.NamespaceURI, sXp_Name.Expression, "string required [length<=200]");
            return false;
        }

        internal bool IsValidLabel()
        {
            Label = _navigator.Eval(sXp_Label);
            if (Label.NonemptyStringLessThanEqual(200))
            {
                return true;
            }

            AdminPackageUtility.ReportSpecificationError(_navigator.NamespaceURI, sXp_Label.Expression, "string required [length<=200]");
            return false;
        }

        internal bool IsValidVersion()
        {
            Version = _navigator.Eval(sXp_Version);
            if (VersionValidation.IsValidVersionInt(Version))
            {
                return true;
            }
            AdminPackageUtility.ReportSpecificationError(_navigator.NamespaceURI, sXp_UniqueId.Expression, "int required [positive][length<=10]");
            return false;
        }
    }
}