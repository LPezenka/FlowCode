using Microsoft.CodeAnalysis.Scripting;
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

        /// <summary>
        /// Handles assignments in the code line
        /// Differentiates between new variable assignments and existing variable assignments
        /// Skips compound assignments (+=, -=, *=, /=)
        /// </summary>
        /// <param name="scriptState"></param>
        /// <param name="code"></param>
        /// <param name="initVariable"></param>
        /// <param name="varName"></param>
        /// <returns></returns>
        private static (ScriptState, string) HandleAssignment(ScriptState scriptState, string code, ref bool initVariable, ref string varName)
        {

            if (Regex.IsMatch(code, @"(?<!\+)\+=(?!\=)") ||
                Regex.IsMatch(code, @"(?<!\-)\-=(?!\=)") ||
                Regex.IsMatch(code, @"(?<!\*)\*=(?!\=)") ||
                Regex.IsMatch(code, @"(?<!\/)\/=(?!\=)"))
            {
                return (scriptState, code);
            }

            // Jump out of method if there is no assignment in the code
            if (Regex.IsMatch(code, @"(?<!\=)\=(?!\=)") == false)
                return (scriptState, code);

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

        /// <summary>
        /// Handles assignment to a new variable.
        /// </summary>
        /// <param name="scriptState"></param>
        /// <param name="code"></param>
        /// <param name="initVariable"></param>
        /// <param name="rightHandPart"></param>
        /// <returns></returns>
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
                    }
                }
                else
                    code = $"string lineInput = \"{rightHandPart.Replace("\"", "").Trim()}\";";
            }
            initVariable = true;
            return (ScriptState, code);
        }

        /// <summary>
        /// 
        /// </summary>
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
                (ScriptState, code) = HandleOutput(ScriptState, code, ref customOutput, ref outputText, ref varName);
                (ScriptState, code) = HandleInput(ScriptState, code, ref initVariable, ref customInput, ref varName);
                (ScriptState, code) = HandleAssignment(ScriptState, code, ref initVariable, ref varName);

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

                    if (initVariable)
                    {
                        // First, execute the lineInput in a new State
                        ScriptState newState = ScriptState.ContinueWithAsync(code, ScriptOptions).Result;
                        var v = newState.Variables.Where(x => x.Name == "lineInput").FirstOrDefault();

                        var vVal = v?.Value.ToString();

                        var existing = ScriptState.Variables.Where(x => x.Name == varName).FirstOrDefault();

                        string postProcess = string.Empty;
                        postProcess = InferType(varName, vVal, out string vType);

                        if (existing is not null)
                            //Remove variable type, as it was already set in the past
                            postProcess = string.Join(" ", postProcess.Split(" ").Skip(1));

                        ScriptState = ScriptState.ContinueWithAsync(postProcess, ScriptOptions).Result;
                        //Code = originalCode;
                    }
                    else
                    {
                        ScriptState = ScriptState.ContinueWithAsync(code, ScriptOptions).Result;
                        //ScriptState.Variables.Remove(ScriptState.Variables.Where(ssv => ssv.Name == "lineInput").FirstOrDefault());
                    }
                }

                VariableLogger?.LogVariables(ScriptState.Variables);

            }
            catch (CompilationErrorException cee)
            {
                throw cee;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static string InferType(string varName, string? vVal, out string vType)
        {
            string varType; // = "string;";

            if (vVal is not null && vVal.Contains("["))
            {
                vVal = vVal.Replace("\"", "");
                varType = "List";

                int openingBracketIndex = vVal.IndexOf('[');
                int closingBracketIndex = vVal.IndexOf(']');
                int delimiterIndex = vVal.IndexOf(',', vVal.IndexOf('['));
                var first = vVal.Substring(openingBracketIndex + 1, Math.Min(closingBracketIndex, delimiterIndex) - 1);
                InferType("temp", first.Trim(), out string inferredType);
                vType = $"List<{inferredType}>";
            }
            else 
            {
                if (int.TryParse(vVal, out _)) varType = "int";
                else if (float.TryParse(vVal, out _)) varType = "float";
                else if (double.TryParse(vVal, out _)) varType = "double";
                else if (bool.TryParse(vVal, out _)) varType = "bool";
                else if (char.TryParse(vVal, out _)) varType = "char";
                else varType = "string";
                vType = varType;
            }

            string postProcess = varType switch
            {
                "float" => $"{varType} {varName} = float.Parse(\"{vVal}\");",
                "double" => $"{varType} {varName} = double.Parse(\"{vVal}\");",
                "bool" => $"{varType} {varName} = bool.Parse(\"{vVal}\");",
                "char" => $"{varType} {varName} = char.Parse(\"{vVal}\");",
                "int" => $"{varType} {varName} = int.Parse(\"{vVal}\");",
                "List" => $"{vType} {varName} = {vVal};",
                _ => $"string {varName} = \"{vVal}\";",
            };
            return postProcess;
        }

        private (ScriptState, string) HandleInput(ScriptState scriptState, string code, ref bool initVariable, ref bool customInput, ref string varName)
        {
            if (Code.Contains(Config.GetKeyword(Config.KeyWord.Input)) == false)
                return (scriptState, code);

            string[] parts = code.Split(['=']);
            varName = parts[0].Trim();
            var filterName = varName;
            if (InputHandler is null)
            {
                code = "string lineInput = Console.ReadLine();";
            }
            else
            {
                customInput = true;
            }

            var v = ScriptState.Variables.Where(x => x.Name == filterName).FirstOrDefault();
            //string varType = "string;";
            initVariable = true;
            return (scriptState, code);
        }

        private (ScriptState, string) HandleOutput(ScriptState scriptState, string code, ref bool customOutput, ref string outputText, ref string varName)
        {
            if (!code.Contains(Config.GetKeyword(Config.KeyWord.Output)))
                return (scriptState, code);

            if (OutputHandler is not null)
            {
                if (code.Contains('\"')) // This is simply a string
                {
                    outputText = code.Split(":")[1].Trim().Replace("\"", "");
                }
                else
                {
                    varName = HandleVariableOutput(ref outputText);
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

        private string HandleVariableOutput(ref string outputText)
        {
            string varName;
            string[] parts = Code.Split(new[] { ':' }, 2);
            varName = parts[1].Trim();
            var filterName = varName;
            var v = ScriptState.Variables.Where(x => x.Name == filterName).FirstOrDefault();
            if (v is not null)
            {
                outputText = v.Value.ToString();
            }

            return varName;
        }
    }
}
