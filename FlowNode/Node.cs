﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowNode
{
    internal abstract class Node
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
                current.Evaluate();
                current = current.Next;
            }
        }

    }
}
