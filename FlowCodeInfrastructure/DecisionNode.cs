using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowCodeInfrastructure
{
    public class DecisionNode : ActionNode
    {
        public Node OnTrue { get; set; }
        public Node OnFalse { get; set; }
        public override void Evaluate()
        {
            if (Code == null) return;
            //try
            //{
                //Code = "bool bresult = (" + Code.Replace(";","") + ");";
                //Code.Replace(";;", ";");

                if (Code.EndsWith(";"))
                    Code = Code.Substring(0, Code.Length - 1);
                ScriptState = ScriptState.ContinueWithAsync<bool>(Code, ScriptOptions).Result;
                var v = (bool)ScriptState.ReturnValue;

                //var v = ScriptState.Variables.Select(va => va).Where(vn => vn.Name == "bresult").FirstOrDefault();
                if (v == true)
                    Next = OnTrue;
                else
                    Next = OnFalse;
            //}
            //catch (Exception ex)
            //{
                //Console.WriteLine(ex.ToString());
                //throw ex;
            //}
        }
    }
}
