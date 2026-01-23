using Interfaces;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace FlowCodeInfrastructure
{
    public class CallerNode: ActionNode
    {
        public Node TargetNode { get; set; }
        public string Variables { get; set; }
        public string ReturnTarget { get; set; }
        public string ReturnSource { get; set; }

        public static ICallStack StackDisplay { get; set; }
        public CallerNode(Node target):base()
        {
            TargetNode = target;
        }

        /// <summary>
        /// Executes the evaluation logic for the current instance.
        /// </summary>
        public override void Evaluate()
        {
            Call();
        }

        /// <summary>
        /// Map full type names to C# short names
        /// Used for type inference in scripting
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        string MapToShortName(string typeName)
        {
            switch (typeName)
            {
                case "System.Boolean":
                    return "bool";
                case "System.Int32":
                    return "int";
                case "System.Char":
                    return "char";
                case "System.String":
                    return "string";
                case "System.Double":
                    return "double";
                case "System.Single":
                    return "float";
                default:
                    return typeName;
            }
        }

        /// <summary>
        /// Executes the target script node, initializing variables and handling return values as required by the node's
        /// configuration.
        /// </summary>
        /// <remarks>This method prepares the script execution environment by mapping variable values and
        /// types, then runs the target node's code. If the node produces a return value, it is assigned to the
        /// appropriate variable in the script state. The method also manages the execution stack display if
        /// present.</remarks>
        public void Call()
        {
            var state = ScriptState;
            var options = ScriptOptions;
            try
            {
                var vars = Variables.Split(",");
                string result = string.Empty;
                TerminatorNode terminatorNode = TargetNode as TerminatorNode;
                if (terminatorNode is null) return; // Improper function name

                StackDisplay?.Push(TargetNode.Code);

                for (int i = 0; i < vars.Length; i++)
                {
                    var v = vars[i];

                    var stateVariable = state.Variables.Where(x => x.Name == v.Trim()).FirstOrDefault();
                    if (stateVariable != null)
                    {
                        string type = stateVariable.Type.ToString();
                        string val = stateVariable.Value.ToString();
                        string name;
                        if (terminatorNode.InputVariables.Count > i)
                            name = terminatorNode.InputVariables[i];
                        else
                            name = stateVariable.Name;
 
                        if (stateVariable.Type.GetInterfaces().Contains(typeof(IList)))
                        {
                            Type itemType = stateVariable.Type.IsGenericType ? stateVariable.Type.GetGenericArguments()[0] : typeof(object);
                            type = $"System.Collections.Generic.List<{MapToShortName(itemType.FullName)}>";
                            IList list = (IList)stateVariable.Value;
                            string[] vs = new string[list.Count];
                            foreach (var item in list)
                                vs[list.IndexOf(item)] = item.ToString();

                            val = "[" + string.Join(",", vs) + "]";
                        }
                        else type = MapToShortName(type);

                        if (val == "True") val = "true";
                        else if (val == "False") val = "false";

                        if (type == "char") val = $"'{val}'";
                        else if (type == "string") val = $"\"{val}\"";

                        result += $"{type} {name} = {val};\n";
                    }
                }
                ScriptState = CSharpScript.RunAsync(result).Result;
                Run(TargetNode);
                //var rval = state.Variables.Where(x => x.Name == ReturnTarget);
                if (TerminatorNode.ReturnValue  != null)
                {
                    var existingVar = state.Variables.Where(x => x.Name == ReturnTarget).FirstOrDefault();
                    if (existingVar != null)
                    {

                        string parenthesis = string.Empty;
                        if (existingVar.Type == typeof(string))
                            parenthesis = "\"";

                            state = state.ContinueWithAsync($"{ReturnTarget} = {parenthesis}{TerminatorNode.ReturnValue}{parenthesis}").Result;
                    }
                    else
                    {
                        string parenthesis = string.Empty; 
                        if (TerminatorNode.ReturnValue.GetType() == typeof(string)) 
                            parenthesis = "\"";
                        state = state.ContinueWithAsync($"{TerminatorNode.ReturnValue.GetType()} {ReturnTarget} = {parenthesis}{TerminatorNode.ReturnValue}{parenthesis};").Result;
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            ScriptState = state;
            ScriptOptions = options;

            StackDisplay?.Pop();
            // TODO: Maybe clear the StackDisplay completely before entering the function, and reset after that.
        }

    }
}
