using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FlowCodeInfrastructure
{
    internal class XMLWriter
    {
        public static void SaveXML(Network network, string path)
        {
            List<Node> visitedNodes = new();
            XElement root = new XElement("VisualTreeNetwork");

            foreach (Node node in network.Nodes)
            {
                XElement xe = GenerateXML(node);
                root.Add(xe);
                visitedNodes.Add(node);
            }

            foreach (Edge edge in network.Edges)
            {
                XElement xe = GenerateXML(edge);
                root.Add(xe);
            }



            //foreach (Edge e in network.Edges)
            //{
            //    Node from = e.Source;
            //    Node to = e.Target;

            //    var potentialRoot = GenerateRoot(from);
            //    if (potentialRoot != null)
            //    {
            //        root.Add(potentialRoot);
            //    }

            //    if (!visitedNodes.Contains(from))
            //    {
            //        XElement xe = GenerateXML(from, edges);
            //        root.Add(xe);
            //        visitedNodes.Add(from);
            //    }

            //    potentialRoot = GenerateRoot(to);
            //    if (potentialRoot != null)
            //    {
            //        root.Add(potentialRoot);
            //    }

            //    if (!visitedNodes.Contains(to))
            //    {
            //        XElement xe = GenerateXML(to, edges);
            //        root.Add(xe);
            //        visitedNodes.Add(to);
            //    }

            //    XElement edgeElement = GenerateXML(e);
            //    root.Add(edgeElement);
            //}
            root.Save(path);
        }

        public static XElement GenerateRoot(Node node)
        {
            //if (node.IsRoot == false) return null;

            XElement x = new XElement("Root");
            XAttribute id = new XAttribute("ID", node.ID.ToString());
            x.Add(id);
            return x;
        }

        public static XElement GenerateXML(Edge edge)
        {
            XElement x = new XElement("Edge");
            XAttribute from = new XAttribute("From", edge.Source);
            XAttribute to = new XAttribute("To", edge.Target);
            XAttribute label = new XAttribute("Label", edge.Text);
            // TODO: Improve FromIndex and ToIndex
            XAttribute fromIndex = new XAttribute("FromIndex", 0);
            XAttribute toIndex = new XAttribute("ToIndex", 0);

            x.Add(toIndex);
            x.Add(fromIndex);
            x.Add(from);
            x.Add(to);
            x.Add(label);
            return x;
        }

        public static XElement GenerateXML(Node node)
        {
            XElement x = new XElement("Node");
            XAttribute id = new XAttribute("ID", node.ID.ToString());
            x.Add(id);
            XAttribute a = new XAttribute("Code", node.Code);
            x.Add(a);
            // TODO: Auto calculate Positions
            XAttribute pos = new XAttribute("Position", "0,0");
            x.Add(pos);

            string type = "Sequence";

            if (node is DecisionNode rcn)
            {
                type = "Decision";
                var yesNode = rcn.OnTrue; 
                if (yesNode is not null)
                {
                    XAttribute yes = new XAttribute("OnTrue", yesNode.ID);
                    x.Add(yes);
                }

                var noNode = rcn.OnFalse;
                if (noNode is not null)
                {
                    XAttribute no = new XAttribute("OnFalse", noNode.ID);
                    x.Add(no);
                }

            }
            else if (node is ActionNode rc)
            {
                type = "Sequence";
            }
            else if (node is TerminatorNode tnc)
            {
                type = "Terminal";
                XAttribute functionName = new XAttribute("FunctionName", tnc.Code);
                x.Add(functionName);
                XAttribute returnVariable = new XAttribute("ReturnVariable", tnc.ResultVariable);
                x.Add(returnVariable);
                XAttribute inputVariables = new XAttribute("InputVariables", tnc.InputVariables);
                x.Add(inputVariables);
            }
            else if (node is CallerNode pn)
            {
                type = "PredefinedProcess";
                string call = pn.Code;
                string fname = call.Split("(")[0];
                if (fname.Contains("="))
                    fname = fname.Split("=")[0].Trim();
                string variables = call.Split("(")[1].Split(")")[0];

                XAttribute target = new XAttribute("Target", fname);
                XAttribute variable = new XAttribute("Variables", variables);
            }

            XAttribute t = new XAttribute("Type", type);
            x.Add(t);

            return x;
        }

    }
}
