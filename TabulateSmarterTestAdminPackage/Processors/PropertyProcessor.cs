using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.Generic;

namespace TabulateSmarterTestAdminPackage.Processors
{
    internal class PropertyProcessor
    {

        private static readonly XPathExpression sXp_Name = XPathExpression.Compile("@name");
        private static readonly XPathExpression sXp_Value = XPathExpression.Compile("@uniqueid");
        private static readonly XPathExpression sXp_Label = XPathExpression.Compile("@label");

        internal PropertyProcessor(XPathNavigator navigator)
        {
            this.navigator = navigator;
        }

        private readonly XPathNavigator navigator;
        private string name { get; set; }
        private string value { get; set; }
        private string label { get; set; }

        internal bool IsPropertyValid()
        {
            return IsValidName() 
                && IsValidValue() 
                && IsValidLabel();
        }

        internal bool IsValidName()
        {
            name = navigator.Eval(sXp_Name);
            return name.NonemptyStringLessThanEqual(200);
        }

        internal bool IsValidValue()
        {
            value = navigator.Eval(sXp_Value);
            return value.NonemptyStringLessThanEqual(200);
        }

        internal bool IsValidLabel()
        {
            label = navigator.Eval(sXp_Label);
            return label.NonemptyStringLessThanEqual(200);
        }
    }
}
