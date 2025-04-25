//using System.Text;
//using System.Windows;
//using System.Windows.Controls;
//using System.Windows.Data;
//using System.Windows.Documents;
//using System.Windows.Input;
//using System.Windows.Media;
//using System.Windows.Media.Imaging;
//using System.Windows.Navigation;
//using System.Windows.Shapes;
//using System.Xml.Linq;

//namespace NodeControlPrototype
//{
//    /// <summary>
//    /// Interaction logic for MainWindow.xaml
//    /// </summary>
//    public partial class MainWindow : Window
//    {
//        int _nodeCounter = 0;
//        private NodeControlBase selectedNode = null;
//        private readonly List<EdgeControl> edges = new();


//        private NodeControlBase _edgeStartNode = null;
//        private int? _edgeStartIndex = null;
//        private EdgeControl _temporaryEdge = null;
//        private readonly List<EdgeControl> _edges = new();

//        public MainWindow()
//        {
//            InitializeComponent();
//        }

//        private void Edge_DeleteRequested(object sender, EventArgs e)
//        {
//            if (sender is EdgeControl edge)
//            {
//                DiagramCanvas.Children.Remove(edge);
//                edges.Remove(edge);
//            }
//        }

//        private void AddDecisionNode_Click(object sender, RoutedEventArgs e)
//        {
//            var decisionNode = new RhombusNodeControl
//            {
//                Width = 180,
//                Height = 80,
//                NodeData = new Node
//                {
//                    Title = $"Decision Node {DiagramCanvas.Children.Count}",
//                    Position = new Point(50 + _nodeCounter * 20, 50 + _nodeCounter * 20)
//                }
//            };

//            /*decisionNode.MouseLeftButtonDown += Node_MouseLeftButtonDown;

//            Canvas.SetLeft(decisionNode, decisionNode.NodeData.Position.X);
//            Canvas.SetTop(decisionNode, decisionNode.NodeData.Position.Y);

//            DiagramCanvas.Children.Add(decisionNode);*/
//            AddNode(decisionNode, new Point(100 + DiagramCanvas.Children.Count * 30, 100 + DiagramCanvas.Children.Count * 20));
//        }

//        private void AddNode(NodeControlBase node, Point position)
//        {
//            node.Width = 120;
//            node.Height = 60;
//            Canvas.SetLeft(node, position.X);
//            Canvas.SetTop(node, position.Y);

//            node.ConnectionPointClicked += Node_ConnectionPointClicked;
//            node.NodeMoved += (s, e) => UpdateEdges();

//            DiagramCanvas.Children.Add(node);
//        }

//        private void Node_ConnectionPointClicked(object sender, ConnectionPointClickedEventArgs e)
//        {
//            if (_temporaryEdge != null)
//            {
//                DiagramCanvas.Children.Remove(_temporaryEdge);
//                _temporaryEdge = null;
//            }

//            _edgeStartNode = e.Node;

//            _edgeStartIndex = e.ConnectionPointIndex;
//            var fromNode = e.Node;

//            if (fromNode != null && _edgeStartIndex.HasValue)
//            {
//                // prüfen, ob bereits eine Edge von diesem Punkt ausgeht
//                fromNode.UnregisterOutputEdge(_edgeStartIndex.Value);

//                // ggf. alte Edge entfernen, wenn du sie getrackt hast
//                var existing = DiagramCanvas.Children
//                    .OfType<EdgeControl>()
//                    .FirstOrDefault(edge => edge.From == fromNode && edge.FromIndex == _edgeStartIndex);

//                if (existing != null)
//                {
//                    DiagramCanvas.Children.Remove(existing);
//                }
//            }

//            _temporaryEdge = new EdgeControl
//            {
//                From = _edgeStartNode,
//                FromIndex = _edgeStartIndex,
//                To = null // Ziel ist Mauszeiger
//            };
//            Panel.SetZIndex(_temporaryEdge, -1);
//            DiagramCanvas.Children.Add(_temporaryEdge);

//            MouseMove += MainWindow_MouseMove;
//            MouseLeftButtonUp += MainWindow_MouseLeftButtonUp;
//        }

//        private void MainWindow_MouseMove(object sender, MouseEventArgs e)
//        {
//            if (_temporaryEdge != null)
//            {
//                Point pos = e.GetPosition(DiagramCanvas);
//                _temporaryEdge.CurrentMousePosition = pos;
//                _temporaryEdge.InvalidateVisual();
//            }
//        }

