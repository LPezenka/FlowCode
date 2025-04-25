using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace XMLUtils
{
    public class XMLNode
    {
        public static XmlNode GetRootNode(XmlNode node)
        {
            if (node.Name == "root")
                return node;

            XmlNode result = null;
            foreach (XmlNode localNode in node.ChildNodes)
            {
                result = GetRootNode(localNode);
                if (result != null) return result;
            }
            return null;
        }
    }
}
