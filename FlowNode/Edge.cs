using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowNode
{
    internal class Edge
    {
        public string ID { get; set; }
        public string Source { get; set; }
        public string Target { get; set; }

        public string Text { get; set; }
    }
}
