using Interfaces;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
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

        public override void Evaluate()
        {
            Call();
        }

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

                //TODO: Refresh Call Stack
                StackDisplay?.Push(TargetNode.Code);

                for (int i = 0; i < vars.Length; i++)
                {
                    var v = vars[i];
                    //var tv = terminatorNode.InputVariables[i];

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

                        //string name = tv; // stateVariable.Name.ToString();

                        if (type == "System.Boolean") type = "bool";
                        else if (type == "System.Int32") type = "int";
                        else if (type == "System.Char") type = "char";

                        if (val == "True") val = "true";
                        else if (val == "False") val = "false";

                        if (type == "char") val = $"'{val}'";

                        result += $"{type} {name} = {val};\n";
                    }
                }
                ScriptState = CSharpScript.RunAsync(result).Result;
                Run(TargetNode);
                //var rval = state.Variables.Where(x => x.Name == ReturnTarget);
                if (TerminatorNode.ReturnValue  != null)
                {
                    //var o = TerminatorNode.ReturnValue;
                    var existingVar = state.Variables.Where(x => x.Name == ReturnTarget).FirstOrDefault();
                    if (existingVar != null)
                    {
                        //var idx = state.Variables.IndexOf(existingVar);
                        //state.Variables[idx] = o;
                        state = state.ContinueWithAsync($"{ReturnTarget} = {TerminatorNode.ReturnValue}").Result;
                    }
                    else
                    {
                        state = state.ContinueWithAsync($"{TerminatorNode.ReturnValue.GetType()} {ReturnTarget} = {TerminatorNode.ReturnValue};").Result;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            //finally
            //{
            ScriptState = state;
            ScriptOptions = options;

            //TODO: Refresh Call Stack
            StackDisplay?.Pop();

            //Run(Next);
            //}
        }

    }
}
