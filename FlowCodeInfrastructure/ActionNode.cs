using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Scripting;
using CargoTrucker;
using Interfaces;
using System.Text.RegularExpressions;

namespace FlowCodeInfrastructure
{
    public class ActionNode : Node
    {
        public static IOutputHandler OutputHandler { get; set; }
        public static IInputHandler InputHandler { get; set; }
        //public string Code { get; set; }
        public static ScriptState ScriptState { get; set; }
        public static ScriptOptions ScriptOptions { get; set; }


        private (ScriptState, string) HandleAssignment(ScriptState scriptState, string code, ref bool initVariable, ref string varName)
        {
            // Jump out of method if there is no assignment in the code
            if (Regex.IsMatch(code, @"(?<!\=)\=(?!\=)") == false)
                return (scriptState, code);

            initVariable = false;
            var parts = code.Split("=");
            // TODO: something a = b == c doesn't work because of the split
            // fix that by finding the first occurence and manually taking substrings

            varName = parts[0].Trim();
            var filterName = varName; // Temp variable as refs cannot be used in Lamdas
            var v = ScriptState.Variables.Where(x => x.Name == filterName).FirstOrDefault();
            if (v is null)
            {
                // There is no variable whose name corresponds to the left side of the assignment
                // Infer type of right hand side, allocate variable and assign value
                (ScriptState, code) = HandleAssignmentToNew(ScriptState, code, ref initVariable, parts[1]);
            }
            return (scriptState, code);
        }

        private static (ScriptState, string) HandleAssignmentToNew(ScriptState scriptState, string code, ref bool initVariable, string rightHandPart)
        {
            var rightHandVariable = rightHandPart.Trim();
            var rhv = scriptState.Variables.Where(x => x.Name == rightHandVariable).FirstOrDefault();

            if (rhv is not null)
                code = $"string lineInput = {rightHandPart}.ToString();";
            else
            {
                //string pattern = @"^\s*[A-Za-z_][A-Za-z0-9_]*\s*(?:\+\+|--|[+\-*/]\s*[A-Za-z_0-9][A-Za-z0-9_]*)+\s*$";

                //if (Regex.IsMatch(code, pattern))
                if (rightHandPart.Contains("*")
                        || rightHandPart.Contains("+")
                        || rightHandPart.Contains("-")
                        || rightHandPart.Contains("/")
                        || rightHandPart.Contains("++")
                        || rightHandPart.Contains("--"))
                {
                    string rightHandCode = "var tv = " + rightHandPart + ";";
                    ScriptState tempState = scriptState.ContinueWithAsync(rightHandCode, ScriptOptions).Result;
                    var tv = tempState.Variables.Where(x => x.Name == "tv").FirstOrDefault();
                    if (tv is not null)
                    {
                        code = $"string lineInput = \"{tv.Value.ToString().Trim()}\"";
                        //scriptState
                        //code = //rightHandCode +
                        //System.Environment.NewLine +
                        //    $"string lineInput = tv.ToString()";
                    }
                    else
                        code = $"string lineInput = \"{rightHandPart.Trim()}\";";
                }
            }
            initVariable = true;
            return (ScriptState, code);
        }

        public override void Evaluate()
        {
            if (Code == null)
                return;

            try
            {
                string originalCode = Code;
                string code = originalCode;
                bool initVariable = false;
                bool customInput = false;
                bool customOutput = false;
                string outputText = string.Empty;
                string varName = string.Empty;
                (ScriptState, code) = HandleOutput(ScriptState, code, ref initVariable, ref customOutput, ref outputText, ref varName);
                (ScriptState, code) = HandleInput(ScriptState, code, ref initVariable, ref customInput, ref varName);
                (ScriptState, code) = HandleAssignment(ScriptState, code, ref initVariable, ref varName);

                // Temporary assignment to keep stuff working
                // TODO: change all Code to code
                //Code = code;

                if (code.Contains(Config.GetKeyword(Config.KeyWord.Function)))
                {
                    // TODO: Improve Logging. Simply writing to the console won't do
                    //Console.WriteLine($"Entering {Code}");
                }
                else
                {
                    // This is going to be a line of code for the scripting engine
                    if (!customInput && !customOutput && !code.EndsWith(';'))
                        code = $"{code};";

                    if (customInput)
                    {
                        code = "string lineInput = \"" + InputHandler.ReadInput("Bitte Wert eingeben: ") + "\";";
                    }

                    if (customOutput)
                    {
                        OutputHandler.ShowOutput(outputText);
                    }
                    else
                    {
                        // TODO: Consider moving this or find another way to get rid of lineInput
                        ScriptState = ScriptState.ContinueWithAsync(code, ScriptOptions).Result;
                    }
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
                        variableLogger?.LogVariables(ScriptState.Variables);
                        //Code = originalCode;
                    }
                }
            }
            catch (CompilationErrorException cee)
            {
                // TODO: log to file
                throw cee;
            }
            catch (Exception ex)
            {
                // TODO: log to file
                throw ex;
            }
        }

        private (ScriptState, string) HandleInput(ScriptState scriptState,string code, ref bool initVariable, ref bool customInput, ref string varName)
        {
            if (Code.Contains(Config.GetKeyword(Config.KeyWord.Input)) == false)
                return (scriptState, code);
            
            string[] parts = code.Split(['=']);
            varName = parts[0].Trim();
            var filterName = varName;
            if (InputHandler is null)
            {
                code = "string lineInput = Console.ReadLine(); ";
            }
            else
            {
                customInput = true;
            }

            var v = ScriptState.Variables.Where(x => x.Name == filterName).FirstOrDefault();
            string varType = "string;";
            initVariable = true;
            return (scriptState, code);
        }

        private (ScriptState, string) HandleOutput(ScriptState scriptState, string code, ref bool initVariable, ref bool customOutput, ref string outputText, ref string varName)
        {
            if (!code.Contains(Config.GetKeyword(Config.KeyWord.Output)))
                return (scriptState, code);

            if (OutputHandler is not null)
            {
                if (code.Contains("\"")) // This is simply a string
                {
                    outputText = Code.Split(":")[1].Trim().Replace("\"", "");
                }
                else
                {
                    var parts = Code.Split(new[] { ':' }, 2);
                    varName = parts[1].Trim();
                    var filterName = varName;
                    var v = ScriptState.Variables.Where(x => x.Name == filterName).FirstOrDefault();
                    if (v is not null)
                    {
                        outputText = v.Value.ToString();
                    }
                }
                customOutput = true;
            }
            else
            {
                code = code.Replace(";", "");
                code = code.Replace(Config.GetKeyword(Config.KeyWord.Output), "Console.WriteLine(");
                code += ");";
            }

            return (scriptState, code);
        }

    }
}
