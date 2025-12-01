using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowEditor.Controls
{
    public class Node
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; }
        public Point Position { get; set; }
    }
}