//        private void MainWindow_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
//        {
//            if (_temporaryEdge != null)
//            {
//                var pos = e.GetPosition(DiagramCanvas);
//                var hit = VisualTreeHelper.HitTest(DiagramCanvas, pos)?.VisualHit;

//                while (hit != null && hit is not NodeControlBase && hit is not Canvas)
//                    hit = VisualTreeHelper.GetParent(hit);

//                if (hit is NodeControlBase targetNode)
//                {
//                    _temporaryEdge.To = targetNode;
//                    _temporaryEdge.ToIndex = 0;
//                    _edges.Add(_temporaryEdge);

//                    _edgeStartNode.RegisterOutputEdge(_edgeStartIndex.Value, _temporaryEdge);
//                    targetNode.NodeMoved += (s, ev) => UpdateEdges();
//                }
//                else
//                {
//                    DiagramCanvas.Children.Remove(_temporaryEdge);
//                }

//                _temporaryEdge.CurrentMousePosition = null;
//                _temporaryEdge = null;
//                _edgeStartNode = null;
//                _edgeStartIndex = null;

//                MouseMove -= MainWindow_MouseMove;
//                MouseLeftButtonUp -= MainWindow_MouseLeftButtonUp;
//            }
//        }

//        private void UpdateEdges()
//        {
//            foreach (var edge in _edges)
//            {
//                edge.InvalidateVisual();
//            }
//        }

//        /*private void AddButton_Click(object sender, RoutedEventArgs e)
//        {
//            var node = new RectangleNodeControl
//            {
//                NodeData = new Node { Title = $"Node {DiagramCanvas.Children.Count}" }
//            };
//            AddNode(node, new Point(100 + DiagramCanvas.Children.Count * 30, 100 + DiagramCanvas.Children.Count * 20));
//        }*/

//        private void AddNode_Click(object sender, RoutedEventArgs e)
//        {
//            var node = new RectangleNodeControl
//            {
//                Width = 180,
//                Height = 80,
//                NodeData = new Node
//                {
//                    Title = $"Node {DiagramCanvas.Children.Count}",
//                    Position = new Point(50 + _nodeCounter * 20, 50 + _nodeCounter * 20)
//                }
//            };
//            //node.MouseLeftButtonDown += Node_MouseLeftButtonDown;

//            //Canvas.SetLeft(node, node.NodeData.Position.X);
//            //Canvas.SetTop(node, node.NodeData.Position.Y);
//            AddNode(node, new Point(100 + DiagramCanvas.Children.Count * 30, 100 + DiagramCanvas.Children.Count * 20));
//            //DiagramCanvas.Children.Add(node);
//        }

//        private void Node_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
//        {
//            var clickedNode = sender as NodeControlBase;

//            if (selectedNode == null)
//            {
//                selectedNode = clickedNode;
//            }
//            else if (clickedNode != selectedNode)
//            {

//                var fromIndex = selectedNode.GetNextFreeOutputIndex();
//                var toIndex = clickedNode.GetNextFreeInputIndex();

//                var edge = new EdgeControl
//                {
//                    From = selectedNode,
//                    To = clickedNode,
//                    FromIndex = fromIndex,
//                    ToIndex = toIndex,
//                    //Width = DiagramCanvas.ActualWidth,
//                    //Height = DiagramCanvas.ActualHeight,
//                    IsHitTestVisible = true,
//                    Label = "Test"
//                };



//                edge.DeleteRequested += Edge_DeleteRequested;
//                selectedNode.NodeMoved += (s, ev) => edge.InvalidateVisual();
//                clickedNode.NodeMoved += (s, ev) => edge.InvalidateVisual();

//                DiagramCanvas.Children.Insert(0, edge); // damit der Pfeil unter den Nodes liegt

//                selectedNode = null; // Reset Auswahl
//            }

//            e.Handled = true;
//        }

//    }
//}


