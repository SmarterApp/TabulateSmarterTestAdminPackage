using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.AttributeValidation;
using TabulateSmarterTestAdminPackage.Common.Generic;

namespace TabulateSmarterTestAdminPackage.Processors
{
    internal class IdentifierProcessor
    {
        private static readonly XPathExpression sXp_Name = XPathExpression.Compile("@name");
        private static readonly XPathExpression sXp_UniqueId = XPathExpression.Compile("@uniqueid");
        private static readonly XPathExpression sXp_Label = XPathExpression.Compile("@label");
        private static readonly XPathExpression sXp_Version = XPathExpression.Compile("@version");

        internal IdentifierProcessor(XPathNavigator navigator)
        {
            this.navigator = navigator;
        }

        private readonly XPathNavigator navigator;
        private string name { get; set; }
        private string version { get; set; }
        private string label { get; set; }
        private string uniqueId { get; set; }

        internal bool IsIdentifierValid()
        {
            return IsValidUniqueId() 
                && IsValidName() 
                && IsValidLabel() 
                && IsValidVersion();
        }

        internal bool IsValidUniqueId()
        {
            uniqueId = navigator.Eval(sXp_UniqueId);
            return uniqueId.NonemptyStringLessThanEqual(255);
        }

        internal bool IsValidName()
        {
            name = navigator.Eval(sXp_Name);
            return name.NonemptyStringLessThanEqual(200);
        }

        internal bool IsValidLabel()
        {
            label = navigator.Eval(sXp_Label);
            return label.NonemptyStringLessThanEqual(200);
        }

        internal bool IsValidVersion()
        {
            version = navigator.Eval(sXp_Version);
            return Version.IsValidVersionInt(version);
        }
    }
}
