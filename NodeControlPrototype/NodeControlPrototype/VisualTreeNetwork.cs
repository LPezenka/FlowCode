using FlowCodeInfrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NodeControlPrototype
{
    internal class VisualTreeNetwork : Network
    {
        public void Generate(List<NodeControlBase> nodes, List<EdgeControl> edges, NodeControlBase rootNode)
        {
            int nodeId = 0;
            int edgeId = 0;

            Dictionary<NodeControlBase, FlowCodeInfrastructure.Node> nodeMapper = new Dictionary<NodeControlBase, FlowCodeInfrastructure.Node>();

            try
            {
                foreach (var v in nodes)
                {
                    switch (v)
                    {
                        case RectangleNodeControl rc:
                            ActionNode an = new ActionNode
                            {
                                ID = $"Node{nodeId++}",
                                Code = rc.NodeData.Title
                            };
                            nodeMapper.Add(rc, an);
                            break;

                        case RhombusNodeControl rc:
                            DecisionNode dn = new DecisionNode
                            {
                                ID = $"Node{nodeId++}",
                                Code = rc.NodeData.Title
                            };
                            nodeMapper.Add(rc, dn);
                            break;
                        case ProcessNode pc:
                            var code = pc.NodeData.Title;
                            var variables = code.Split("(").Skip(1).FirstOrDefault();
                            variables = variables.Replace(")", "");
                            var returnSource = code.Split("=").Skip(1).FirstOrDefault().Trim();
                            returnSource = returnSource.Replace("()", "");
                            var returnTarget = code.Split("=").FirstOrDefault().Trim();



                            CallerNode cn = new CallerNode(null)
                            {
                                ID = $"Node{nodeId++}",
                                Code = pc.NodeData.Title,
                                Variables = variables,
                                ReturnSource = returnSource,
                                ReturnTarget = returnTarget,
                                TargetNode = null
                            };
                            nodeMapper.Add(pc, cn);
                            break;
                        case TerminatorNodeControl pc:
                            TerminatorNode tn = new TerminatorNode()
                            {
                                ID = $"Node{nodeId++}",
                                ResultVariable = pc.ReturnVariable,
                                Code = pc.FunctionName //pc.NodeData.Title
                            };
                            nodeMapper.Add(pc, tn);
                            break;

                        default:
                            Console.WriteLine("Error!");
                            break;
                    }
                }


                foreach (var edge in edges)
                {
                    NodeControlBase parentControl = nodeMapper.Keys.Where(n => n == edge.From).FirstOrDefault();
                    NodeControlBase childControl = nodeMapper.Keys.Where(n => n == edge.To).FirstOrDefault();

                    FlowCodeInfrastructure.Edge newEdge = new Edge();
                    newEdge.ID = $"Edge{edgeId++}";
                    newEdge.Text = edge.LabelBox.Text;


                    var parent = nodeMapper[parentControl];
                    var child = nodeMapper[childControl];

                    newEdge.Source = parent.ID;
                    newEdge.Target = child.ID;

                    Edges.Add(newEdge);


                    if (child != null && parent != null) child.Parent = parent.ID;
                    if (parent != null) parent.Next = child;

                    if (parentControl is not null)
                        if (parentControl is ProcessNode pc)
                        {



                            //if (edge.FromIndex == 2)
                            //{
                            //    (parent as CallerNode).TargetNode = child;
                            //    (parent as CallerNode).Variables = newEdge.Text;
                            //}
                            //else
                            //    (parent as CallerNode).Next = child;
                        }

                    if (childControl is not null)
                        if (childControl is TerminatorNodeControl tn)
                        {
                            var v = nodeMapper[tn] as TerminatorNode;
                            if (v is not null)
                            {
                                // Extract text from the textbox in the custom control and write its value to v.ResultVariable
                            }
                        }


                }

                foreach (var v in nodes)
                {
                    switch (v)
                    {
                        case RectangleNodeControl rc:
                            break;

                        case RhombusNodeControl rc:
                            try
                            {
                                var n = nodeMapper[rc] as DecisionNode;
                                var onTrue = Edges.Where(e => e.Source == n.ID && e.Text.ToLower() == Config.GetKeyword(Config.KeyWord.True).ToLower()).FirstOrDefault().Target;
                                var onFalse = Edges.Where(e => e.Source == n.ID && e.Text.ToLower() == Config.GetKeyword(Config.KeyWord.False).ToLower()).FirstOrDefault().Target;
                                n.OnTrue = nodeMapper.Values.Where(nm => nm.ID == onTrue).FirstOrDefault();
                                n.OnFalse = nodeMapper.Values.Where(nm => nm.ID == onFalse).FirstOrDefault();
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }
                            break;

                        case ProcessNode pn:
                            var c = nodeMapper[pn] as CallerNode;
                            string fname = pn.NodeData.Title.ToString();
                            if (fname.Contains("="))
                            {
                                fname = fname.Split("=")[1];
                                if (fname.Contains("("))
                                    fname = fname.Split("(")[0];
                                fname = fname.Trim();
                            }
                            //var targetNode = nodeMapper.Keys
                            //   .Where(x => x.GetType() == typeof(TerminatorNodeControl))
                            //   .Where(x => fname.Contains((x as TerminatorNodeControl).FunctionName))
                            //   .Select(x => x)
                            //   .FirstOrDefault();

                            var target = nodeMapper.Where(x => x.Key.GetType() == typeof(TerminatorNodeControl))
                                .Where(x => fname == (x.Key as TerminatorNodeControl).FunctionName)
                                .Select(x => x.Value)
                                .FirstOrDefault();
                            c.TargetNode = target;
                            break;
                    }
                }


                Nodes = nodeMapper.Values.ToList();
                RootNode = nodeMapper[rootNode];
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
