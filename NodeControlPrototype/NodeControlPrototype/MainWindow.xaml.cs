// MainWindow.xaml.cs
using CargoTrucker;
using CargoTrucker.Client;
using FlowCodeInfrastructure;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Win32;
using NodeControlPrototype.Controls;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;
using System.Xml.Linq;
using Interfaces;

namespace NodeControlPrototype
{
    public partial class MainWindow : Window, IErrorLogger
    {
        /// <summary>
        /// Vertical spacing (in pixels) used when auto-placing newly added nodes below the last one.
        /// </summary>
        private int _offset = 25;

        /// <summary>
        /// Incremental counter used to generate unique node titles and default positions.
        /// </summary>
        private int _nodeCount = 0;

        /// <summary>
        /// Node where an interactive edge creation (drag) was started.
        /// </summary>
        private NodeControlBase _edgeStartNode = null;

        /// <summary>
        /// Connection point index on the start node for the interactive edge creation.
        /// </summary>
        private int? _edgeStartIndex = null;

        /// <summary>
        /// Temporary edge shown while the user is dragging to connect two nodes.
        /// </summary>
        private EdgeControl _temporaryEdge = null;

        /// <summary>
        /// Collection of all committed edges currently present on the canvas.
        /// </summary>
        private readonly List<EdgeControl> edges = new();

        /// <summary>
        /// Optional mapping of edges to their label TextBoxes on the canvas (legacy/aux; labels are also stored on EdgeControl.LabelBox).
        /// </summary>
        private readonly Dictionary<EdgeControl, TextBox> edgeLabels = new();

        /// <summary>
        /// All node controls present on the canvas, used for lookups (e.g., when loading edges) and updates.
        /// </summary>
        private List<NodeControlBase> canvasNodes = new();

        /// <summary>
        /// Remembered position used as the baseline for auto-placing the next node.
        /// </summary>
        private Point _lastNodePosition;

        /// <summary>
        /// Current zoom scale applied to the diagram canvas via LayoutTransform.
        /// </summary>
        private double scaleValue = 1.0f;


        private DeleteZone _dz;

        public MainWindow()
        {
            this.WindowState = WindowState.Maximized;
            InitializeComponent();
            _lastNodePosition = new Point(Application.Current.MainWindow.Width / 2, 15);
            //DeleteZone 
            _dz = new DeleteZone();
            //dz.DragEnter += dz.OnDragEnter;
            _dz.MouseEnter += Dz_MouseEnter;
            _dz.MouseLeave += Dz_MouseLeave;
            //_dz.BackgroundColor = Brushes.Red;
            //_dz.AllowDrop = true;

            Canvas.SetLeft(_dz, 400);
            Canvas.SetTop(_dz, 400);
            DiagramCanvas.Children.Add(_dz);


            Config.SetKeyWord(Config.KeyWord.True, "Ja");
            Config.SetKeyWord(Config.KeyWord.False, "Nein");
            Config.SetKeyWord(Config.KeyWord.Function, "Function");
            Config.SetKeyWord(Config.KeyWord.Input, "Eingabe");
            Config.SetKeyWord(Config.KeyWord.Output, "Ausgabe");
        }

        private void Dz_MouseEnter(object sender, MouseEventArgs e)
        {

            //throw new NotImplementedException();
            _dz.BackgroundColor = Brushes.Red;
        }

        private void Dz_MouseLeave(object sender, MouseEventArgs e)
        {
            _dz.BackgroundColor = Brushes.Transparent;
        }

        private void AddNode(NodeControlBase node, Point? nodePosition)
        {
            node.Width = 240;
            node.Height = 90;
            //node.Width = double.NaN;
            //node.Height = double.NaN;
            Point position;
            if (nodePosition != null)
            {
                position = (Point)nodePosition;
            }
            else
            {
                position = new Point(
                _lastNodePosition.X,
                _lastNodePosition.Y + node.Height + _offset);

                node.NodeData.Position = position;
            }

            _lastNodePosition = position;

            Canvas.SetLeft(node, position.X);
            Canvas.SetTop(node, position.Y);

            node.ConnectionPointClicked += Node_ConnectionPointClicked;
            node.NodeMoved += (s, e) => UpdateEdges();
            node.RootRequested += Node_RootRequested;
            //node.MouseDoubleClick += node.
            DiagramCanvas.Children.Add(node);
            canvasNodes.Add(node);
        }

        private NodeControlBase currentRoot;

        private void Node_RootRequested(object sender, EventArgs e)
        {
            if (currentRoot != null)
            {
                currentRoot.Background = currentRoot.OriginalBackground;
                currentRoot.IsRoot = false;
            }

            currentRoot = sender as NodeControlBase;

            if (currentRoot != null)
            {
                currentRoot.Background = Brushes.Gold;
                currentRoot.IsRoot = true;
            }
        }


