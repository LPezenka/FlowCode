using Microsoft.CodeAnalysis.CSharp.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowNode
{
    internal class DecisionNode:ActionNode
    {
        public Node OnTrue { get; set; }
        public Node OnFalse { get; set; }
        public override void Evaluate()
        {
            if (Code == null) return;
            try
            {
                Code = "bool bresult = (" + Code.Replace(";","") + ");";
                Code.Replace(";;", ";");

                ScriptState = ScriptState.ContinueWithAsync<bool>(Code, ScriptOptions).Result;

                var v = ScriptState.Variables.Select(va => va).Where(vn => vn.Name == "bresult").FirstOrDefault();
                if ((bool)v.Value == true)
                    Next = OnTrue;
                else
                    Next = OnFalse;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());   
            }
        }
    }
}
