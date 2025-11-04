using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FlowCodeInfrastructure
{
    public abstract class Node
    {
        public IHighlightable GraphicalNode;
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
                //long ticks = DateTime.Now.Ticks;

                if (current.GraphicalNode != null)
                {
                    current.GraphicalNode.SetActive(true);
                }
                //if (current.GetType() == typeof(CallerNode) || current.Next.GetType() == typeof(CallerNode))
                //    Console.WriteLine("Calling...");
                current.Evaluate();

                // Add a delay.Should be specified as a class Level Config
                //DateTime dt = new DateTime(DateTime.Now.Ticks - ticks);
                //while (dt.Second < 4)
                //    dt = new DateTime(DateTime.Now.Ticks - ticks);
                //Thread.Sleep(2000); 
                if (current.GraphicalNode != null)
                {
                    current.GraphicalNode.SetActive(false);
                }
                current = current.Next;
            }
        }
    }
}
