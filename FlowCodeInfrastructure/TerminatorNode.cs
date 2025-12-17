using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowCodeInfrastructure
{
    public class TerminatorNode: ActionNode
    {
        public static object ReturnValue { get; set; }
        public string ResultVariable { get; set; }
        
        public List<string> InputVariables { get; set; }

        public override void Evaluate()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(ResultVariable))
                    ReturnValue = ScriptState.Variables.Where(x => x.Name == ResultVariable).FirstOrDefault()?.Value;
                else
                    ReturnValue = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