// MainWindow.xaml.cs
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace NodeControlPrototype
{
    public partial class MainWindow : Window
    {
        private int _nodeCounter = 0;
        private NodeControlBase _edgeStartNode = null;
        private int? _edgeStartIndex = null;
        private EdgeControl _temporaryEdge = null;
        private readonly List<EdgeControl> _edges = new();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void AddNode(NodeControlBase node, Point position)
        {
            node.Width = 120;
            node.Height = 60;
            Canvas.SetLeft(node, position.X);
            Canvas.SetTop(node, position.Y);

            node.ConnectionPointClicked += Node_ConnectionPointClicked;
            node.NodeMoved += (s, e) => UpdateEdges();

            DiagramCanvas.Children.Add(node);
        }

        private void AddDecisionNode_Click(object sender, RoutedEventArgs e)
        {
            var decisionNode = new RhombusNodeControl
            {
                Width = 180,
                Height = 80,
                NodeData = new Node
                {
                    Title = $"Decision Node {_nodeCounter++}",
                    Position = new Point(50 + _nodeCounter * 20, 50 + _nodeCounter * 20)
                }
            };

            AddNode(decisionNode, decisionNode.NodeData.Position);
        }

        private void AddNode_Click(object sender, RoutedEventArgs e)
        {
            var node = new RectangleNodeControl
            {
                Width = 180,
                Height = 80,
                NodeData = new Node
                {
                    Title = $"Node {_nodeCounter++}",
                    Position = new Point(50 + _nodeCounter * 20, 50 + _nodeCounter * 20)
                }
            };
            AddNode(node, node.NodeData.Position);
        }

        private void Node_ConnectionPointClicked(object sender, ConnectionPointClickedEventArgs e)
        {
            if (_temporaryEdge != null)
            {
                DiagramCanvas.Children.Remove(_temporaryEdge);
                _temporaryEdge = null;
            }

            _edgeStartNode = e.Node;
            _edgeStartIndex = e.ConnectionPointIndex;

            if (_edgeStartNode != null && _edgeStartIndex.HasValue)
            {
                _edgeStartNode.UnregisterOutputEdge(_edgeStartIndex.Value);

                var existing = DiagramCanvas.Children
                    .OfType<EdgeControl>()
                    .FirstOrDefault(edge => edge.From == _edgeStartNode && edge.FromIndex == _edgeStartIndex);

                if (existing != null)
                {
                    DiagramCanvas.Children.Remove(existing);
                    _edges.Remove(existing);
                }
            }

            _temporaryEdge = new EdgeControl
            {
                From = _edgeStartNode,
                FromIndex = _edgeStartIndex,
                To = null
            };
            Panel.SetZIndex(_temporaryEdge, -1);
            DiagramCanvas.Children.Add(_temporaryEdge);

            MouseMove += MainWindow_MouseMove;
            MouseLeftButtonUp += MainWindow_MouseLeftButtonUp;
        }

        private void MainWindow_MouseMove(object sender, MouseEventArgs e)
        {
            if (_temporaryEdge != null)
            {
                Point pos = e.GetPosition(DiagramCanvas);
                _temporaryEdge.CurrentMousePosition = pos;
                _temporaryEdge.InvalidateVisual();
            }
        }

        private void MainWindow_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_temporaryEdge != null)
            {
                var pos = e.GetPosition(DiagramCanvas);
                var hit = VisualTreeHelper.HitTest(DiagramCanvas, pos)?.VisualHit;

                while (hit != null && hit is not NodeControlBase && hit is not Canvas)
                    hit = VisualTreeHelper.GetParent(hit);

                if (hit is NodeControlBase targetNode)
                {
                    if (_temporaryEdge == null)
                    {
                        Console.WriteLine("Temporary edge is null");
                        return;
                    }
                    _temporaryEdge.To = targetNode;
                    _temporaryEdge.ToIndex = targetNode.GetNextFreeInputIndex();

                    _edgeStartNode.RegisterOutputEdge(_edgeStartIndex.Value, _temporaryEdge);
                    _temporaryEdge.Label = "";

                    _edges.Add(_temporaryEdge);

                    _edgeStartNode.NodeMoved += (s, ev) => _temporaryEdge.InvalidateVisual();
                    targetNode.NodeMoved += (s, ev) => _temporaryEdge.InvalidateVisual();
                }
                else
                {
                    DiagramCanvas.Children.Remove(_temporaryEdge);
                }

                _temporaryEdge.CurrentMousePosition = null;
                _temporaryEdge = null;
                _edgeStartNode = null;
                _edgeStartIndex = null;

                MouseMove -= MainWindow_MouseMove;
                MouseLeftButtonUp -= MainWindow_MouseLeftButtonUp;
            }
        }

        private void UpdateEdges()
        {
            foreach (var edge in _edges)
            {
                edge.InvalidateVisual();
            }
        }

        private void Edge_DeleteRequested(object sender, EventArgs e)
        {
            if (sender is EdgeControl edge)
            {
                edge.From?.UnregisterOutputEdge(edge.FromIndex ?? 0);
                DiagramCanvas.Children.Remove(edge);
                _edges.Remove(edge);
            }
        }
    }
}
