using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System.Windows.Input;

namespace NodeControlPrototype
{
    public class EdgeControl : Control
    {
        public TextBox LabelBox { get; set; }
        public NodeControlBase From { get; set; }
        public NodeControlBase To { get; set; }
        public int? FromIndex { get; set; } = 0;
        public int? ToIndex { get; set; } = 0;
        public string Label { get; set; }
        public Point? CurrentMousePosition { get; set; }

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
            //base.OnRender(drawingContext);

            //if (From == null) return;

            //Point start = From.TranslatePoint(From.GetConnectionPoints()[(int)(FromIndex ?? 0)], this);
            //Point end;

            //if (To != null && ToIndex.HasValue)
            //{
            //    end = To.TranslatePoint(To.GetConnectionPoints()[(int)ToIndex], this);
            //}
            //else if (CurrentMousePosition.HasValue)
            //{
            //    end = CurrentMousePosition.Value;
            //}
            //else
            //{
            //    return;
            //}

            //var pen = new Pen(Brushes.Black, 2);
            //drawingContext.DrawLine(pen, start, end);
            //DrawArrowHead(drawingContext, start, end);

            //if (!string.IsNullOrWhiteSpace(Label))
            //{
            //    var mid = new Point((start.X + end.X) / 2, (start.Y + end.Y) / 2);
            //    var formattedText = new FormattedText(
            //        Label,
            //        System.Globalization.CultureInfo.CurrentCulture,
            //        FlowDirection.LeftToRight,
            //        new Typeface("Segoe UI"),
            //        12,
            //        Brushes.Black,
            //        VisualTreeHelper.GetDpi(this).PixelsPerDip);

            //    drawingContext.DrawText(formattedText, mid);
            //    LabelPosition = new Point((start.X + end.X) / 2 - 40, (start.Y + end.Y) / 2 - 10);
            //    InvalidateArrange();
            //    UpdateLayout();
            //}

            base.OnRender(drawingContext);

            if (From == null || FromIndex == null)
                return;

            Point start = From.TranslatePoint(From.GetConnectionPoints()[(int)FromIndex], Application.Current.MainWindow);
            Point end;

            if (To != null && ToIndex != null)
            {
                end = To.TranslatePoint(To.GetConnectionPoints()[(int)ToIndex], Application.Current.MainWindow);
            }
            else if (CurrentMousePosition.HasValue)
            {
                end = CurrentMousePosition.Value;
            }
            else return;

            var pen = new Pen(Brushes.Black, 2);
            drawingContext.DrawLine(pen, start, end);

            LabelPosition = new Point((start.X + end.X) / 2 - 40, (start.Y + end.Y) / 2 - 10);
            InvalidateArrange();

            if (LabelBox != null)
            {
                Canvas.SetLeft(LabelBox, LabelPosition.X);
                Canvas.SetTop(LabelBox, LabelPosition.Y);
            }

            DrawArrowHead(drawingContext, start, end);

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
    }
}
