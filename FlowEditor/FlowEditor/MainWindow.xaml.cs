// MainWindow.xaml.cs
using CargoTrucker;
using CargoTrucker.Client;
using FlowCodeInfrastructure;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Win32;
using FlowEditor.Controls;
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
using System.Configuration;
using FlowEditor.Windows;
using System.Security.Cryptography.Xml;
using System.Numerics;
using System.Printing;

// TODO: Drag Nodes onto canvas
// TODO: draw onTrue edge green, onFalse edge red
// TODO: consider connecting nodes by clicking rather than dragging an edge
// TODO: Think about following the currently active node with the camera during flowchart evaluation
// TODO: Load code snippets from file

namespace FlowEditor
{
    public partial class MainWindow : Window, IErrorLogger
    {
        /// <summary>
        /// Vertical spacing (in pixels) used when auto-placing newly added nodes below the last one.
        /// </summary>
        private int verticalOffset = 25;

        /// <summary>
        /// Incremental counter used to generate unique node titles and default positions.
        /// </summary>
        private int nodeCount = 0;

        /// <summary>
        /// Node where an interactive edge creation (drag) was started.
        /// </summary>
        private NodeControlBase edgeStartNode = null;

        /// <summary>
        /// Connection point index on the start node for the interactive edge creation.
        /// </summary>
        private int? edgeStartIndex = null;

        /// <summary>
        /// Temporary edge shown while the user is dragging to connect two nodes.
        /// </summary>
        private EdgeControl temporaryEdge = null;

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
        private Point lastNodePosition;

        /// <summary>
        /// Current zoom scale applied to the diagram canvas via LayoutTransform.
        /// </summary>
        private double scaleValue = 1.0f;

        /// <summary>
        /// Represents the internal deletion zone used for managing resource cleanup operations.
        /// </summary>
        private DeleteZone deletionZone;

        /// <summary>
        /// Provides access to the output logger used for recording or displaying log messages.
        /// </summary>
        /// <remarks>This field is intended for internal use to manage logging output. Use the associated
        /// public properties or methods to interact with logging functionality.</remarks>
        private OutputControl outputLogger;

        /// <summary>
        /// 
        /// </summary>
        private OutputControl variableLogger;

        /// <summary>
        /// 
        /// </summary>
        private OutputControl callStack;

        public MainWindow()
        {
            ShowTipsScreen();

            this.WindowState = WindowState.Maximized;
            InitializeComponent();
            lastNodePosition = new Point(Application.Current.MainWindow.Width / 2, 15);
            //DeleteZone 
            deletionZone = new DeleteZone();
            //dz.DragEnter += dz.OnDragEnter;
            deletionZone.MouseEnter += Dz_MouseEnter;
            deletionZone.MouseLeave += Dz_MouseLeave;
            deletionZone.MouseDown += DeletionZone_MouseDown;
            deletionZone.Visibility = Visibility.Hidden;

            Canvas.SetLeft(deletionZone, 400);
            Canvas.SetTop(deletionZone, 400);
            Overlay.Children.Add(deletionZone);

            Config.SetKeyWord(Config.KeyWord.True, "Ja");
            Config.SetKeyWord(Config.KeyWord.False, "Nein");
            Config.SetKeyWord(Config.KeyWord.Function, "Function");
            Config.SetKeyWord(Config.KeyWord.Input, "Eingabe");
            Config.SetKeyWord(Config.KeyWord.Output, "Ausgabe");
            Config.Load();

            DiagramCanvas.MouseDown += MainWindow_MouseDown;
            DiagramCanvas.MouseRightButtonDown += DiagramCanvas_MouseRightButtonDown;
            DiagramCanvas.MouseRightButtonUp += DiagramCanvas_MouseRightButtonUp;
            //DiagramCanvas.MouseMove += DiagramCanvas_MouseMove;
            DiagramCanvas.MouseMove += DiagramCanvasMove;

            outputLogger = new OutputControl();
            Overlay.Children.Add(outputLogger);
            outputLogger.Width=400;

            variableLogger = new OutputControl();
            Overlay.Children.Add(variableLogger);

            callStack = new OutputControl();
            Overlay.Children.Add(callStack);

            FlowCodeInfrastructure.Node.VariableLogger = variableLogger;
            FlowCodeInfrastructure.CallerNode.StackDisplay = callStack;

            outputLogger.Visibility = Visibility.Hidden;
            variableLogger.Visibility = Visibility.Hidden;
            callStack.Visibility = Visibility.Hidden;


            LoadSettingsFile();

            SequenceNodeDetailWindow.Load("./Snippets/ActionNode.snippets");
        }

