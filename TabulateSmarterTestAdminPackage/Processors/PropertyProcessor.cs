using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.Generic;
using TabulateSmarterTestAdminPackage.Utility;

namespace TabulateSmarterTestAdminPackage.Processors
{
    internal class PropertyProcessor
    {

        private static readonly XPathExpression sXp_Name = XPathExpression.Compile("@name");
        private static readonly XPathExpression sXp_Value = XPathExpression.Compile("@uniqueid");
        private static readonly XPathExpression sXp_Label = XPathExpression.Compile("@label");

        internal PropertyProcessor(XPathNavigator navigator)
        {
            this._navigator = navigator;
        }

        private readonly XPathNavigator _navigator;
        private string Name { get; set; }
        private string Value { get; set; }
        private string Label { get; set; }

        internal bool IsPropertyValid()
        {
            return IsValidName() 
                && IsValidValue() 
                && IsValidLabel();
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

        internal bool IsValidValue()
        {
            Value = _navigator.Eval(sXp_Value);
            if(Value.NonemptyStringLessThanEqual(200))
            {
                return true;
            }
            AdminPackageUtility.ReportSpecificationError(_navigator.NamespaceURI, sXp_Value.Expression, "string required [length<=200]");
            return false;
        }

        internal bool IsValidLabel()
        {
            Label = _navigator.Eval(sXp_Label);
            if(Label.NonemptyStringLessThanEqual(200))
            {
                return true;
            }
            AdminPackageUtility.ReportSpecificationError(_navigator.NamespaceURI, sXp_Name.Expression, "string required [length<=200]");
            return false;
        }
    }
}
