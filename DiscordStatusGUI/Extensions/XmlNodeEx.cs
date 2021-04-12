using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace DiscordStatusGUI.Extensions
{
    static class XmlNodeEx
    {
        public static XmlNode[] GetByClassName(this XmlNode node, string className)
        {
            return node.ChildNodes.Cast<XmlNode>().Where(xnode => 
                xnode.Attributes != null && 
                xnode.Attributes["class"] != null && 
                xnode.Attributes["class"].Value.Contains(className)).ToArray();
        }
    }
}