        void LoadSettingsFile()
        {
            if (File.Exists("./config/settings.config"))
            {
                var lines = File.ReadAllLines("./config/settings.config");
                foreach (var line in lines)
                {
                    var parts = line.Split(':');
                    if (parts.Length == 2)
                    {
                        ConfigurationManager.AppSettings[parts[0].Trim()] = parts[1].Trim();
                    }
                }
            }
        }

        private static void ShowTipsScreen()
        {
            WelcomeWindow ww = new WelcomeWindow("./res/tips.json");
            ww.ShowDialog();
        }

        private void DiagramCanvas_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            isPanning = false;
        }

        Point panStart;
        bool isPanning = false;
        private void DiagramCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
           if (!isPanning)
           {
                isPanning = true;
                panStart = e.GetPosition(DiagramCanvas);
           }
            else
            {
                var pos = e.GetPosition(DiagramCanvas);
                Point translate = new Point(panStart.X - pos.X,
                    panStart.Y - pos.Y);
                PanNodes(translate.X, translate.Y);
                panStart = pos;
            }
        }

        private void PositionOutput()
        {
            outputLogger.Width = Overlay.ActualWidth / 5;
            Canvas.SetLeft(outputLogger, Overlay.ActualWidth - 300);
            Canvas.SetTop(outputLogger, 60);

            Canvas.SetLeft(variableLogger, Overlay.ActualWidth - 500);
            Canvas.SetTop(variableLogger, 60);

            Canvas.SetLeft(callStack, Overlay.ActualWidth - 700);
            Canvas.SetTop(callStack, 60);

            variableLogger.SetTitle("Variables");
            variableLogger.Background = Brushes.Blue;

            callStack.SetTitle("Call Stack");
            callStack.Background = Brushes.Green;
        }

        private void DeletionZone_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DeleteSelectedNode();
            ToggleDeleteZone(false);
        }

        private void MainWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            NodeControlBase.LastSelected?.SetActive(false);
            deletionZone.Visibility = Visibility.Hidden;
        }

        public void ToggleDeleteZone(bool active)
        {
            if (active)
                deletionZone.Visibility = Visibility.Visible;
            else
                deletionZone.Visibility = Visibility.Hidden;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            ResetDeletionZone();
            PositionOutput();
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            ResetDeletionZone();
        }

        private void ResetDeletionZone()
        {
            Canvas.SetLeft(deletionZone, Overlay.ActualWidth - deletionZone.ActualWidth);
            Canvas.SetTop(deletionZone, Overlay.ActualHeight - deletionZone.ActualHeight);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                DeleteSelectedNode();
                ToggleDeleteZone(false);
            }
            else if (e.Key==Key.F2)
            {
                GatherFunctions();
                OpenDetailWindow();
            }
            base.OnKeyDown(e);
        }

        private void GatherFunctions()
        {
            try
            {
                var signatures = canvasNodes.Where(x => x.GetType() == typeof(TerminalNodeControl)).
                    Where(x=> string.IsNullOrWhiteSpace((x as TerminalNodeControl).FunctionName) == false)
                    .Select(x => new KeyValuePair<string, string>((x as TerminalNodeControl).FunctionName,
                    $"{(x as TerminalNodeControl).FunctionName}({string.Join(",", (x as TerminalNodeControl).InputVariables)})")).ToDictionary<string, string>();

                ProcessNodeDetailWindow.Signatures = signatures;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        private void DeleteSelectedNode()
        {
            List<EdgeControl> edgesToDelete = new();
            foreach (var edge in edges.Where(x => x.From == NodeControlBase.LastSelected || x.To == NodeControlBase.LastSelected))
            {
                var from = edge.From;
                var to = edge.To;
                var fidx = edge.FromIndex;
                var tidx = edge.ToIndex;

                if (fidx is not null)
                    from.UnregisterOutputEdge((int)fidx);
                if (tidx is not null)
                    to.UnregisterOutputEdge((int)tidx);

                edgesToDelete.Add(edge);
            }

            foreach (var edge in edgesToDelete)
            {
                edges.Remove(edge);
                DiagramCanvas.Children.Remove(edge.StateButton);
                DiagramCanvas.Children.Remove(edge.LabelBox);
                DiagramCanvas.Children.Remove(edge);
            }

            DiagramCanvas.Children.Remove(NodeControlBase.LastSelected);
            canvasNodes.Remove(NodeControlBase.LastSelected);

            if (NodeControlBase.LastSelected is TerminalNodeControl tn)
            {
                if (ProcessNodeDetailWindow.FunctionNames is not null)
                    ProcessNodeDetailWindow.FunctionNames.Remove(tn.FunctionName);
            }

            NodeControlBase.LastSelected = null;
            ToggleDeleteZone(false);
        }

        private void Dz_MouseEnter(object sender, MouseEventArgs e)
        {
            deletionZone.BackgroundColor = Brushes.Red;
        }

        private void Dz_MouseLeave(object sender, MouseEventArgs e)
        {
            deletionZone.BackgroundColor = Brushes.Transparent;
        }

        private void AddNode(NodeControlBase node, Point? nodePosition)
        {
            Point position;
            if (nodePosition != null)
            {
                position = (Point)nodePosition;
                if (NodeControlBase.LastSelected is not null)
                    NodeControlBase.LastSelected.SetActive(false);
            }
            else
            {
                if (NodeControlBase.LastSelected is not null)
                {
                    verticalOffset = (int)(NodeControlBase.LastSelected.ActualHeight * 1.25);
                    position.X = Canvas.GetLeft(NodeControlBase.LastSelected);
                    position.Y = Canvas.GetTop(NodeControlBase.LastSelected) + verticalOffset;
                    NodeControlBase.LastSelected.SetActive(false);
                }
                else
                {
                    position = new Point(
                    lastNodePosition.X,
                    lastNodePosition.Y + verticalOffset);
                }
                
                node.NodeData.Position = position;
            }
            NodeControlBase.LastSelected = node;
            lastNodePosition = position;

            Canvas.SetLeft(node, position.X);
            Canvas.SetTop(node, position.Y);

            node.ConnectionPointClicked += Node_ConnectionPointClicked;
            node.NodeMoved += (s, e) => CheckAndUpdateEdges();
            node.RootRequested += Node_RootRequested;
            node.ToggleDeletionZone += (s, e) => ToggleDeleteZone(true);
            //node.MouseDoubleClick += node.
            DiagramCanvas.Children.Add(node);
            canvasNodes.Add(node);
            node.SetActive(true);
        }

        private void CheckAndUpdateEdges()
        {
            UpdateEdges();
            foreach (EdgeControl e in edges)
                CheckSelfIntersection(e);
        }

        private NodeControlBase currentRoot;

        private void Node_RootRequested(object sender, EventArgs e)
        {
            if (currentRoot != null)
            {
                currentRoot.Background = currentRoot.OriginalBackground;
                currentRoot.BorderBrush = Brushes.Black;
                currentRoot.BorderThickness = new Thickness(0.0);
                currentRoot.IsRoot = false;
                NodeControlBase.LastSelected = null;
                ToggleDeleteZone(false);
            }

            currentRoot = sender as NodeControlBase;

            if (currentRoot != null)
            {
                currentRoot.BorderBrush = Brushes.OrangeRed;
                currentRoot.BorderThickness = new Thickness(9.0);
                currentRoot.IsRoot = true;
                NodeControlBase.LastSelected = null;
                ToggleDeleteZone(false);
            }
        }

        private void AddDecisionNode_Click(object sender, RoutedEventArgs e)
        {
            GetNodeColorColor("DecisionNodeColor", out byte alpha, out byte r, out byte g, out byte b);
            GetNodeColorColor("DecisionTextColor", out byte alphaText, out byte rText, out byte gText, out byte bText);

            var decisionNode = new DecisionNodeControl
            {
                Width = 180,
                Height = 160,
                OriginalBackground = new SolidColorBrush(Color.FromArgb(alpha, r, g, b)),//Color.FromArgb(0xff, 0xf6, 0xae, 0x2d)),,
                Foreground = new SolidColorBrush(Color.FromArgb(alphaText, rText, gText, bText)),
                NodeData = new Controls.Node
                {
                    Title = $"Decision Node({nodeCount++})",
                }
            };

            AddNode(decisionNode, null); // decisionNode.NodeData.Position);
        }

        private static void GetNodeColorColor(string nodeType, out byte alpha, out byte r, out byte g, out byte b)
        {
            var nodeColor = ConfigurationManager.AppSettings[nodeType];
            alpha = byte.Parse(nodeColor[..2], NumberStyles.HexNumber);
            r = byte.Parse(nodeColor.Substring(2, 2), NumberStyles.HexNumber);
            g = byte.Parse(nodeColor.Substring(4, 2), NumberStyles.HexNumber);
            b = byte.Parse(nodeColor.Substring(6, 2), NumberStyles.HexNumber);
        }

        private void AddNode_Click(object sender, RoutedEventArgs e)
        {
            GetNodeColorColor("SequenceNodeColor", out byte alpha, out byte r, out byte g, out byte b);
            GetNodeColorColor("SequenceTextColor", out byte alphaText, out byte rText, out byte gText, out byte bText);

            var node = new SequenceNodeControl
            {
                Width = 180,
                Height = 90,
                OriginalBackground = new SolidColorBrush(Color.FromArgb(alpha, r,g,b)),// Color.FromArgb(0xff, 0x33, 0x65, 0x8a)),
                Foreground = new SolidColorBrush(Color.FromArgb(alphaText, rText, gText, bText)),
                NodeData = new Controls.Node
                {
                    Title = $"Node({nodeCount++})",
                }
            };
            AddNode(node, null);
        }

        private void Node_ConnectionPointClicked(object sender, ConnectionPointClickedEventArgs e)
        {
            if (temporaryEdge != null)
            {
                DiagramCanvas.Children.Remove(temporaryEdge);
                temporaryEdge = null;
            }
            
            edgeStartNode = e.Node;
            edgeStartIndex = e.ConnectionPointIndex;

            if (edgeStartNode != null && edgeStartIndex.HasValue)
            {
                edgeStartNode.UnregisterOutputEdge(edgeStartIndex.Value);

                var existing = DiagramCanvas.Children
                    .OfType<EdgeControl>()
                    .FirstOrDefault(edge => edge.From == edgeStartNode && edge.FromIndex == edgeStartIndex);

                if (existing != null)
                {
                    DiagramCanvas.Children.Remove(existing);

                    if (existing.LabelBox != null)
                    {
                        DiagramCanvas.Children.Remove(existing.StateButton);
                        DiagramCanvas.Children.Remove(existing.LabelBox);
                        existing.LabelBox = null;
                        existing.StateButton = null;
                    }

                    edges.Remove(existing);
                }
            }

            temporaryEdge = new EdgeControl
            {
                From = edgeStartNode,
                FromIndex = edgeStartIndex,
                To = null
            };
            Panel.SetZIndex(temporaryEdge, -1);

            temporaryEdge.Label = string.Empty;
            DiagramCanvas.Children.Add(temporaryEdge);

            MouseMove += MainWindow_MouseMove;
            MouseLeftButtonUp += MainWindow_MouseLeftButtonUp;
        }

        private void DiagramCanvasMove(object sender, MouseEventArgs e)
        {
                if (e.RightButton == MouseButtonState.Pressed)
                {
                    if (!isPanning)
                    {
                        isPanning = true;
                        panStart = e.GetPosition(DiagramCanvas);
                    }
                    else
                    {
                        var pos = e.GetPosition(DiagramCanvas);
                        Point translate = new Point(panStart.X - pos.X,
                            panStart.Y - pos.Y);
                        PanNodes(-translate.X, -translate.Y);
                        panStart = pos;
                    }
                }
        }

        private void MainWindow_MouseMove(object sender, MouseEventArgs e)
        {
            if (temporaryEdge != null)
            {
                Point pos = e.GetPosition(DiagramCanvas);
                temporaryEdge.CurrentMousePosition = pos;
                temporaryEdge.InvalidateVisual();
            }
        }

        private void AddTerminatorNode_Click(object sender, RoutedEventArgs e)
        {
            GetNodeColorColor("TerminatorNodeColor", out byte alpha, out byte r, out byte g, out byte b);
            GetNodeColorColor("TerminatorTextColor", out byte alphaText, out byte rText, out byte gText, out byte bText);

            var terminator = new TerminalNodeControl
            {
                Width = 240,
                Height = 160,
                OriginalBackground = new SolidColorBrush( Color.FromArgb(alpha,r,g,b) ),//Color.FromArgb(0xff, 0xf2, 0x64, 0x19)),
                Foreground = new SolidColorBrush(Color.FromArgb(alphaText, rText, gText, bText)),
                NodeData = new Controls.Node
                {
                    Title = "Start/End"
                },
                TerminalType = "Start"
            };

            AddNode(terminator, null);
        }
        private void AddFunctionNode_Click(object sender, RoutedEventArgs e)
        {
            GetNodeColorColor("ProcessNodeColor", out byte alpha, out byte r, out byte g, out byte b);
            GetNodeColorColor("ProcessTextColor", out byte alphaText, out byte rText, out byte gText, out byte bText);

            var processNode = new ProcessNodeControl()
            {
                Width = 180,
                Height = 90,
                OriginalBackground = new SolidColorBrush(Color.FromArgb(alpha,r,g,b)), //0xff, 0x86, 0xbb, 0xd8)),
                Foreground = new SolidColorBrush(Color.FromArgb(alphaText, rText, gText, bText)),
                NodeData = new Controls.Node
                {
                    Title = $"ProcessCall({nodeCount++})"
                }
            };
            AddNode(processNode, null);
        }

        private void MainWindow_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var localTempEdge = temporaryEdge; // Kopie

            if (localTempEdge == null || edgeStartNode == null || !edgeStartIndex.HasValue)
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
                localTempEdge.Label = Config.GetKeyword(Config.KeyWord.True);
                if (localTempEdge.To == localTempEdge.From)
                {
                    DiagramCanvas.Children.Remove(localTempEdge);
                    CleanupTemporaryEdge();
                    return;
                }

                edgeStartNode.RegisterOutputEdge(edgeStartIndex.Value, localTempEdge);

                edges.Add(localTempEdge);
                localTempEdge.DeleteRequested += Edge_DeleteRequested;

                edgeStartNode.NodeMoved += (s, ev) => UpdateEdges();
                targetNode.NodeMoved += (s, ev) => UpdateEdges();

                // Label erzeugen
                GenerateEdgeLabel(localTempEdge);
                CheckSelfIntersection(localTempEdge);
            }
            else
            {
                // Kein Node getroffen -> temporäre Edge entfernen
                DiagramCanvas.Children.Remove(localTempEdge);
            }

            CleanupTemporaryEdge();
        }

        // Check wheter edge intersects origin node;
        // if yes -> reroute towards nearest canvas edge
        private void CheckSelfIntersection(EdgeControl localTempEdge)
        {
            var connectionsOut = localTempEdge.From.GetConnectionPoints();
            //var connectionsOut = _edgeStartNode.GetConnectionPoints();
            var connectionsIn = localTempEdge.To.GetConnectionPoints();

            double nodeX1 = Canvas.GetLeft(localTempEdge.From);
            double nodeY1 = Canvas.GetTop(localTempEdge.From);
            double nodeX2 = nodeX1 + localTempEdge.From.ActualWidth;
            double nodeY2 = nodeY1 + localTempEdge.From.ActualHeight;

            double nodeX3 = Canvas.GetLeft(localTempEdge.To);
            double nodeY3 = Canvas.GetTop(localTempEdge.To);
            double nodeX4 = nodeX3 + localTempEdge.To.ActualWidth;
            double nodeY4 = nodeY3 + localTempEdge.To.ActualHeight;

            Point lineStart = new Point(
                nodeX1 + connectionsOut[(int)localTempEdge.FromIndex].X,
                nodeY1 + connectionsOut[(int)localTempEdge.FromIndex].Y
                );

            Point lineEnd = new Point(
                nodeX3 + connectionsIn[(int)localTempEdge.ToIndex].X,
                nodeY3 + connectionsIn[(int)localTempEdge.ToIndex].Y
                );

            List<Point> existingPoints = new List<Point>(localTempEdge.ControlPoints);
            existingPoints.Insert(0, lineStart);
            existingPoints.Add(lineEnd);
            bool reRoute = false;
            for (int i = 1; i < existingPoints.Count; i++)
            {
                bool bottomHit = LineLineIntersection(
                    existingPoints[i - 1],
                    existingPoints[i],
                    new Point(nodeX3, nodeY4),
                    new Point(nodeX4, nodeY4)
                    );

                bool bottomHitTargetNode = LineLineIntersection(
                    existingPoints[i - 1],
                    existingPoints[i],
                    new Point(nodeX1, nodeY2),
                    new Point(nodeX2, nodeY2)
                    );

                if (bottomHit ||bottomHitTargetNode)
                {
                    reRoute = true;
                }
                //double dx = localTempEdge.ControlPoints[i].X - localTempEdge.ControlPoints[i-1].X;
                //double dy = localTempEdge.ControlPoints[i].Y - localTempEdge.ControlPoints[i - 1].Y;
                // This is a straight line. 
            }

            // TODO: At some point, try to minimize / optimize rms distance from controlpoints to edge
            if (reRoute) 
            {
                localTempEdge.ControlPoints.Clear();
                RerouteEdge(localTempEdge);
            }

            UpdateEdges();
        }

        private void RerouteEdge(EdgeControl localTempEdge)
        {
            var connectionsOut = localTempEdge.From.GetConnectionPoints();
            var connectionsIn = localTempEdge.To.GetConnectionPoints();

            double nodeX1 = Canvas.GetLeft(localTempEdge.From);
            double nodeY1 = Canvas.GetTop(localTempEdge.From);
            double nodeX2 = nodeX1 + localTempEdge.From.ActualWidth;
            double nodeY2 = nodeY1 + localTempEdge.From.ActualHeight;

            double nodeX3 = Canvas.GetLeft(localTempEdge.To);
            double nodeY3 = Canvas.GetTop(localTempEdge.To);
            double nodeX4 = nodeX3 + localTempEdge.To.ActualWidth;
            double nodeY4 = nodeY3 + localTempEdge.To.ActualHeight;

            Point lineStart = new Point(
                nodeX1 + connectionsOut[(int)localTempEdge.FromIndex].X,
                nodeY1 + connectionsOut[(int)localTempEdge.FromIndex].Y
                );

            Point lineEnd = new Point(
                nodeX3 + connectionsIn[(int)localTempEdge.ToIndex].X,
                nodeY3 + connectionsIn[(int)localTempEdge.ToIndex].Y
                );


            int goRight = (lineStart.X > lineEnd.X) ? 1:-1;

            // Avoid magic numbers
            double xOffset = Math.Max(localTempEdge.From.ActualWidth, localTempEdge.To.ActualWidth) * 2 / 3;

            Point down = new Point(lineStart.X, lineStart.Y + 30);
            Point downRight = new Point(down.X + xOffset * goRight, down.Y);
            Point aboveRight = new Point(downRight.X, lineEnd.Y - 30);
            Point above = new Point(lineEnd.X, lineEnd.Y - 30);

            localTempEdge.ControlPoints.Add(down);
            localTempEdge.ControlPoints.Add(downRight);
            localTempEdge.ControlPoints.Add(aboveRight);
            localTempEdge.ControlPoints.Add(above);
        }

        private bool LineLineIntersection(Point p1, Point p2, Point p3, Point p4)
        {

            double t = ((p1.X - p3.X) * (p3.Y - p4.Y) - (p1.Y - p3.Y) * (p3.X - p4.X)) / ((p1.X - p2.X) * (p3.Y - p4.Y) - (p1.Y - p2.Y) * (p3.X - p4.X));
            double u = (-1) * ((p1.X - p2.X) * (p1.Y - p3.Y) - (p1.Y - p2.Y) * (p1.X - p3.X)) / ((p1.X - p2.X) * (p3.Y - p4.Y) - (p1.Y - p2.Y) * (p3.X - p4.X));

            // if uA and uB are between 0-1, lines are colliding
            if (t >= 0 && t <= 1 && u >= 0 && u <= 1)
                return true;

            return false;
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

            var stateButton = new Button()
            {
                Content = localTempEdge.Label,
                Width = 80,
                Background = Brushes.White,
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(1),
                FontSize = 12,
                Padding = new Thickness(2),
                Visibility = Visibility.Hidden
            };

            // Position Label
            var start = localTempEdge.From.TranslatePoint(
                localTempEdge.From.GetConnectionPoints()[localTempEdge.FromIndex ?? 0], DiagramCanvas);
            var end = localTempEdge.To.TranslatePoint(
                localTempEdge.To.GetConnectionPoints()[localTempEdge.ToIndex ?? 0], DiagramCanvas);

            var labelPos = new Point((start.X + end.X) / 2 - 40, (start.Y + end.Y) / 2 - 10);

            Canvas.SetLeft(labelBox, labelPos.X);
            Canvas.SetTop(labelBox, labelPos.Y);

            Canvas.SetLeft(stateButton, labelPos.X);
            Canvas.SetTop(stateButton, labelPos.Y);


            labelBox.TextChanged += (s, ev) => localTempEdge.Label = labelBox.Text;
            stateButton.Click += localTempEdge.StateButtonClicked;

            DiagramCanvas.Children.Add(labelBox);
            DiagramCanvas.Children.Add(stateButton);

            localTempEdge.LabelBox = labelBox;
            localTempEdge.StateButton = stateButton;

        }

        private void CleanupTemporaryEdge()
        {
            temporaryEdge = null;
            edgeStartNode = null;
            edgeStartIndex = null;

            MouseMove -= MainWindow_MouseMove;
            MouseLeftButtonUp -= MainWindow_MouseLeftButtonUp;
        }

        /// <summary>
        /// Updates the visual representation and label positions of all edges in the diagram.
        /// </summary>
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

                if (edge.LabelBox != null)
                {
                    DiagramCanvas.Children.Remove(edge.StateButton);
                    DiagramCanvas.Children.Remove(edge.LabelBox);
                    edge.LabelBox = null;
                }
                DiagramCanvas.Children.Remove(edge);

                edges.Remove(edge);
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

        private void Run_Click(object sender, RoutedEventArgs e)
        {
            outputLogger?.Reset();
            GenerateNetwork();
            if (currentRoot == null)
            {
                MessageBox.Show("No root node selected!");
                return;
            }
            StopButton.Visibility = Visibility.Visible;
            vtn.ErrorLogger = this;

            NodeControlBase.LastSelected?.SetActive(false);
            deletionZone.Visibility = Visibility.Hidden;

            ScriptOptions scriptOptions = ScriptOptions.Default;
            FlowCodeInfrastructure.Network.ResetCargoTrucker();

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
            ActionNode.OutputHandler = outputLogger;// new OutputHandler();

            //SequenceNodeControl.DetailWindow = new SequenceNodeDetailWindow();

            outputLogger.Visibility = Visibility.Visible;
            variableLogger.Visibility = Visibility.Visible;
            callStack.Visibility = Visibility.Visible;

            // Start network parsing in new thread. This is necessary in order to highlight the nodes
            // in the GUI using Dispatcher.Invoke()
            Thread t = new Thread(() =>
            {
                vtn.Evaluate();
                Application.Current.Dispatcher.Invoke(() =>
                {
                    OnEvaluationFinished();
                });
            });
            t.Start();
        }

        private void OnEvaluationFinished()
        {
            StopButton.Visibility = Visibility.Collapsed;
        }

        private void LogError(string message)
        {
            var c = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Encountered an error:");
            Console.WriteLine(message);
            Console.WriteLine("Dumped network to file");
            Console.ForegroundColor = c;

            MessageBox.Show(message);
            File.AppendAllText("./error.log", $"\n{DateTime.Now.ToShortTimeString()}: - {message}");


            // check string against regex to see whether it contains the error code
            // the error code is always preceded by "error"


            outputLogger?.ShowOutput($"Fehler: {message}");

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
            //Console.WriteLine(doc);
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
                var id = c.Attribute("ID")?.Value;
                var code = c.Attribute("Code")?.Value;
                var position = Point.Parse(c.Attribute("Position")?.Value);
                var nodeType = c.Attribute("Type")?.Value;
                NodeControlBase node; //= new RectangleNodeControl();

                byte alpha = 0, r = 0, g = 0, b = 0;
                byte alphaText = 0, rText = 0, gText = 0, bText = 0;
                switch (nodeType)
                {
                    case "Sequence":
                        node = new SequenceNodeControl()
                        {
                            Width = 180,
                            Height = 90
                        };
                        GetNodeColorColor("SequenceNodeColor", out alpha, out r, out g, out b);
                        GetNodeColorColor("SequenceTextColor", out alphaText, out rText, out gText, out bText);
                        node.Foreground = new SolidColorBrush(Color.FromArgb(alphaText, rText, gText, bText));
                        node.OriginalBackground = new SolidColorBrush(Color.FromArgb(alpha, r, g, b));
                        break;
                    case "Decision":
                        node = new DecisionNodeControl()
                        {
                            Width = 180,
                            Height = 160,
                        };
                        GetNodeColorColor("DecisionNodeColor", out alpha, out r, out  g, out b);
                        GetNodeColorColor("DecisionTextColor", out alphaText, out rText, out gText, out bText);
                        node.Foreground = new SolidColorBrush(Color.FromArgb(alphaText, rText, gText, bText));
                        node.OriginalBackground = new SolidColorBrush(Color.FromArgb(alpha, r, g, b));

                        break;
                    case "PredefinedProcess":
                        node = new ProcessNodeControl()
                        {
                            Width = 180,
                            Height = 90
                        };
                        
                        GetNodeColorColor("ProcessNodeColor", out alpha, out r, out g, out b);
                        GetNodeColorColor("ProcessTextColor", out alphaText, out rText, out gText, out bText);
                        node.Foreground = new SolidColorBrush(Color.FromArgb(alphaText, rText, gText, bText));
                        node.OriginalBackground = new SolidColorBrush(Color.FromArgb(alpha, r, g, b));
                        
                        break;
                    case "Terminal":
                        var fName = c.Attribute("FunctionName")?.Value;
                        var returnVariable = c.Attribute("ReturnVariable")?.Value;
                        var inputVariables = c.Attribute("InputVariables")?.Value;
                        node = new TerminalNodeControl()
                        {
                            FunctionName = fName,
                            ReturnVariable = returnVariable,
                            InputVariables = inputVariables,
                            Width = 240,
                            Height = 160
                        };

                        GetNodeColorColor("TerminatorNodeColor", out alpha, out r, out g, out b);
                        GetNodeColorColor("TerminatorTextColor", out alphaText, out rText, out gText, out bText);
                        node.Foreground = new SolidColorBrush(Color.FromArgb(alphaText, rText, gText, bText));
                        node.OriginalBackground = new SolidColorBrush(Color.FromArgb(alpha, r, g, b));

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

            LoadEdges(doc);
            InvalidateVisual();
            UpdateTerminals();

            foreach (EdgeControl edge in edges)
                CheckSelfIntersection(edge);

        }

        private void OpenDetailWindow()
        {
            var snc = NodeControlBase.LastSelected;
            if (snc is not null)
            {
                snc.ShowDetailWindow();
            }
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

        public void OnNext(string value)
        {
            //Console.Write("Message to observers: " + value);
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
            ResetDeletionZone();
            //PositionOutput();
        }

        private void MenuItemNew_Click(object sender, RoutedEventArgs e)
        {
            foreach (var edge in edges.ToList())
            {
                edge.From?.UnregisterOutputEdge(edge.FromIndex ?? 0);
                if (edge.LabelBox != null)
                {
                    DiagramCanvas.Children.Remove(edge.StateButton);
                    DiagramCanvas.Children.Remove(edge.LabelBox);
                    edge.LabelBox = null;
                    edge.StateButton = null;
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
                    else if (f.EndsWith("field.json")) // Maybe not smart
                    {
                        LoadCargoTruckerField(f);
                        MessageBox.Show("Field Loaded", "Success", MessageBoxButton.OK);
                    }
                }
            }
        }

        private static void LoadCargoTruckerField(string f)
        {
            //File.Replace(f, "./field.json", "./field.json.old");
            var dest = Path.GetFullPath("./field.json");
            var backup = dest + ".old";

            if (File.Exists(dest))
                File.Copy(dest, backup, overwrite: true);

            File.Copy(f, dest, overwrite: true);   // keeps f intact
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ResetDeletionZone();
        }

        // Pan with WASD and arrow keys
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            double offsetX = 0.0;
            double offsetY = 0.0;

            if (FocusManager.GetFocusedElement(this) is not TextBox focussed)
            {
                if (e.Key == Key.W || e.Key == Key.Up) // pan up
                    offsetY = -10.0;
                else if (e.Key == Key.A || e.Key == Key.Left) // pan left
                    offsetX = -10.0;
                else if (e.Key == Key.S || e.Key == Key.Down) // pan down
                    offsetY = 10.0;
                else if (e.Key == Key.D || e.Key == Key.Right) // pan right
                    offsetX = 10.0;
                if (offsetX != 0.0 && offsetY != 0.0)
                    PanNodes(offsetX, offsetY);
            }
            if (e.Key == Key.F5)
                Run_Click(null, null);
            else if (e.Key == Key.F1)
            {
                ShowTipsScreen();
            }
            else if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control)
            {
                MenuItemSave_Click(null, null);
            }
            else if (e.Key == Key.L && Keyboard.Modifiers == ModifierKeys.Control)
            {
                MenuItemLoad_Click(null, null);
            }
        }

        private void PanNodes(double offsetX, double offsetY)
        {
            foreach (NodeControlBase n in canvasNodes)
            {
                var l = Canvas.GetLeft(n);
                var t = Canvas.GetTop(n);

                var newX = l + offsetX;
                var newY = t + offsetY;

                // This is necessary to update node positions on Pan; otherwise, the saved graph gets cluttered
                n.NodeData.Position = new Point(newX, newY);

                Canvas.SetLeft(n, newX);
                Canvas.SetTop(n, newY);
            }

            foreach (EdgeControl e in edges)
            {
                for (int i = 0; i < e.ControlPoints.Count; i++)
                {
                    var p = e.ControlPoints[i];
                    p.X += offsetX;
                    p.Y += offsetY;
                    e.ControlPoints[i] = p;
                }
            }

            UpdateEdges();
        }

        private void ShowTips_Click(object sender, RoutedEventArgs e)
        {
            ShowTipsScreen();
        }

        private void StopNetwork(object sender, RoutedEventArgs e)
        {
            Network.InterruptProcess();
            StopButton.Visibility = Visibility.Collapsed;
        }

        private void LoadCTField(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new()
            {
                DefaultExt = "json",
                Filter = "JSON Files|*.json|All files (*.*)|*.*"
            };
            if (ofd.ShowDialog() == true)
            {
                var f = ofd.FileName;
                LoadCargoTruckerField(f);
                MessageBox.Show("Field Loaded","Success",MessageBoxButton.OK);
            }
        }
    }
}