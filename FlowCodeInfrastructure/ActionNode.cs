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


        public string GetInputValue()
        {
            var v = ScriptState.Variables.Where(x => x.Name == "lineInput").FirstOrDefault();
            if (v != null)
                return v.Value.ToString();
            else
                return string.Empty;
        }

        public ScriptState InitVariable(string vVal, string varName)
        {
            //var v = ScriptState.Variables.Where(x => x.Name == "lineInput").FirstOrDefault();
            //var vVal = v.Value.ToString();
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
            return ScriptState;
        }

        public ScriptState UpdateValue(string varName, string varValue)
        {
            var existing = ScriptState.Variables.Where(x => x.Name == varName).FirstOrDefault();
            var t = existing.Type;
            string postProcess = string.Empty;
            switch (t)
            {
                case Type intType when intType == typeof(int):
                    postProcess = $"{varName} = int.Parse(\"{varValue}\");";
                    break;
                case Type floatType when floatType == typeof(float):
                    postProcess = $"{varName} = float.Parse(\"{varValue}v);";
                    break;
                case Type boolType when boolType == typeof(bool):
                    postProcess = $"{varName} = bool.Parse(\"{varValue}\");";
                    break;
                case Type charType when charType == typeof(char):
                    postProcess = $"{varName} = char.Parse(\"{varValue}\");";
                    break;
                case Type stringType when stringType == typeof(string):
                    postProcess = $"{varName} = {t.Name};";
                    break;
                case Type decimalType when decimalType == typeof(decimal):
                    postProcess = $"{varName} = decimal.Parse(\"{varValue}\");";
                    break;
            }
            ScriptState = ScriptState.ContinueWithAsync(postProcess, ScriptOptions).Result;
            return ScriptState;
        }

        public override void Evaluate()
        {
            if (Code == null)
            {
                return;
            }
            try
            {
                bool initVariable = false;
                bool updateValue = false;
                string varName = string.Empty;
                string originalCode = Code;

                if (Code.Contains("Ausgabe:"))
                {
                    Code = Code.Replace(";", "");
                    Code = Code.Replace("Ausgabe:", "Console.WriteLine(");
                    Code += ");";
                }
                else if (Code.Contains("Eingabe"))
                {
                    string[] parts = Code.Split(new[] { '=' });
                    if (parts.Length > 1)
                    {
                        varName = parts[0].Trim();
                        var existing = ScriptState.Variables.Where(x => x.Name == varName).FirstOrDefault();
                        if (existing == null)
                        {                            
                            Code = "lineInput = Console.ReadLine(); ";
                            if (ScriptState.Variables.Where(x => x.Name == "lineInput").FirstOrDefault() == null)
                                Code = "string " + Code;
                            initVariable = true;
                        }
                        else
                        {
                            Code = "lineInput = Console.ReadLine(); ";
                            if (ScriptState.Variables.Where(x => x.Name == "lineInput").FirstOrDefault() == null)
                                Code = "string " + Code;
                            updateValue = true;
                        }
                    }
                    //Code = Code.Replace(";", "");
                    //var v = ScriptState.Variables.Where(x => x.Name == varName).FirstOrDefault();
                    //string varType = "string;";
                }
                else if (Code.Contains("=") && !Code.Contains("=="))
                {
                    var parts = Code.Split("=");
                    if (parts.Length > 1)
                    {
                        varName = parts[0].Trim();
                        var existing = ScriptState.Variables.Where(x => x.Name == varName).FirstOrDefault();
                        if (existing == null)
                        {
                            Code = $"string lineInput = \"{parts[1]}\";";
                            initVariable = true;
                        }
 
                    }
                }

                if (Code.Contains("Function"))
                {
                    Console.WriteLine($"Entering {Code}");
                }
                else
                {
                    if (Code.EndsWith(';') == false) Code = $"{Code};";
                    ScriptState = ScriptState.ContinueWithAsync(Code, ScriptOptions).Result;
                    var existing = ScriptState.Variables.Where(x => x.Name == varName).FirstOrDefault();
                    if (initVariable && existing == null)
                        InitVariable(GetInputValue(), varName);
                    else if (updateValue && existing != null)
                        UpdateValue(varName, GetInputValue());

                    var lineInputVar = ScriptState.Variables.Where(x => x.Name == "lineInput").FirstOrDefault();
                    
                    //ScriptState.Variables.Remove(lineInputVar);
                }

                Code = originalCode;
            }
            catch(CompilationErrorException cee)
            {
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
