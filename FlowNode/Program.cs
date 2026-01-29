// See https://aka.ms/new-console-template for more information

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

DrawIONetwork dion = new();
dion.Load("./data/cargotrucker1.drawio");

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

static XmlNode GetRootNode(XmlNode node)
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