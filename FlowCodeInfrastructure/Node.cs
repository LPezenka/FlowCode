using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowCodeInfrastructure
{
    public abstract class Node
    {
        public Node Next { get; set; }
        public string ID { get; set; }
        public string Parent { get; set; }

        public string Code { get; set; }

        public abstract void Evaluate();
        public static void Run(Node n)
        {
            Node current = n;
            while (current != null)
            {
                //if (current.GetType() == typeof(CallerNode) || current.Next.GetType() == typeof(CallerNode))
                //    Console.WriteLine("Calling...");
                current.Evaluate();
                current = current.Next;
            }
        }

    }
}