        private void AddDecisionNode_Click(object sender, RoutedEventArgs e)
        {
            var decisionNode = new DecisionNodeControl
            {
                Width = 240,
                Height = 160,
                OriginalBackground = Brushes.Red,
                NodeData = new Controls.Node
                {
                    Title = $"Decision Node({_nodeCount++})",
                    //Position = null //new Point(50 + _nodeCounter * 20, 50 + _nodeCounter * 20)
                }
            };

            AddNode(decisionNode, null); // decisionNode.NodeData.Position);
        }

        private void AddNode_Click(object sender, RoutedEventArgs e)
        {
            var node = new SequenceNodeControl
            {
                Width = 120,
                Height = 40,
                OriginalBackground = Brushes.Gray,
                NodeData = new Controls.Node
                {
                    Title = $"Node({_nodeCount++})",
                    //Position = new Point(50 + _nodeCounter * 20, 50 + _nodeCounter * 20)
                }
            };
            AddNode(node, null);// node.NodeData.Position);
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

                    edges.Remove(existing);
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


        private void AddTerminatorNode_Click(object sender, RoutedEventArgs e)
        {
            var terminator = new TerminalNodeControl
            {
                Width = 160,
                Height = 120,
                OriginalBackground = Brushes.LightGreen,
                NodeData = new Controls.Node
                {
                    Title = "Start/End",
                    Position = new Point(50 + _nodeCount * 20, 50 + _nodeCount * 20)
                },
                TerminalType = "Start"
            };

            AddNode(terminator, terminator.NodeData.Position);
        }
        private void AddFunctionNode_Click(object sender, RoutedEventArgs e)
        {
            var processNode = new ProcessNodeControl()
            {
                Width = 120,
                Height = 60,
                OriginalBackground = Brushes.Orange,
                NodeData = new Controls.Node
                {
                    Title = $"ProcessCall({_nodeCount++})",
                    Position = new Point(50 + _nodeCount * 20, 50 + _nodeCount * 20)
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

                edges.Add(localTempEdge);
                localTempEdge.DeleteRequested += Edge_DeleteRequested;

                _edgeStartNode.NodeMoved += (s, ev) => UpdateEdges();
                targetNode.NodeMoved += (s, ev) => UpdateEdges();

                // Label erzeugen
                GenerateEdgeLabel(localTempEdge);
                // InvalidateVisual();

                //_edgeLabels.Add(localTempEdge, labelBox);
            }
            else
            {
                // Kein Node getroffen -> temporäre Edge entfernen
                DiagramCanvas.Children.Remove(localTempEdge);
            }

            CleanupTemporaryEdge();
        }

        private void GenerateEdgeLabel(EdgeControl localTempEdge)
        {
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
            foreach (var edge in edges)
            {
                edge.InvalidateVisual();

                if (edgeLabels.TryGetValue(edge, out var labelBox) && edge.From != null && edge.To != null)
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

                edges.Remove(edge);

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

            if (currentRoot == null)
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
                    case SequenceNodeControl rc:
                        nodes.Add(rc);
                        break;
                    case DecisionNodeControl rc:
                        nodes.Add(rc);
                        break;
                    case ProcessNodeControl pc:
                        nodes.Add(pc);
                        break;
                    case TerminalNodeControl tc:
                        nodes.Add(tc);
                        break;
                }
            }

            vtn.Generate(nodes, edges, currentRoot);
        }


        private void SaveBitmap(string path)
        {
            AddNodeButton.Visibility = Visibility.Collapsed;
            AddDecisionButton.Visibility = Visibility.Collapsed;
            AddProcessCallButton.Visibility = Visibility.Collapsed;
            AddTerminalButton.Visibility = Visibility.Collapsed;
            RunButton.Visibility = Visibility.Collapsed;
            try
            {
                var target = DiagramCanvas;
                if (target == null)
                    return;
                BitmapExporter.ExportBitmap(path, target);
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
                AddTerminalButton.Visibility = Visibility.Visible;
                RunButton.Visibility = Visibility.Visible;
            }
        }

        //private void ExportBitmap(string path, Canvas target)
        //{
        //    Rect bounds = VisualTreeHelper.GetDescendantBounds(target);

        //    RenderTargetBitmap rtb = new RenderTargetBitmap((Int32)bounds.Width, (Int32)bounds.Height, 96, 96, PixelFormats.Pbgra32);

        //    DrawingVisual dv = new DrawingVisual();

        //    using (DrawingContext dc = dv.RenderOpen())
        //    {
        //        VisualBrush vb = new VisualBrush(target);
        //        dc.DrawRectangle(vb, null, new Rect(new Point(), bounds.Size));
        //    }

        //    rtb.Render(dv);

        //    PngBitmapEncoder png = new PngBitmapEncoder();

        //    png.Frames.Add(BitmapFrame.Create(rtb));

        //    using (Stream stm = File.Create(path))
        //    {
        //        png.Save(stm);
        //    }
        //}

        private void Run_Click(object sender, RoutedEventArgs e)
        {
            GenerateNetwork();
            vtn.ErrorLogger = this;
            if (currentRoot == null)
            {
                MessageBox.Show("No root node selected!");
                return;
            }

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
            //result = result.ContinueWithAsync("int i = 0, j = 1; char a = 'a'; bool b = true, c = false;", scriptOptions).Result;
            ActionNode.ScriptState = result;
            ActionNode.ScriptOptions = scriptOptions;
            ActionNode.InputHandler = new InputHandler();
            ActionNode.OutputHandler = new OutputHandler();

            // Start network parsing in new thread. This is necessary in order to highlight the nodes
            // in the GUI using Dispatcher.Invoke()
            Thread t = new Thread(() =>
            {
                vtn.Evaluate();
                //FlowCodeInfrastructure.Node.Run(vtn.RootNode);
            });
            //try
            //{
                t.Start();
            //}
            //catch(Exception ex)
            //{
            //    if (!Directory.Exists("./dump"))
            //        Directory.CreateDirectory("./dump");
            //    XMLWriter.SaveXML("./dump/error.vtdump", edges);
            //}
        }

        private void LogError(string messsage)
        {
            if (!Directory.Exists("./dump"))
                    Directory.CreateDirectory("./dump");
                XMLWriter.SaveXML("./dump/error.vtdump", edges);
        }

        private void MenuItemExit_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void MenuItemExport_Click(object sender, RoutedEventArgs e)
        {
            var sfd = new SaveFileDialog();
            sfd.DefaultExt = "png";
            sfd.Filter = "PNG Files|*.png |All files (*.*)|*.*";
            if (sfd.ShowDialog() == true)
            {
                SaveBitmap(sfd.FileName);
            }
        }


        private void MenuItemLoad_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.DefaultExt = "vtn";
            if (ofd.ShowDialog() == true)
            {
                LoadXML(ofd.FileName);
            }
        }


