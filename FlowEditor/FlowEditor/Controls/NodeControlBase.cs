using Interfaces;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace FlowEditor.Controls
{
    public abstract class NodeControlBase : Control, IHighlightable, INotifyPropertyChanged
    {

        public static NodeControlBase LastSelected { get; set; } = null;

        //public static Brush TemplateBrush { get; set; }
        public Brush OriginalBackground { get; set; } = Brushes.Gray;
        public Node NodeData { get; set; }

        public string Code
        {
            get => NodeData.Title;
            set
            {
                NodeData.Title = value;
                NotifyPropertyChanged(nameof(Code));
            }
        }

        protected int FirstOutputIndex { get; set; } = 1; // Standard: ein Eingang bei Index 0
        public abstract List<Point> GetConnectionPoints();

        public event EventHandler<NodeMovedEventArgs> NodeMoved;
        public event EventHandler<ConnectionPointClickedEventArgs> ConnectionPointClicked;
        public event EventHandler RootRequested;
        public event EventHandler HighlightRequested;
        public event EventHandler ToggleDeletionZone;
        public event PropertyChangedEventHandler? PropertyChanged;
        //public static Window DetailWindow { get; set; }

        public bool IsRoot { get; set; }

        public virtual void ShowDetailWindow()
        {

        }

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


        protected NodeControlBase()
        {
            //Background = TemplateBrush;
        }

        public int? GetNextFreeOutputIndex(out EdgeControl? existingEdge)
        {
            for (int i = FirstOutputIndex; i < GetConnectionPoints().Count; i++)
            {
                if (!occupiedOutputEdges.TryGetValue(i, out EdgeControl? value))
                {
                    existingEdge = null;
                    return i;
                }
                else
                {
                    existingEdge = value;
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
            return 0; // just one input index per node
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
                    drawingContext.DrawEllipse(null, new Pen(Brushes.Blue, 2), point, 8, 8);
                }

                drawingContext.DrawEllipse(brush, null, point, 6, 6);
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            if (LastSelected is not null)
                LastSelected.SetActive(false);
            
            SetActive(true);
            LastSelected = this;
            ToggleDeletionZone?.Invoke(this, EventArgs.Empty);

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
            e.Handled = true;
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            _isDragging = false;
            ReleaseMouseCapture();
        }

        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            RootRequested?.Invoke(this, EventArgs.Empty);
            e.Handled = true;
            
            base.OnMouseDoubleClick(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (_isDragging && e.LeftButton == MouseButtonState.Pressed)
            {
                if (Parent is Canvas parent)
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

        public virtual void SetActive(bool active)
        {

            Application.Current.Dispatcher.Invoke(() =>
            {
                if (active)
                {
                    BorderBrush = Brushes.LightGreen;
                    BorderThickness = new Thickness(9.0);
                    NotifyPropertyChanged("Background");
                    InvalidateVisual();
                }
                else
                {
                    Background = OriginalBackground;// Brushes.White;
                    BorderBrush = Brushes.Black;
                    BorderThickness = new Thickness(1.0);
                    NotifyPropertyChanged("Background");
                    InvalidateVisual();
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
