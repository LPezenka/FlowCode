using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Scripting;

namespace FlowNode
{
    internal class ActionNode : Node
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
                ScriptState = ScriptState.ContinueWithAsync(Code, ScriptOptions).Result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
