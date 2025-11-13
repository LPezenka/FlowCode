using Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Linq;

namespace NodeControlPrototype.Controls
{
    // FlowchartControlLibrary/NodeControlBase.cs



    public abstract class NodeControlBase : Control, IHighlightable, INotifyPropertyChanged
    {
        //    public Node NodeData { get; set; }
        //    public abstract List<Point> GetConnectionPoints();

        //    public event EventHandler<NodeMovedEventArgs> NodeMoved;
        //    public event EventHandler<ConnectionPointClickedEventArgs> ConnectionPointClicked;

        //    // Tracking used output points by index
        //    private readonly HashSet<int> usedOutputIndices = new();
        //    protected int firstOutput = 1;

        //    protected void RaiseNodeMoved() => NodeMoved?.Invoke(this, new NodeMovedEventArgs(this));

        //    protected void OnConnectionPointClicked(int index) => ConnectionPointClicked?.Invoke(this, new ConnectionPointClickedEventArgs(this, index));

        //    private bool _isDragging;
        //    private Point _dragStart;
        //    private int? _hoveredConnectionPointIndex = null;

        //    public int? GetNextFreeOutputIndex()
        //    {
        //        for (int i = firstOutput; i < GetConnectionPoints().Count; i++)
        //        {
        //            if (!usedOutputIndices.Contains(i))
        //            {
        //                usedOutputIndices.Add(i);
        //                return i;
        //            }
        //        }
        //        return null;
        //    }

        //    protected override void OnRender(DrawingContext drawingContext)
        //    {
        //        base.OnRender(drawingContext);

        //        var connectionPoints = GetConnectionPoints();
        //        for (int i = 0; i < connectionPoints.Count; i++)
        //        {
        //            Point point = connectionPoints[i];
        //            Brush brush = usedOutputIndices.Contains(i) ? Brushes.Red : Brushes.Green;

        //            if (_hoveredConnectionPointIndex == i)
        //            {
        //                drawingContext.DrawEllipse(null, new Pen(Brushes.Blue, 2), point, 7, 7);
        //            }

        //            drawingContext.DrawEllipse(brush, null, point, 5, 5);
        //        }
        //    }

        //    public int? GetNextFreeInputIndex()
        //    {
        //        for (int i = 0; i < GetConnectionPoints().Count; i++)
        //        {
        //            if (!usedOutputIndices.Contains(i))
        //            {
        //                usedOutputIndices.Add(i);
        //                return i;
        //            }
        //        }
        //        return null;
        //    }

        //    public void ReleaseOutputIndex(int index)
        //    {
        //        usedOutputIndices.Remove(index);
        //    }

        //    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        //    {
        //        base.OnMouseLeftButtonDown(e);

        //        var pos = e.GetPosition(this);
        //        var points = GetConnectionPoints();
        //        for (int i = 0; i < points.Count; i++)
        //        {
        //            if ((points[i] - pos).Length < 8)
        //            {
        //                OnConnectionPointClicked(i);
        //                return;
        //            }
        //        }

        //        _isDragging = true;
        //        _dragStart = e.GetPosition(Parent as UIElement);
        //        CaptureMouse();
        //    }

        //    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        //    {
        //        base.OnMouseLeftButtonUp(e);
        //        _isDragging = false;
        //        ReleaseMouseCapture();
        //    }

        //    protected override void OnMouseMove(MouseEventArgs e)
        //    {
        //        base.OnMouseMove(e);
        //        if (_isDragging && e.LeftButton == MouseButtonState.Pressed)
        //        {
        //            var parent = Parent as Canvas;
        //            if (parent != null)
        //            {
        //                Point currentPos = e.GetPosition(parent);
        //                double offsetX = currentPos.X - _dragStart.X;
        //                double offsetY = currentPos.Y - _dragStart.Y;

        //                double newLeft = Canvas.GetLeft(this) + offsetX;
        //                double newTop = Canvas.GetTop(this) + offsetY;

        //                Canvas.SetLeft(this, newLeft);
        //                Canvas.SetTop(this, newTop);

        //                NodeData.Position = new Point(newLeft, newTop);
        //                _dragStart = currentPos;

        //                RaiseNodeMoved();
        //            }
        //        }
        //        else
        //        {
        //            var pos = e.GetPosition(this);
        //            var points = GetConnectionPoints();
        //            _hoveredConnectionPointIndex = null;

        //            for (int i = 0; i < points.Count; i++)
        //            {
        //                if ((points[i] - pos).Length < 8)
        //                {
        //                    _hoveredConnectionPointIndex = i;
        //                    break;
        //                }
        //            }
        //            InvalidateVisual();
        //        }
        //    }
        //}

        //public class NodeMovedEventArgs : EventArgs
        //{
        //    public NodeControlBase NodeControl { get; }
        //    public NodeMovedEventArgs(NodeControlBase nodeControl) => NodeControl = nodeControl;
        //}

        //public class ConnectionPointClickedEventArgs : EventArgs
        //{
        //    public NodeControlBase Node { get; }
        //    public int ConnectionPointIndex { get; }

        //    public ConnectionPointClickedEventArgs(NodeControlBase node, int connectionPointIndex)
        //    {
        //        Node = node;
        //        ConnectionPointIndex = connectionPointIndex;
        //    }
        //}

        public Brush OriginalBackground { get; set; } = Brushes.Gray;
        public Node NodeData { get; set; }
        protected int FirstOutputIndex { get; set; } = 1; // Standard: ein Eingang bei Index 0
        public abstract List<Point> GetConnectionPoints();

