// MainWindow.xaml.cs
using CargoTrucker;
using CargoTrucker.Client;
using FlowCodeInfrastructure;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Win32;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

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
        private NodeControlBase rootNode = null;


        public MainWindow()
        {
            InitializeComponent();
        }

        private void AddNode(NodeControlBase node, Point position)
        {
            node.Width = 240;
            node.Height = 60;
            Canvas.SetLeft(node, position.X);
            Canvas.SetTop(node, position.Y);

            node.ConnectionPointClicked += Node_ConnectionPointClicked;
            node.NodeMoved += (s, e) => UpdateEdges();
            node.RootRequested += Node_RootRequested;
            //node.MouseDoubleClick += node.
            DiagramCanvas.Children.Add(node);
        }

        private NodeControlBase _currentRoot;

        private void Node_RootRequested(object sender, EventArgs e)
        {
            if (_currentRoot != null)
            {
                _currentRoot.Background = Brushes.White;
                _currentRoot.IsRoot = false;
            }

            _currentRoot = sender as NodeControlBase;

            if (_currentRoot != null)
            {
                _currentRoot.Background = Brushes.LightGray;
                _currentRoot.IsRoot = true;
            }
        }


        private void AddDecisionNode_Click(object sender, RoutedEventArgs e)
        {
            var decisionNode = new RhombusNodeControl
            {
                Width = 60,
                Height = 60,
                NodeData = new Node
                {
                    Title = $"Decision Node({_nodeCounter++})",
                    Position = new Point(50 + _nodeCounter * 20, 50 + _nodeCounter * 20)
                }
            };

            AddNode(decisionNode, decisionNode.NodeData.Position);
        }

        private void AddNode_Click(object sender, RoutedEventArgs e)
        {
            var node = new RectangleNodeControl
            {
                Width = 60,
                Height = 40,
                NodeData = new Node
                {
                    Title = $"Node({_nodeCounter++})",
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

        private void AddFunctionNode_Click(object sender, RoutedEventArgs e)
        {
            var processNode = new ProcessNode()
            {
                Width = 90,
                Height = 50,
                NodeData = new Node
                {
                    Title = $"ProcessCall({_nodeCounter++})",
                    Position = new Point(50 + _nodeCounter * 20, 50 + _nodeCounter * 20)
                }
            };
            AddNode(processNode, processNode.NodeData.Position);
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

        private VisualTreeNetwork vtn = null;
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            GenerateNetwork();
        }

        private void GenerateNetwork()
        {
            Config.SetKeyWord(Config.KeyWord.True, "Ja");
            Config.SetKeyWord(Config.KeyWord.False, "Nein");
            Config.SetKeyWord(Config.KeyWord.Function, "Function");
            if (_currentRoot == null)
            {
                MessageBox.Show("No root node selected!");
                return;
            }

            vtn = new VisualTreeNetwork();
            List<NodeControlBase> nodes = new();

            foreach (var c in DiagramCanvas.Children)
            {
                switch (c)
                {
                    case RectangleNodeControl rc:
                        nodes.Add(rc);
                        break;
                    case RhombusNodeControl rc:
                        nodes.Add(rc);
                        break;
                    case ProcessNode pc:
                        nodes.Add(pc);
                        break;
                }
            }

            vtn.Generate(nodes, _edges, _currentRoot);
        }


        private void SaveBitmap(string path)
        {
            AddNodeButton.Visibility = Visibility.Collapsed;
            AddDecisionButton.Visibility = Visibility.Collapsed;
            AddProcessCallButton.Visibility = Visibility.Collapsed;
            RunButton.Visibility = Visibility.Collapsed;
            try
            {
                var target = DiagramCanvas;
                if (target == null)
                    return;

                Rect bounds = VisualTreeHelper.GetDescendantBounds(target);

                RenderTargetBitmap rtb = new RenderTargetBitmap((Int32)bounds.Width, (Int32)bounds.Height, 96, 96, PixelFormats.Pbgra32);

                DrawingVisual dv = new DrawingVisual();

                using (DrawingContext dc = dv.RenderOpen())
                {
                    VisualBrush vb = new VisualBrush(target);
                    dc.DrawRectangle(vb, null, new Rect(new Point(), bounds.Size));
                }

                rtb.Render(dv);

                PngBitmapEncoder png = new PngBitmapEncoder();

                png.Frames.Add(BitmapFrame.Create(rtb));

                using (Stream stm = File.Create(path))
                {
                    png.Save(stm);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                AddNodeButton.Visibility = Visibility.Visible;
                AddDecisionButton.Visibility = Visibility.Visible;
                AddProcessCallButton.Visibility = Visibility.Visible;
                RunButton.Visibility = Visibility.Visible;
            }
        }

        private void Run_Click(object sender, RoutedEventArgs e)
        {


            //if (vtn == null)
            //{
                GenerateNetwork();
            //}

            if (_currentRoot == null)
            {
                MessageBox.Show("No root node selected!");
                return;
            }
            //CargoTrucker.Client.GameApi.Forward();
            //Forward();



            ScriptOptions scriptOptions = ScriptOptions.Default;

            //Add reference to mscorlib
            var mscorlib = typeof(System.Object).Assembly;
            var systemCore = typeof(System.Linq.Enumerable).Assembly;
            var cargoTrucker = typeof(CargoTrucker.Client.GameApi).Assembly;

            scriptOptions = scriptOptions.AddReferences(mscorlib, systemCore, cargoTrucker);
            //Add namespaces
            scriptOptions = scriptOptions.AddImports("System");
            scriptOptions = scriptOptions.AddImports("System.Linq");
            scriptOptions = scriptOptions.AddImports("System.Collections.Generic");
            //scriptOptions = scriptOptions.AddImports("System.Diagnostics");
            scriptOptions = scriptOptions.AddImports("CargoTrucker.Client.GameApi");
            
            var result = CSharpScript.RunAsync("Console.WriteLine(\"Starting Script\")", scriptOptions).Result;
            result = result.ContinueWithAsync("int i = 0, j = 1; char a = 'a'; bool b = true, c = false;", scriptOptions).Result;
            ActionNode.ScriptState = result;
            ActionNode.ScriptOptions = scriptOptions;

            FlowCodeInfrastructure.Node.Run(vtn.RootNode);
        }

        private void MenuItemExit_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void MenuItemExport_Click(object sender, RoutedEventArgs e)
        {
            var sfd = new SaveFileDialog();
            sfd.DefaultExt = "png";
            sfd.Filter = "PNG Files | *.png | All files (*.*)|*.*";
            if (sfd.ShowDialog() == true)
            {
                SaveBitmap(sfd.FileName);
            }
        }
    }
}
