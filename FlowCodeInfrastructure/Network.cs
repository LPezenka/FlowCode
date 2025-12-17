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

        public static async void ResetCargoTrucker()
        {

            int timeout = 1000;
            Task task = Task.Run(() =>
            {
                CargoTrucker.Client.GameApi.ReloadField();
            }
            );

            if (await Task.WhenAny(task, Task.Delay(timeout)) == task)
            {
                Console.WriteLine("Successfully connected to CargoTrucker");
            }
            else
            {
                Console.WriteLine("Connection to CargoTrucker timed out");
            }

            // This is a problem, since it blocks the thread if there is no CargoTrucker running
            //Task.Run(() =>
            //{
            //    CargoTrucker.Client.GameApi.ReloadField();
            //}
            //);
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
                XMLWriter.SaveXML(this, "dump/error.dump");
                ErrorLogger.LogError(ex.Message);
            }
        }

        public static void InterruptProcess()
        {
            Node.InterruptProcess = true;
        }

    }
}
