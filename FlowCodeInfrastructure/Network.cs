using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowCodeInfrastructure
{
    public class Network
    {
        public List<Node> Nodes { get; set; }
        public List<Edge> Edges { get; set; }
        public Node RootNode { get; set; }
        public Network()
        {
            Nodes = new List<Node>();
            Edges = new List<Edge>();
            RootNode = null;
        }
        public void FromDrawIOFile(string fname)
        {
            var dion = new DrawIONetwork();
            dion.Load(fname);
        }

        public void Evaluate()
        {
            try
            {
                if (RootNode is not null)
                    RootNode.Evaluate();
            }
            catch (Exception ex)
            {
                XMLWriter.SaveXML(this, "dump/error.dump");
            }
        }
    }
}
