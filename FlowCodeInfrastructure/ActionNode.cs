using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Scripting;
using CargoTrucker;

namespace FlowCodeInfrastructure
{
    public class ActionNode : Node
    {
        //public string Code { get; set; }
        public static ScriptState ScriptState { get; set; }
        public static ScriptOptions ScriptOptions { get; set; }

        public override void Evaluate()
        {
            if (Code == null)
            {
                return;
            }
            try
            {
                bool initVariable = false;
                string varName = string.Empty;

                if (Code.Contains("Ausgabe:"))
                {
                    Code = Code.Replace(";", "");
                    Code = Code.Replace("Ausgabe:", "Console.WriteLine(");
                    Code += ");";
                }
                else if (Code.Contains("Eingabe"))
                {
                    string[] parts = Code.Split(new[] { '=' });
                    varName = parts[0].Trim();
                    Code = "string lineInput = Console.ReadLine(); ";
                    //Code = Code.Replace(";", "");
                    var v = ScriptState.Variables.Where(x => x.Name == varName).FirstOrDefault();
                    string varType = "string;";
                    initVariable = true;
                    //if (v is null)
                    //{
                    //    if (float.TryParse(lineInput, out float f)) varType = "float";
                    //    else if (int.TryParse(lineInput, out int intValue)) varType = "int";
                    //    else if (bool.TryParse(lineInput, out bool boolValue)) varType = "bool";
                    //    else if (char.TryParse(lineInput, out char charValue)) varType = "char";
                    //    else varType = "string";

                    //    Code += "int " + varName + "; ";
                    //}
                    
                    // TODO für verschiedene Datentypen implementieren oder Typ der Eingabe erkennen und Parsen
                    //Code += ");";
                     
                }

                if (Code.Contains("Function"))
                {
                    Console.WriteLine($"Entering {Code}");
                }
                else
                {
                    if (Code.EndsWith(';') == false) Code = $"{Code};";
                    ScriptState = ScriptState.ContinueWithAsync(Code, ScriptOptions).Result;
                    if (initVariable)
                    {
                        var v = ScriptState.Variables.Where(x => x.Name == "lineInput").FirstOrDefault();
                        var vVal = v.Value.ToString();
                        string varType = "string;";
                        if (int.TryParse(vVal, out int intValue)) varType = "int";
                        else if (float.TryParse(vVal, out float f)) varType = "float";
                        else if (bool.TryParse(vVal, out bool boolValue)) varType = "bool";
                        else if (char.TryParse(vVal, out char charValue)) varType = "char";
                        else varType = "string";
                        string postProcess = string.Empty;  
                        switch (varType)
                        {
                            case "float":
                                postProcess = $"{varType} {varName} = float.Parse(lineInput);";
                                break;
                            case "bool":
                                postProcess = $"{varType} {varName} = bool.Parse(lineInput);";
                                break;
                            case "char":
                                postProcess = $"{varType} {varName} = char.Parse(lineInput);";
                                break;
                            case "int":
                                postProcess = $"{varType} {varName} = int.Parse(lineInput);";
                                break;
                            default:
                                postProcess = $"string {varName} = lineInput;";
                                break;
                        }

                        ScriptState = ScriptState.ContinueWithAsync(postProcess, ScriptOptions).Result;

                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
