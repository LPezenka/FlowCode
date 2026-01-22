using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System.Windows.Input;
using FlowCodeInfrastructure;

namespace FlowEditor.Controls
{
    public enum ConnectionDirection
    {
        Top,
        Bottom,
        Left,
        Right
    }

    public class EdgeControl : Control
    {
        public Button StateButton { get; set; }
        public TextBox LabelBox { get; set; }
        public NodeControlBase From { get; set; }
        public NodeControlBase To { get; set; }
        public int? FromIndex { get; set; } = 0;
        public int? ToIndex { get; set; } = 0;
        public string Label { get; set; }
        public Point? CurrentMousePosition { get; set; }

        public List<Point> ControlPoints { get; set; } = new();
        private List<ControlPointVisual> _controlPointVisuals = new();
        private ControlPointVisual _draggingPoint = null;
        private Point _lastMousePos;

        public Point LabelPosition
        {
            get { return (Point)GetValue(LabelPositionProperty); }
            set { SetValue(LabelPositionProperty, value); }
        }

        public static readonly DependencyProperty LabelPositionProperty =
    DependencyProperty.Register(
        nameof(LabelPosition),
        typeof(Point),
        typeof(EdgeControl),
        new FrameworkPropertyMetadata(
            new Point(0, 0),
            FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
            OnLabelPositionChanged));

