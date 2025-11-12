using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Scripting;
using CargoTrucker;
using Interfaces;

namespace FlowCodeInfrastructure
{
    public class ActionNode : Node
    {
        public static IOutputHandler OutputHandler { get; set; }
        public static IInputHandler InputHandler { get; set; }
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
                string originalCode = Code;
                bool initVariable = false;
                bool customInput = false;
                bool customOutput = false;
                string outputText = string.Empty;
                string varName = string.Empty;

                if (Code.Contains(Config.GetKeyword(Config.KeyWord.Output)))
                {
                    if (OutputHandler is not null)
                    {
                        if (Code.Contains("\""))
                        {
                            outputText = Code.Split(":")[1].Trim().Replace("\"", "");
                        }
                        else
                        {
                            var parts = Code.Split(new[] { ':' }, 2);
                            varName = parts[1].Trim();
                            var v = ScriptState.Variables.Where(x => x.Name == varName).FirstOrDefault();
                            if (v is not null)
                            {
                                outputText = v.Value.ToString();
                            }
                        }
                        customOutput = true;
                    }
                    else
                    {
                        Code = Code.Replace(";", "");
                        Code = Code.Replace("Ausgabe:", "Console.WriteLine(");
                        Code += ");";
                    }
                }
                else if (Code.Contains(Config.GetKeyword(Config.KeyWord.Input)))
                {

                    string[] parts = Code.Split(new[] { '=' });
                    varName = parts[0].Trim();
                    if (InputHandler is null)
                    {
                        Code = "string lineInput = Console.ReadLine(); ";
                    }
                    else
                    { 
                        customInput = true;
                    }
                    //Code = Code.Replace(";", "");
                    var v = ScriptState.Variables.Where(x => x.Name == varName).FirstOrDefault();
                    string varType = "string;";
                    initVariable = true;
                }
                else if (Code.Contains("=") && !Code.Contains("=="))
                {
                    var parts = Code.Split("=");
                    varName = parts[0].Trim();

                    var v = ScriptState.Variables.Where(x => x.Name == varName).FirstOrDefault();
                    if (v is null)
                    {
                        var rightHandVariable = parts[1].Trim();
                        var rhv = ScriptState.Variables.Where(x => x.Name == rightHandVariable).FirstOrDefault();
                        if (rhv is not null)
                            Code = $"string lineInput = {parts[1]}.ToString();";
                        else
                            Code = $"string lineInput = \"{parts[1]}\";";
                        initVariable = true;
                    }
                }

                if (Code.Contains(Config.GetKeyword(Config.KeyWord.Function)))
                {
                    Console.WriteLine($"Entering {Code}");
                }
                else
                {
                    if (Code.EndsWith(';') == false) Code = $"{Code};";
                    if (customInput)
                    {
                        Code = "string lineInput = \"" + InputHandler.ReadInput("Bitte Wert eingeben: ") + "\";";
                    }

                    if (customOutput)
                    {
                        OutputHandler.ShowOutput(outputText);
                    }
                    else
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
                        Code = originalCode;
                    }
                }
            }
            catch(CompilationErrorException cee)
            {
                Console.WriteLine(cee.ToString());
                //if (Code.Contains("="))
                //{
                //    var parts = Code.Split("=");
                //    var varname = parts[0];
                //}
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
