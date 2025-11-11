using FlowCodeInfrastructure;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace NodeControlPrototype
{
    internal class XMLWriter
    {
        public static void SaveXML(string path, List<EdgeControl> edges)
        {
            List<NodeControlBase> visitedNodes = new();
            XElement root = new XElement("VisualTreeNetwork");
            foreach (EdgeControl e in edges)
            {
                NodeControlBase from = e.From;
                NodeControlBase to = e.To;

                var potentialRoot = GenerateRoot(from);
                if (potentialRoot != null)
                {
                    root.Add(potentialRoot);
                }

                if (!visitedNodes.Contains(from))
                {
                    XElement xe = GenerateXML(from, edges);
                    root.Add(xe);
                    visitedNodes.Add(from);
                }

                potentialRoot = GenerateRoot(to);
                if (potentialRoot != null)
                {
                    root.Add(potentialRoot);
                }

                if (!visitedNodes.Contains(to))
                {
                    XElement xe = GenerateXML(to, edges);
                    root.Add(xe);
                    visitedNodes.Add(to);
                }

                XElement edgeElement = GenerateXML(e);
                root.Add(edgeElement);
            }
            root.Save(path);
        }

        public static XElement GenerateRoot(NodeControlBase node)
        {
            if (node.IsRoot == false) return null;

            XElement x = new XElement("Root");
            XAttribute id = new XAttribute("ID", node.NodeData.Id.ToString());
            x.Add(id);
            return x;
        }

        public static XElement GenerateXML(EdgeControl edge)
        {
            XElement x = new XElement("Edge");
            XAttribute from = new XAttribute("From", edge.From.NodeData.Id);
            XAttribute to = new XAttribute("To", edge.To.NodeData.Id);
            XAttribute label = new XAttribute("Label", edge.Label);
            XAttribute fromIndex = new XAttribute("FromIndex", edge.FromIndex);
            XAttribute toIndex = new XAttribute("ToIndex", edge.ToIndex);

            x.Add(toIndex);
            x.Add(fromIndex);
            x.Add(from);
            x.Add(to);
            x.Add(label);
            return x;
        }

        public static XElement GenerateXML(NodeControlBase node, List<EdgeControl> edges)
        {
            XElement x = new XElement("Node");
            XAttribute id = new XAttribute("ID", node.NodeData.Id.ToString());
            x.Add(id);
            XAttribute a = new XAttribute("Code", node.NodeData.Title);
            x.Add(a);
            XAttribute pos = new XAttribute("Position", node.NodeData.Position.ToString(CultureInfo.GetCultureInfo("en-US")));
            x.Add(pos);

            string type = "Sequence";

            if (node is DecisionNodeControl rcn)
            {
                type = "Decision";
                var nodeEdges = edges.Where(e => e.From == node).ToList();
                var yesEdge = nodeEdges.Where(e => e.Label == FlowCodeInfrastructure.Config.GetKeyword(Config.KeyWord.True)).FirstOrDefault();
                if (yesEdge is not null)
                {
                    var yesTo = yesEdge.To;
                    XAttribute yes = new XAttribute("OnTrue", yesTo.NodeData.Id);
                    x.Add(yes);
                }

                var noEdge = nodeEdges.Where(e => e.Label == FlowCodeInfrastructure.Config.GetKeyword(Config.KeyWord.False)).FirstOrDefault();
                if (noEdge is not null)
                {
                    var noTo = noEdge.To;
                    XAttribute no = new XAttribute("OnFalse", noTo.NodeData.Id);
                    x.Add(no);
                }

            }
            else if (node is SequenceNodeControl rc)
            {
                type = "Sequence";
            }
            else if (node is TerminalNodeControl tnc)
            {
                type = "Terminal";
                XAttribute functionName = new XAttribute("FunctionName", tnc.FunctionName);
                x.Add(functionName);
                XAttribute returnVariable = new XAttribute("ReturnVariable", tnc.ReturnVariable);
                x.Add(returnVariable);
                XAttribute inputVariables = new XAttribute("InputVariables", tnc.InputVariables);
                x.Add(inputVariables);
            }
            else if (node is ProcessNodeControl pn)
            {
                type = "PredefinedProcess";
                string call = pn.NodeData.Title;
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