        private static void OnLabelPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is EdgeControl edge)
            {
                edge.InvalidateArrange();
            }
        }

        static EdgeControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(EdgeControl),
                new FrameworkPropertyMetadata(typeof(EdgeControl)));
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if (From == null || FromIndex == null)
                return;

            Point start = From.TranslatePoint(From.GetConnectionPoints()[(int)FromIndex], this);// Application.Current.MainWindow);
            Point end;

            SolidColorBrush edgeColor = Brushes.Black;
            if (LabelBox != null && LabelBox.Text == Config.GetKeyword(Config.KeyWord.True))
            {
                edgeColor = Brushes.LightGreen;
                StateButton.Foreground = Brushes.LightGreen;
            }
            else if (LabelBox != null && LabelBox.Text == Config.GetKeyword(Config.KeyWord.False))
            {
                edgeColor = Brushes.Red;
                StateButton.Foreground = Brushes.Red;
                edgeColor = Brushes.Red;
            }

            if (To != null && ToIndex != null)
            {
                end = To.TranslatePoint(To.GetConnectionPoints()[(int)ToIndex], this); // Application.Current.MainWindow);
            }
            else if (CurrentMousePosition.HasValue)
            {
                end = CurrentMousePosition.Value;
            }
            else return;

            var pen = new Pen(edgeColor, 2);
            if (From.GetType() == typeof(ProcessNodeControl) && FromIndex == 2)
            {
                pen.DashStyle = DashStyles.DashDotDot;
            }

            // Gather all points to form the polyline
            List<Point> points = new List<Point> { start };

            if (ControlPoints == null || ControlPoints.Count == 0)
            {
                ConnectionDirection? startDir = GetConnectionDirection(From, FromIndex ?? 0);
                ConnectionDirection? endDir = GetConnectionDirection(To, ToIndex ?? 0);

                var routed = RouteOrthogonally(start, end, startDir, endDir);
                points.AddRange(routed);
            }
            else
            {
                points.AddRange(ControlPoints);
            }

            points.Add(end);
            DrawLineSegments(drawingContext, pen, points);

            // Compute label position at the middle of the polyline

            // Toggle StateButton visibility based on wheter or not the edge connects a decision node
            ToggleButtonVisibility();

            Point labelPos;
            int middleIndex = (points.Count - 1) / 2;
            Point lp1 = points[middleIndex];
            Point lp2 = points[middleIndex + 1];
            labelPos = new Point((lp1.X + lp2.X) / 2 - 40, (lp1.Y + lp2.Y) / 2 - 10);

            LabelPosition = labelPos;
            InvalidateArrange();

            // Position state button in the middle of the edge
            PositionStateButton();

            // Compute arrowhead based on last segment of the polyline
            Point arrowStart = points[points.Count - 2];
            Point arrowEnd = points[points.Count - 1];
            DrawArrowHead(drawingContext, arrowStart, arrowEnd);
            //if (LabelBox is not null && LabelBox.Visibility == Visibility.Visible)
            //LabelBox.Foreground = edgeColor;
        }

        private void PositionStateButton()
        {
            if (LabelBox != null)
            {
                Canvas.SetLeft(LabelBox, LabelPosition.X);
                Canvas.SetTop(LabelBox, LabelPosition.Y);
            }
            if (StateButton is not null)
            {
                Canvas.SetLeft(StateButton, LabelPosition.X);
                Canvas.SetTop(StateButton, LabelPosition.Y);
            }
        }

        private void ToggleButtonVisibility()
        {
            if (From.GetType() == typeof(SequenceNodeControl) || From.GetType() == typeof(TerminalNodeControl))
            {
                if (StateButton is not null)
                    StateButton.Visibility = Visibility.Collapsed;

                if (LabelBox is not null)
                    LabelBox.Visibility = Visibility.Collapsed;
            }
            else if (From.GetType() == typeof(ProcessNodeControl) && FromIndex != 2)
            {
                if (StateButton is not null)
                    StateButton.Visibility = Visibility.Collapsed;

                if (LabelBox is not null)
                    LabelBox.Visibility = Visibility.Collapsed;
            }
        }

        private static void DrawLineSegments(DrawingContext drawingContext, Pen pen, List<Point> points)
        {
            for (int i = 0; i < points.Count - 1; i++)
            {
                drawingContext.DrawLine(pen, points[i], points[i + 1]);
            }
        }

        private void DrawArrowHead(DrawingContext dc, Point start, Point end)
        {
            Vector direction = end - start;
            direction.Normalize();
            Vector normal = new Vector(-direction.Y, direction.X);
            Point arrowBase = end - direction * 10;
            Point p1 = arrowBase + normal * 5;
            Point p2 = arrowBase - normal * 5;

            var figure = new PathFigure { StartPoint = end };
            figure.Segments.Add(new LineSegment(p1, true));
            figure.Segments.Add(new LineSegment(p2, true));
            figure.Segments.Add(new LineSegment(end, true));

            var geometry = new PathGeometry();
            geometry.Figures.Add(figure);
            dc.DrawGeometry(Brushes.Black, null, geometry);
        }

        public event EventHandler DeleteRequested;

        protected void RaiseDeleteRequested() => DeleteRequested?.Invoke(this, EventArgs.Empty);

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            //Probably not needed anymore, as edges are automatically re-routed
            //
            //if (e.ClickCount == 2)
            //{
            //    Point clickPos = e.GetPosition(Application.Current.MainWindow);
            //    InsertControlPoint(clickPos);
            //    e.Handled = true;
            //}
        }

        public void InsertControlPoint(Point position)
        {
            if (ControlPoints == null)
                ControlPoints = new List<Point>();

            // Füge den Punkt vor dem letzten Segment ein (wenn es existiert)
            // Oder einfach in die Mitte
            int insertIndex = ControlPoints.Count / 2;
            ControlPoints.Insert(insertIndex, position);

            // Visuelles Element erzeugen
            var visual = new ControlPointVisual();
            visual.SetPosition(position);
            visual.PositionChanged += (s, newPos) =>
            {
                ControlPoints[insertIndex] = newPos;
                InvalidateVisual();
            };

            // Zur Canvas hinzufügen
            if (Application.Current.MainWindow is MainWindow main && main.DiagramCanvas != null)
            {
                main.DiagramCanvas.Children.Add(visual);
            }

            InvalidateVisual();
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.ChangedButton == MouseButton.Right)
            {
                if (From != null && FromIndex.HasValue)
                {
                    From.UnregisterOutputEdge(FromIndex.Value);
                }
                RaiseDeleteRequested();
                e.Handled = true;
            }
        }

        public List<Point> RouteOrthogonally(Point start, Point end, ConnectionDirection? fromDirection, ConnectionDirection? toDirection)
        {
            var points = new List<Point>();


            Point current = start;


            // Schritt 1: Offset vom Start weg in die gewünschte Richtung
            current = OffsetPoint(current, fromDirection, 20);
            points.Add(current);


            // Schritt 2: Auf Zielrichtung vorbereiten
            Point preTarget = OffsetPoint(end, toDirection, 20);


            // Schritt 3: Zwischenpunkt einfügen
            if (fromDirection == ConnectionDirection.Left || fromDirection == ConnectionDirection.Right)
            {
                points.Add(new Point(preTarget.X, current.Y));
            }
            else
            {
                points.Add(new Point(current.X, preTarget.Y));
            }


            points.Add(preTarget);
            return points;
        }


        private Point OffsetPoint(Point p, ConnectionDirection? direction, double offset)
        {
            return direction switch
            {
                ConnectionDirection.Top => new Point(p.X, p.Y - offset),
                ConnectionDirection.Bottom => new Point(p.X, p.Y + offset),
                ConnectionDirection.Left => new Point(p.X - offset, p.Y),
                ConnectionDirection.Right => new Point(p.X + offset, p.Y),
                _ => p
            };
        }


        private ConnectionDirection? GetConnectionDirection(NodeControlBase node, int ?index)
        {

            if (node == null || !index.HasValue)
                return null;


            var points = node.GetConnectionPoints();


            if (index.Value < 0 || index.Value >= points.Count)
                return null;


            Point point = points[index.Value];


            double left = point.X;
            double right = node.ActualWidth - point.X;
            double top = point.Y;
            double bottom = node.ActualHeight - point.Y;


            double min = new[] { left, right, top, bottom }.Min();


            if (min == top) return ConnectionDirection.Top;
            if (min == bottom) return ConnectionDirection.Bottom;
            if (min == left) return ConnectionDirection.Left;
            if (min == right) return ConnectionDirection.Right;


            return null;
        }

        internal void StateButtonClicked(object sender, RoutedEventArgs e)
        {
            StateButton.Content = StateButton.Content ==
            FlowCodeInfrastructure.Config.GetKeyword(Config.KeyWord.True) ? FlowCodeInfrastructure.Config.GetKeyword(Config.KeyWord.False) : FlowCodeInfrastructure.Config.GetKeyword(Config.KeyWord.True);
            if (LabelBox is not null)
                LabelBox.Text = StateButton.Content.ToString();
            this.InvalidateVisual();
        }
    }
}