        private void LoadXML(string fname)
        {
            XDocument doc = XDocument.Load(fname);
            Console.WriteLine(doc);
            // Parse document and generate Nodes and edges

            string rootId = string.Empty;
            var potentialRoot = doc.Root.Elements("Root").FirstOrDefault();
            if (!(potentialRoot is null))
            {
                rootId = potentialRoot.Attribute("ID")?.Value;
            }

            foreach (var c in doc.Root.Elements("Node"))
            {
                //Console.WriteLine(c);
                var id = c.Attribute("ID").Value;
                var code = c.Attribute("Code").Value;
                var position = Point.Parse(c.Attribute("Position").Value);
                var nodeType = c.Attribute("Type")?.Value;
                NodeControlBase node; //= new RectangleNodeControl();

                switch (nodeType)
                {
                    case "Sequence":
                        node = new SequenceNodeControl();
                        break;
                    case "Decision":
                        node = new DecisionNodeControl();
                        break;
                    case "PredefinedProcess":
                        node = new ProcessNodeControl();
                        break;
                    case "Terminal":
                        var fName = c.Attribute("FunctionName")?.Value;
                        var returnVariable = c.Attribute("ReturnVariable")?.Value;
                        var inputVariables = c.Attribute("InputVariables")?.Value;
                        node = new TerminalNodeControl()
                        {
                            FunctionName = fName,
                            ReturnVariable = returnVariable,
                            InputVariables = inputVariables
                        };

                        break;
                    default:
                        node = new SequenceNodeControl();
                        break;
                }

                Guid newGuid;
                if (!Guid.TryParse(id, out newGuid))
                {
                    newGuid = Guid.NewGuid();
                }

                node.NodeData = new Controls.Node()
                {
                    Id = newGuid,
                    Title = code,
                    Position = position
                };
                node.Width = 60;
                node.Height = 40;


                if (rootId != string.Empty)
                    if (node.NodeData.Id == Guid.Parse(rootId))
                    {
                        Node_RootRequested(node, EventArgs.Empty);
                    }
                AddNode(node, position);
                DiagramCanvas.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                DiagramCanvas.Arrange(new Rect(
                0, 0,
                this.Width,
                this.Height));
            }


            InvalidateVisual();
            LoadEdges(doc);
            InvalidateVisual();
            UpdateTerminals();

        }

