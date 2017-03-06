using System.Collections.Generic;
using System.Linq;
using System.Xml.XPath;

namespace SmarterTestPackage.Common.Extensions
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

        public static List<XPathNavigator> GenerateList(this XPathNavigator navigator, string path)
        {
            return navigator
                .Select(path)
                .Cast<XPathNavigator>()
                .ToList();
        }
    }
}