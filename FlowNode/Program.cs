// See https://aka.ms/new-console-template for more information


using FlowNode;
using System.Xml.Linq;
using System.Xml;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using CargoTrucker;
using CargoTrucker.Client;

List<Node> nodes = new List<Node>();
List<Edge> edges = new List<Edge>();


//List<Tuple<string, string>> edges = new List<Tuple<string, string>>();


XmlDocument doc = new XmlDocument();
doc.Load("./data/cargotrucker1.drawio");
XmlNode rootNode = GetRootNode(doc.DocumentElement);  //doc.DocumentElement.SelectSingleNode("root");



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
        edges.Add(e);
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
            else
                n = new ActionNode();
            // TODO: OnTrue und OnFalse ordentlich setzen
        }
        
        n.ID = id;

        if (code != null)
        {
            code = code.Replace("&gt;", ">");
            code = code.Replace("&lt;", "<");
            if (code.Length > 0) code = code + ";";
        }

        n.Code = code;
        n.Parent = parent;
        nodes.Add(n);
    }
}

//return; 

//var filename = "./data/minmax.drawio";
//string text = File.ReadAllText(filename);
//foreach (var line in text.Split('\n'))
//{
//    if (line.Contains("<mxCell"))
//    {
//        Node n = new Node();
//        Edge e = new Edge();
//        var elements = line.Trim().Split(" ");
//        bool isEdge = false;

//        foreach (var element in elements)
//        {
//            if (element.Contains("id="))
//            {
//                var id = element.Split("=")[1].Replace("\"", "");
//                Console.WriteLine($"id={id}");
//                n.ID = id;
//                e.ID = id;
//            }
//            else if (element.Contains("parent="))
//            {
//                var parent = element.Split("=")[1].Replace("\"", "");
//                Console.WriteLine($"parent={parent}");
//                n.Parent = parent;
//            }
//            else if (element.Contains("value="))
//            {
//                var es = element.Split("=\"");
//                if (es.Length > 0)
//                {
//                    var code = es[1];
//                    code.Replace("\"", "");
//                    code = code.Replace("&amp;gt;", ">");
//                    //code = code.Replace("&amp;gt;", "<");
//                    if (code.Length > 0)
//                    {
//                        code = code + ";";
//                        Console.WriteLine($"code={code}");
//                        n.Code = code;
//                    }
//                }


//            }
//            else if (element.Contains("style="))
//            {
//                var style = element.Split("=")[1].Replace("\"", "");
//                Console.WriteLine($"style={style}");
//            }
//            else if (element.Contains("vertex="))
//            {
//                var vertex = element.Split("=")[1].Replace("\"", "");
//                Console.WriteLine($"vertex={vertex}");
//            }
//            else if (element.Contains("target="))
//            {
//                var target = element.Split("=")[1].Replace("\"", "");
//                Console.WriteLine($"target={target}");
//                e.Target = target;
//            }
//            else if (element.Contains("source="))
//            {
//                var source = element.Split("=")[1].Replace("\"", "");
//                Console.WriteLine($"source={source}");
//                e.Source = source;
//            }
//            else if (element.Contains("edge="))
//            {
//                var edge = element.Split("=")[1].Replace("\"", "");
//                Console.WriteLine($"edge={edge}");
//                isEdge = true;
//            }
//            //else if (element.Contains("/>"))
//            //{
//            //    nodes.Add(n);
//            //}
//        }
//        if (!isEdge)
//            nodes.Add(n);
//        else
//        {
//            edges.Add(e);
//        }
//    }
//}


foreach (var edge in edges)
{
    //var source = edge.Item1;
    //var target = edge.Item2;    
    Node parent = nodes.Select(x => x).Where(x => x.ID == edge.Source).FirstOrDefault();
    Node child = nodes.Select(x => x).Where(x => x.ID == edge.Target).FirstOrDefault();

    if (child != null && parent != null) child.Parent = parent.ID;
    if (parent != null) parent.Next = child;

}

foreach (var node in nodes)
{
    //Console.WriteLine($"ID={node.ID}, Parent={node.Parent}");
    Node parent = nodes.Select(x => x).Where(x => x.ID == node.Parent).FirstOrDefault();
    node.Parent = parent?.ID;
    if (parent is not null)
        parent.Next = node;
}

foreach (var dn in nodes)
{
    if (dn.GetType() == typeof(DecisionNode))
    {
        var outgoing = edges.Where(ed => ed.Source == dn.ID).ToList();
        var onTrue = outgoing.Where(ot => ot.Text == "Ja").FirstOrDefault().Target;
        var onFalse = outgoing.Where(ot => ot.Text == "Nein").FirstOrDefault().Target;
        var ldn = (dn as DecisionNode);
        if (ldn != null)
        {
            ldn.OnTrue = nodes.Where(y => y.ID == onTrue).FirstOrDefault();
            ldn.OnFalse = nodes.Where(y => y.ID == onFalse).FirstOrDefault();
        }
    }
}


//foreach (var node in nodes)
//{
//    Console.WriteLine($"ID={node.ID}, Next={node.Next.ID}");
//}



//int a = 5, b = 3;
//int min = 0, max = 0;



ScriptOptions scriptOptions = ScriptOptions.Default;

//Add reference to mscorlib
var mscorlib = typeof(System.Object).Assembly;
var systemCore = typeof(System.Linq.Enumerable).Assembly;
var cargoTrucker = typeof(CargoTrucker.Client.GameApi).Assembly;

scriptOptions = scriptOptions.AddReferences(mscorlib, systemCore, cargoTrucker);
//Add namespaces
scriptOptions = scriptOptions.AddImports("System");
scriptOptions = scriptOptions.AddImports("System.Linq");
scriptOptions = scriptOptions.AddImports("System.Collections.Generic");
scriptOptions = scriptOptions.AddImports("CargoTrucker.Client.GameApi");

var result = CSharpScript.RunAsync("Console.WriteLine(\"Starting Script\")", scriptOptions).Result;
ActionNode.ScriptState = result;
ActionNode.ScriptOptions = scriptOptions;

var primaryNode = nodes.Select(x => x).Where(x => x.ID == "WIyWlLk6GJQsqaUBKTNV-0").FirstOrDefault();
Node.Run(primaryNode);


XmlNode GetRootNode(XmlNode node)
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