        private void LoadEdges(XDocument doc)
        {
            foreach (var e in doc.Root.Elements("Edge"))
            {
                var from = canvasNodes.Where(x => x.NodeData.Id.ToString() == e.Attribute("From").Value).FirstOrDefault();
                var to = canvasNodes.Where(x => x.NodeData.Id.ToString() == e.Attribute("To").Value).FirstOrDefault();
                var from_idx = int.Parse(e.Attribute("FromIndex").Value);
                var to_idx = int.Parse(e.Attribute("ToIndex").Value);
                var lbl = e.Attribute("Label").Value;

                InsertEdge(from, to, from_idx, to_idx, lbl);

            }
        }

        private void UpdateTerminals()
        {
            foreach (TerminalNodeControl tnc in canvasNodes.Where(x => x.GetType() == typeof(TerminalNodeControl)))
            {
                if (tnc.FunctionName != string.Empty)
                    tnc.TerminalType = "Start";
                else
                    tnc.TerminalType = "End";
            }
        }

        private void InsertEdge(NodeControlBase? from, NodeControlBase? to, int from_idx, int to_idx, string lbl)
        {
            EdgeControl newEdge = new EdgeControl()
            {
                Label = lbl,
                ToIndex = to_idx,
                FromIndex = from_idx,
                To = to,
                From = from
            };


            from.RegisterOutputEdge(from_idx, newEdge);

            newEdge.DeleteRequested += Edge_DeleteRequested;

            //from.NodeMoved += (s, ev) => UpdateEdges();
            //to.NodeMoved += (s, ev) => UpdateEdges();
            edges.Add(newEdge);
            DiagramCanvas.Children.Add(newEdge);
            GenerateEdgeLabel(newEdge);
        }


        private void MenuItemSave_Click(object sender, RoutedEventArgs e)
        {
            var sfd = new SaveFileDialog();
            sfd.DefaultExt = "vtn";
            sfd.Filter = "Visual Tree Network Files|*.vtn|All files (*.*)|*.*";
            if (sfd.ShowDialog() == true)
            {
                XMLWriter.SaveXML(sfd.FileName, edges);
            }
        }

        public void OnCompleted()
        {
            throw new NotImplementedException();
        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }


        public void OnNext(string value)
        {
            Console.Write("Message to observers: " + value);
        }

        private void DiagramCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            // Determine the direction of the zoom (in or out)
            bool zoomIn = e.Delta > 0;

            // Set the scale value based on the direction of the zoom
            scaleValue += zoomIn ? 0.1 : -0.1;

            // Set the maximum and minimum scale values
            scaleValue = scaleValue < 0.1 ? 0.1 : scaleValue;
            scaleValue = scaleValue > 10.0 ? 10.0 : scaleValue;

            // Apply the scale transformation to the ItemsControl
            ScaleTransform scaleTransform = new ScaleTransform(scaleValue, scaleValue);
            DiagramCanvas.LayoutTransform = scaleTransform;
        }

        private void MenuItemNew_Click(object sender, RoutedEventArgs e)
        {
            foreach (var edge in edges.ToList())
            {
                edge.From?.UnregisterOutputEdge(edge.FromIndex ?? 0);
                if (edge.LabelBox != null)
                {
                    DiagramCanvas.Children.Remove(edge.LabelBox);
                    edge.LabelBox = null;
                }
                DiagramCanvas.Children.Remove(edge);
                edges.Remove(edge);
            }

            foreach (var node in canvasNodes.ToList())
            {
                DiagramCanvas.Children.Remove(node);
            }

        }

        private void Preferences_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow sw = new SettingsWindow();
            sw.ShowDialog();
        }

        private void DeleteZone_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(FrameworkElement)))
            {
                e.Effects = DragDropEffects.Move;
                //DeleteZone.Background = Brushes.Red;
                //DeleteZoneText.Text = "Release to delete";
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void DeleteZone_DragLeave(object sender, DragEventArgs e)
        {

        }

        private void DeleteZone_DragOver(object sender, DragEventArgs e)
        {

        }

        private void DeleteZone_Drop(object sender, DragEventArgs e)
        {

        }

        void IErrorLogger.LogError(string messsage)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                LogError(messsage);
            });
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // Note that you can have more than one file.
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                foreach (string f in files)
                {

                    if (f.EndsWith(".vtn"))
                        LoadXML(f);
                    else if (f == "field.json") // Maybe not smart
                    {
                        File.Replace(f, "field.json", "field.json.old");
                    }
                }
            }
        }
    }
}