        public event EventHandler<NodeMovedEventArgs> NodeMoved;
        public event EventHandler<ConnectionPointClickedEventArgs> ConnectionPointClicked;
        public event EventHandler RootRequested;
        public event EventHandler HighlightRequested;
        public event PropertyChangedEventHandler? PropertyChanged;

        public bool IsRoot { get; set; }

        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "Background")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void RaiseNodeMoved() => NodeMoved?.Invoke(this, new NodeMovedEventArgs(this));

        protected void OnConnectionPointClicked(int index)
            => ConnectionPointClicked?.Invoke(this, new ConnectionPointClickedEventArgs(this, index));

        private bool _isDragging;
        private Point _dragStart;
        private int? _hoveredConnectionPointIndex = null;

        private readonly Dictionary<int, EdgeControl> occupiedOutputEdges = new();

        public int? GetNextFreeOutputIndex(out EdgeControl? existingEdge)
        {
            for (int i = FirstOutputIndex; i < GetConnectionPoints().Count; i++)
            {
                if (!occupiedOutputEdges.ContainsKey(i))
                {
                    existingEdge = null;
                    return i;
                }
                else
                {
                    existingEdge = occupiedOutputEdges[i];
                    return i;
                }
            }
            existingEdge = null;
            return null;
        }

        public void RegisterOutputEdge(int index, EdgeControl edge)
        {
            occupiedOutputEdges[index] = edge;
            InvalidateVisual();
        }

        public void UnregisterOutputEdge(int index)
        {
            if (occupiedOutputEdges.ContainsKey(index))
                occupiedOutputEdges.Remove(index);
            InvalidateVisual();
        }

        public int? GetNextFreeInputIndex()
        {
            for (int i = 0; i < FirstOutputIndex; i++)
            {
                return i; // allow multiple edges on inputs
            }
            return null;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            var connectionPoints = GetConnectionPoints();
            for (int i = 0; i < connectionPoints.Count; i++)
            {
                Point point = connectionPoints[i];
                Brush brush = i >= FirstOutputIndex && occupiedOutputEdges.ContainsKey(i) ? Brushes.Red : Brushes.Green;

                if (_hoveredConnectionPointIndex == i)
                {
                    drawingContext.DrawEllipse(null, new Pen(Brushes.Blue, 2), point, 7, 7);
                }

                drawingContext.DrawEllipse(brush, null, point, 5, 5);
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            var pos = e.GetPosition(this);
            var points = GetConnectionPoints();
            for (int i = 0; i < points.Count; i++)
            {
                if ((points[i] - pos).Length < 8)
                {
                    OnConnectionPointClicked(i);
                    return;
                }
            }

            _isDragging = true;
            _dragStart = e.GetPosition(Parent as UIElement);
            CaptureMouse();
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            _isDragging = false;
            ReleaseMouseCapture();
        }

        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            //IsRoot = !IsRoot;
            //if (IsRoot)
            //{
            //    this.Background = Brushes.LightGray;
            //}
            //else
            //{
            //    this.Background = Brushes.Transparent;
            //}
            //    base.OnMouseDoubleClick(e);
            RootRequested?.Invoke(this, EventArgs.Empty);
            e.Handled = true;
            
            base.OnMouseDoubleClick(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (_isDragging && e.LeftButton == MouseButtonState.Pressed)
            {
                var parent = Parent as Canvas;
                if (parent != null)
                {
                    Point currentPos = e.GetPosition(parent);
                    double offsetX = currentPos.X - _dragStart.X;
                    double offsetY = currentPos.Y - _dragStart.Y;

                    double newLeft = Canvas.GetLeft(this) + offsetX;
                    double newTop = Canvas.GetTop(this) + offsetY;

                    Canvas.SetLeft(this, newLeft);
                    Canvas.SetTop(this, newTop);

                    NodeData.Position = new Point(newLeft, newTop);
                    _dragStart = currentPos;

                    RaiseNodeMoved();
                }
            }
            else
            {
                var pos = e.GetPosition(this);
                var points = GetConnectionPoints();
                _hoveredConnectionPointIndex = null;

                for (int i = 0; i < points.Count; i++)
                {
                    if ((points[i] - pos).Length < 8)
                    {
                        _hoveredConnectionPointIndex = i;
                        break;
                    }
                }
                InvalidateVisual();
            }
        }

        public void SetActive(bool active)
        {

            Application.Current.Dispatcher.Invoke(() =>
            {

                if (active)
                {
                    //HighlightRequested?.Invoke(this, EventArgs.Empty);
                    Background = Brushes.Magenta;
                    NotifyPropertyChanged("Background");
                    InvalidateVisual();
                }
                else
                {
                    if (!IsRoot)
                    {
                        Background = OriginalBackground;// Brushes.White;
                        //Background = Brushes.Gold;
                    }
                    else
                    {
                        Background = Brushes.Gold;
                        //Background = OriginalBackground;
                    }
                    //Task.Run(async () =>
                    //{
                    //await Task.Delay(100);
                    //Dispatcher.Invoke(() =>
                    NotifyPropertyChanged("Background");
                    //);
                        InvalidateVisual();
                    //});
                }
            });
            
        }
    }

    public class NodeMovedEventArgs : EventArgs
    {
        public NodeControlBase NodeControl { get; }
        public NodeMovedEventArgs(NodeControlBase nodeControl) => NodeControl = nodeControl;
    }

    public class ConnectionPointClickedEventArgs : EventArgs
    {
        public NodeControlBase Node { get; }
        public int ConnectionPointIndex { get; }

        public ConnectionPointClickedEventArgs(NodeControlBase node, int connectionPointIndex)
        {
            Node = node;
            ConnectionPointIndex = connectionPointIndex;
        }
    }
}
