using Interfaces;

namespace FlowCodeInfrastructure
{
    public abstract class Node
    {
        internal static bool InterruptProcess { get; set; } = false;
        public static IVariableLogger VariableLogger { get; set; } = null;
        public static int Delay { get; set; } = 500;
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
                current.GraphicalNode?.SetActive(true);

                try
                {
                    current.Evaluate();
                }
                catch (Exception ex)
                {
                    throw ex;
                }

                // Add a delay.Should be specified as a class Level Config
                Thread.Sleep(Delay); 
                current.GraphicalNode?.SetActive(false);

                if (!InterruptProcess) 
                    current = current.Next;
                else
                {
                    InterruptProcess = false;
                    return;
                }
            }
        }
    }
}
