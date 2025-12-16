using Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowCodeInfrastructure
{
    public class Network
    {
        public IErrorLogger ErrorLogger { get; set; }
        public List<Node> Nodes { get; set; }
        public List<Edge> Edges { get; set; }
        public Node RootNode { get; set; }
        public Network()
        {
            Nodes = new List<Node>();
            Edges = new List<Edge>();
            RootNode = null;
        }

        public static void ResetCargoTrucker()
        {
            CargoTrucker.Client.GameApi.ReloadField();
            //CargoTrucker.Client.GameApi.
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
                    Node.Run(RootNode);
            }
            catch (Exception ex)
            {
                //Console.Error.WriteLine(ex.ToString());
                XMLWriter.SaveXML(this, "dump/error.dump");
                ErrorLogger.LogError(ex.Message);
            }
        }
    }
}
