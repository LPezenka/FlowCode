﻿// See https://aka.ms/new-console-template for more information

using System.Xml.Linq;
using System.Xml;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using CargoTrucker;
using CargoTrucker.Client;
using FlowCodeInfrastructure;

Config.SetKeyWord(Config.KeyWord.True, "Ja");
Config.SetKeyWord(Config.KeyWord.False, "Nein");
Config.SetKeyWord(Config.KeyWord.Function, "Function");

//List<Node> nodes = new List<Node>();
//List<Edge> edges = new List<Edge>();


DrawIONetwork dion = new();
dion.Load("./data/cargotrucker1.drawio");
//dion.Load("./data/primecheck.drawio");



//XmlDocument doc = new XmlDocument();
//doc.Load("./data/cargotrucker1.drawio");
//XmlNode rootNode = GetRootNode(doc.DocumentElement);  //doc.DocumentElement.SelectSingleNode("root");

//foreach (XmlNode node in rootNode.ChildNodes)
//{
//    string txt = node.InnerText; //or loop through its children as well
//    string id = node.Attributes["id"]?.InnerText;
//    string parent = node.Attributes["parent"]?.InnerText;
//    string code = node.Attributes["value"]?.InnerText;
//    string target = node.Attributes["target"]?.InnerText;
//    string source = node.Attributes["source"]?.InnerText;
//    string edge = node.Attributes["edge"]?.InnerText;
//    string style = node.Attributes["style"]?.InnerText;
//    if (edge != null)
//    {
//        Edge e = new Edge();
//        e.ID = id;
//        e.Source = source;
//        e.Target = target;
//        e.Text = code;
//        edges.Add(e);
//    }
//    else
//    {
//        Node n = null;

//        if (style == null)
//            n = new ActionNode();
//        else
//        {
//            if (style.Contains("rhombus") || style.Contains("mxgraph.flowchart.decision"))
//                n = new DecisionNode();
//            else if (style.Contains("process"))
//                n = new CallerNode(null);
//            else if (style.Contains("terminator"))
//                n = new TerminatorNode();
//            else
//                n = new ActionNode();
//        }
        
//        n.ID = id;
//        if (n.GetType() == typeof(CallerNode))
//        {
//            code = code.Replace("</div>", "");
//            code = code.Replace("<div>", "");

//            //(n as CallerNode).Variables = rows.Where(x => x.Contains("Variables:")).FirstOrDefault().Split(":").Skip(1).FirstOrDefault();
//            var callerNode = (CallerNode)n;
//            callerNode.Variables = code.Split("(").Skip(1).FirstOrDefault();
//            callerNode.Variables = callerNode.Variables.Replace(")", "");
//            callerNode.ReturnSource = code.Split("=").Skip(1).FirstOrDefault().Trim();
//            callerNode.ReturnSource = callerNode.ReturnSource.Replace("()", "");

//            callerNode.ReturnTarget = code.Split("=").FirstOrDefault().Trim();
//        }
//        else if (n.GetType() == typeof(TerminatorNode))
//        {
//            code = code.Replace("return ", "");
//            (n as TerminatorNode).ResultVariable = code;
//        }

//        if (code != null)
//        {
//            code = code.Replace("&gt;", ">");
//            code = code.Replace("&lt;", "<");
//            if (code.Length > 0) code = code + ";";
//        }

//        n.Code = code;
//        n.Parent = parent;
//        nodes.Add(n);
//    }
//}

//foreach (var edge in edges)
//{
//    //var source = edge.Item1;
//    //var target = edge.Item2;    
//    Node parent = nodes.Select(x => x).Where(x => x.ID == edge.Source).FirstOrDefault();
//    Node child = nodes.Select(x => x).Where(x => x.ID == edge.Target).FirstOrDefault();

//    if (child != null && parent != null) child.Parent = parent.ID;
//    if (parent != null) parent.Next = child;

//}

//foreach (var node in nodes)
//{
//    //Console.WriteLine($"ID={node.ID}, Parent={node.Parent}");
//    Node parent = nodes.Select(x => x).Where(x => x.ID == node.Parent).FirstOrDefault();
//    node.Parent = parent?.ID;
//    if (parent is not null)
//        parent.Next = node;
//}

//foreach (var dn in nodes)
//{
//    if (dn.GetType() == typeof(DecisionNode))
//    {
//        var outgoing = edges.Where(ed => ed.Source == dn.ID).ToList();
//        var onTrue = outgoing.Where(ot => ot.Text == Config.GetKeyword(Config.KeyWord.True)).FirstOrDefault().Target;
//        var onFalse = outgoing.Where(ot => ot.Text == Config.GetKeyword(Config.KeyWord.False)).FirstOrDefault().Target;
//        var ldn = (dn as DecisionNode);
//        if (ldn != null)
//        {
//            ldn.OnTrue = nodes.Where(y => y.ID == onTrue).FirstOrDefault();
//            ldn.OnFalse = nodes.Where(y => y.ID == onFalse).FirstOrDefault();
//        }
//    }
//    else if (dn.GetType() == typeof(CallerNode))
//    {
//        var target = nodes.Where(x => x.Code != null && x.Code.Contains($"{Config.GetKeyword(Config.KeyWord.Function)} {(dn as CallerNode).ReturnSource}")).FirstOrDefault();
//        (dn as CallerNode).TargetNode = target;
//    }
//}


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
result = result.ContinueWithAsync("int i = 0, j = 1; char a = 'a'; bool b = true, c = false;", scriptOptions).Result;
ActionNode.ScriptState = result;
ActionNode.ScriptOptions = scriptOptions;

var primaryNode = dion.Nodes.Select(x => x).Where(x => x.ID == "UQ2DM836Tyy0t1lEemUF-0").FirstOrDefault();
//var primaryNode = dion.Nodes.Select(x => x).Where(x => x.ID == "UQgl7eAVbDISpkBKBPCj-0").FirstOrDefault();

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