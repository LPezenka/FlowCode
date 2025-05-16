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

        public override void Evaluate()
        {
            if (Code == null)
            {
                return;
            }
            try
            {
                if (Code.Contains("Ausgabe:"))
                {
                    Code = Code.Replace(";", "");
                    Code = Code.Replace("Ausgabe:", "Console.WriteLine(");
                    Code += ");";
                }
                else if (Code.Contains("Eingabe"))
                {
                    string[] parts = Code.Split(new[] { '=' });
                    string varName = parts[0].Trim();
                    Code = "string lineInput = Console.ReadLine(); ";
                    //Code = Code.Replace(";", "");
                    Code += varName + " = int.Parse(lineInput);";
                    // TODO für verschiedene Datentypen implementieren oder Typ der Eingabe erkennen und Parsen
                    //Code += ");";
                     
                }

                if (Code.Contains("Function"))
                {
                    Console.WriteLine($"Entering {Code}");
                }
                else
                {
                    if (Code.EndsWith(';') == false) Code = $"{Code};";
                    ScriptState = ScriptState.ContinueWithAsync(Code, ScriptOptions).Result;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
