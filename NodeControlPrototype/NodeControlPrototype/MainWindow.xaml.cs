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
        private readonly Dictionary<EdgeControl, TextBox> _edgeLabels = new();

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

                //if (existing != null)
                //{
                //    DiagramCanvas.Children.Remove(existing);
                //    _edges.Remove(existing);
                //}
                if (existing != null)
                {
                    DiagramCanvas.Children.Remove(existing);

                    if (existing.LabelBox != null)
                    {
                        DiagramCanvas.Children.Remove(existing.LabelBox);
                        existing.LabelBox = null;
                    }

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

            _temporaryEdge.Label = string.Empty;
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
            var localTempEdge = _temporaryEdge; // Kopie

            if (localTempEdge == null || _edgeStartNode == null || !_edgeStartIndex.HasValue)
            {
                CleanupTemporaryEdge();
                return;
            }

            var pos = e.GetPosition(DiagramCanvas);
            var hit = VisualTreeHelper.HitTest(DiagramCanvas, pos)?.VisualHit;

            while (hit != null && hit is not NodeControlBase && hit is not Canvas)
                hit = VisualTreeHelper.GetParent(hit);

            if (hit is NodeControlBase targetNode)
            {
                localTempEdge.To = targetNode;
                localTempEdge.ToIndex = targetNode.GetNextFreeInputIndex();

                if (localTempEdge.To == localTempEdge.From)
                {
                    CleanupTemporaryEdge();
                    return;
                }

                _edgeStartNode.RegisterOutputEdge(_edgeStartIndex.Value, localTempEdge);

                _edges.Add(localTempEdge);
                localTempEdge.DeleteRequested += Edge_DeleteRequested;

                _edgeStartNode.NodeMoved += (s, ev) => UpdateEdges();
                targetNode.NodeMoved += (s, ev) => UpdateEdges();

                // Label erzeugen
                var labelBox = new TextBox
                {
                    Text = localTempEdge.Label,
                    Width = 80,
                    Background = Brushes.White,
                    BorderBrush = Brushes.Gray,
                    BorderThickness = new Thickness(1),
                    FontSize = 12,
                    Padding = new Thickness(2),
                    TextAlignment = TextAlignment.Center
                };

                // Position Label
                var start = localTempEdge.From.TranslatePoint(
                    localTempEdge.From.GetConnectionPoints()[localTempEdge.FromIndex ?? 0], DiagramCanvas);
                var end = localTempEdge.To.TranslatePoint(
                    localTempEdge.To.GetConnectionPoints()[localTempEdge.ToIndex ?? 0], DiagramCanvas);

                var labelPos = new Point((start.X + end.X) / 2 - 40, (start.Y + end.Y) / 2 - 10);

                Canvas.SetLeft(labelBox, labelPos.X);
                Canvas.SetTop(labelBox, labelPos.Y);

                labelBox.TextChanged += (s, ev) => localTempEdge.Label = labelBox.Text;

                DiagramCanvas.Children.Add(labelBox);

                localTempEdge.LabelBox = labelBox;

                //_edgeLabels.Add(localTempEdge, labelBox);
            }
            else
            {
                // Kein Node getroffen -> temporäre Edge entfernen
                DiagramCanvas.Children.Remove(localTempEdge);
            }

            CleanupTemporaryEdge();
        }

        private void CleanupTemporaryEdge()
        {
            _temporaryEdge = null;
            _edgeStartNode = null;
            _edgeStartIndex = null;

            MouseMove -= MainWindow_MouseMove;
            MouseLeftButtonUp -= MainWindow_MouseLeftButtonUp;
        }
        private void UpdateEdges()
        {
            foreach (var edge in _edges)
            {
                edge.InvalidateVisual();

                if (_edgeLabels.TryGetValue(edge, out var labelBox) && edge.From != null && edge.To != null)
                {
                    var start = edge.From.TranslatePoint(edge.From.GetConnectionPoints()[edge.FromIndex ?? 0], DiagramCanvas);
                    var end = edge.To.TranslatePoint(edge.To.GetConnectionPoints()[edge.ToIndex ?? 0], DiagramCanvas);
                    var labelPos = new Point((start.X + end.X) / 2 - 40, (start.Y + end.Y) / 2 - 10);

                    Canvas.SetLeft(labelBox, labelPos.X);
                    Canvas.SetTop(labelBox, labelPos.Y);
                }
            }
        }

        private void Edge_DeleteRequested(object sender, EventArgs e)
        {
            if (sender is EdgeControl edge)
            {
                edge.From?.UnregisterOutputEdge(edge.FromIndex ?? 0);
                //DiagramCanvas.Children.Remove(edge);

                if (edge.LabelBox != null)
                {
                    DiagramCanvas.Children.Remove(edge.LabelBox);
                    edge.LabelBox = null;
                }
                DiagramCanvas.Children.Remove(edge);
                
                _edges.Remove(edge);
                
                //if (_edgeLabels.TryGetValue(edge, out var label))
                //{
                //    DiagramCanvas.Children.Remove(label);
                //    _edgeLabels.Remove(edge);
                //}
            }
        }
    }
}
