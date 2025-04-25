using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml;
using XMLUtils;

namespace FlowCodeInfrastructure
{
    public class DrawIONetwork:Network
    {
        public void Load(string filename)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(filename);
            XmlNode rootNode = XMLNode.GetRootNode(doc.DocumentElement);  //doc.DocumentElement.SelectSingleNode("root");

            foreach (XmlNode node in rootNode.ChildNodes)
            {
                string txt = node.InnerText; //or loop through its children as well
                string id = node.Attributes["id"]?.InnerText;
                string parent = node.Attributes["parent"]?.InnerText;
                string code = node.Attributes["value"]?.InnerText;
                string target = node.Attributes["target"]?.InnerText;
                string source = node.Attributes["source"]?.InnerText;
                string edge = node.Attributes["edge"]?.InnerText;
                string style = node.Attributes["style"]?.InnerText;
                if (edge != null)
                {
                    Edge e = new Edge();
                    e.ID = id;
                    e.Source = source;
                    e.Target = target;
                    e.Text = code;
                    Edges.Add(e);
                }
                else
                {
                    Node n = null;

                    if (style == null)
                        n = new ActionNode();
                    else
                    {
                        if (style.Contains("rhombus") || style.Contains("mxgraph.flowchart.decision"))
                            n = new DecisionNode();
                        else if (style.Contains("process"))
                            n = new CallerNode(null);
                        else if (style.Contains("terminator"))
                            n = new TerminatorNode();
                        else
                            n = new ActionNode();
                    }

                    n.ID = id;
                    if (n.GetType() == typeof(CallerNode))
                    {
                        code = code.Replace("</div>", "");
                        code = code.Replace("<div>", "");

                        //(n as CallerNode).Variables = rows.Where(x => x.Contains("Variables:")).FirstOrDefault().Split(":").Skip(1).FirstOrDefault();
                        var callerNode = (CallerNode)n;
                        callerNode.Variables = code.Split("(").Skip(1).FirstOrDefault();
                        callerNode.Variables = callerNode.Variables.Replace(")", "");
                        callerNode.ReturnSource = code.Split("=").Skip(1).FirstOrDefault().Trim();
                        callerNode.ReturnSource = callerNode.ReturnSource.Replace("()", "");

                        callerNode.ReturnTarget = code.Split("=").FirstOrDefault().Trim();
                    }
                    else if (n.GetType() == typeof(TerminatorNode))
                    {
                        code = code.Replace("return ", "");
                        (n as TerminatorNode).ResultVariable = code;
                    }

                    if (code != null)
                    {
                        code = code.Replace("&gt;", ">");
                        code = code.Replace("&lt;", "<");
                        if (code.Length > 0) code = code + ";";
                    }

                    n.Code = code;
                    n.Parent = parent;
                    Nodes.Add(n);
                }
            }

            foreach (var edge in Edges)
            {
                Node parent = Nodes.Select(x => x).Where(x => x.ID == edge.Source).FirstOrDefault();
                Node child = Nodes.Select(x => x).Where(x => x.ID == edge.Target).FirstOrDefault();

                if (child != null && parent != null) child.Parent = parent.ID;
                if (parent != null) parent.Next = child;
            }

            foreach (var node in Nodes)
            {
                //Console.WriteLine($"ID={node.ID}, Parent={node.Parent}");
                Node parent = Nodes.Select(x => x).Where(x => x.ID == node.Parent).FirstOrDefault();
                node.Parent = parent?.ID;
                if (parent is not null)
                    parent.Next = node;
            }

            foreach (var dn in Nodes)
            {
                if (dn.GetType() == typeof(DecisionNode))
                {
                    var outgoing = Edges.Where(ed => ed.Source == dn.ID).ToList();
                    var onTrue = outgoing.Where(ot => ot.Text == Config.GetKeyword(Config.KeyWord.True)).FirstOrDefault().Target;
                    var onFalse = outgoing.Where(ot => ot.Text == Config.GetKeyword(Config.KeyWord.False)).FirstOrDefault().Target;
                    var ldn = (dn as DecisionNode);
                    if (ldn != null)
                    {
                        ldn.OnTrue = Nodes.Where(y => y.ID == onTrue).FirstOrDefault();
                        ldn.OnFalse = Nodes.Where(y => y.ID == onFalse).FirstOrDefault();
                    }
                }
                else if (dn.GetType() == typeof(CallerNode))
                {
                    var target = Nodes.Where(x => x.Code != null && x.Code.Contains($"{Config.GetKeyword(Config.KeyWord.Function)} {(dn as CallerNode).ReturnSource}")).FirstOrDefault();
                    (dn as CallerNode).TargetNode = target;
                }
            }

        }

    }
}
