using FlowCodeInfrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeControlPrototype
{
    internal class VisualTreeNetwork:Network
    {
        public void Generate(List<NodeControlBase> nodes, List<EdgeControl> edges, NodeControlBase rootNode)
        {
            int nodeId = 0;
            int edgeId = 0;

            Dictionary<NodeControlBase, FlowCodeInfrastructure.Node> nodeMapper = new Dictionary<NodeControlBase, FlowCodeInfrastructure.Node>();

            foreach (var v in nodes)
            {
                switch(v)
                {
                    case RectangleNodeControl rc:
                        ActionNode an = new ActionNode();
                        an.ID = $"Node{nodeId++}";
                        an.Code = rc.NodeData.Title;
                        nodeMapper.Add(rc, an);
                        break;

                    case RhombusNodeControl rc:
                        DecisionNode dn = new DecisionNode();
                        dn.ID = $"Node{nodeId++}";
                        dn.Code = rc.NodeData.Title;
                        nodeMapper.Add(rc, dn);
                        break;

                    default:
                        Console.WriteLine("Error!");
                        break;
                }
            }


            foreach (var edge in edges)
            {
                NodeControlBase parentControl   = nodeMapper.Keys.Where(n => n == edge.From).FirstOrDefault();
                NodeControlBase childControl    = nodeMapper.Keys.Where(n => n == edge.To).FirstOrDefault();

                FlowCodeInfrastructure.Edge newEdge = new Edge();
                newEdge.ID = $"Edge{edgeId++}";
                newEdge.Text = edge.LabelBox.Text;
 

                var parent  = nodeMapper[parentControl];
                var child   = nodeMapper[childControl];

                newEdge.Source = parent.ID;
                newEdge.Target = child.ID;

                Edges.Add(newEdge);


                if (child != null && parent != null) child.Parent = parent.ID;
                if (parent != null) parent.Next = child;
            }
            Nodes = nodeMapper.Values.ToList();
            RootNode = nodeMapper[rootNode];
        }
    }
}
