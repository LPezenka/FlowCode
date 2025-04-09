using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowNode
{
    internal class TerminatorNode: ActionNode
    {
        public static object ReturnValue { get; set; }
        public static string ReturnName { get; set; }
        public string ResultVariable { get; set; }
        

        public override void Evaluate()
        {
            ReturnValue = ScriptState.Variables.Where(x => x.Name == ResultVariable).FirstOrDefault().Value;
            ReturnName = ResultVariable;
        }
    }
}
