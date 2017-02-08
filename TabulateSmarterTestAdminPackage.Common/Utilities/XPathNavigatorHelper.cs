using System.Xml.XPath;

namespace TabulateSmarterTestAdminPackage.Common.Utilities
{
    public static class XPathNavitagorHelper
    {
        public static string Eval(this XPathNavigator nav, XPathExpression expression)
        {
            if (expression.ReturnType == XPathResultType.NodeSet)
            {
                var nodes = nav.Select(expression);
                if (nodes.MoveNext())
                {
                    return nodes.Current.ToString();
                }
                return string.Empty;
            }
            return nav.Evaluate(expression).ToString();
        }
    }
}