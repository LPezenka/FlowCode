using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;

namespace NodeControlPrototype.Controls
{
    public class ControlPointVisual : UserControl
    {
        private Ellipse _ellipse;
        private bool _isDragging = false;
        private Point _dragStart;

        public event EventHandler<Point> PositionChanged;

        public ControlPointVisual()
        {
            Width = 10;
            Height = 10;

            _ellipse = new Ellipse
            {
                Fill = Brushes.Blue,
                Stroke = Brushes.Black,
                StrokeThickness = 1,
                Width = 10,
                Height = 10
            };

            Content = _ellipse;

            MouseLeftButtonDown += OnMouseLeftButtonDown;
            MouseLeftButtonUp += OnMouseLeftButtonUp;
            MouseMove += OnMouseMove;
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isDragging = true;
            _dragStart = e.GetPosition(Parent as UIElement);
            CaptureMouse();
            e.Handled = true;
        }

        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDragging)
            {
                _isDragging = false;
                ReleaseMouseCapture();
                e.Handled = true;
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging && Parent is Canvas canvas)
            {
                var pos = e.GetPosition(canvas);
                var offset = pos - _dragStart;
                _dragStart = pos;

                double newX = Canvas.GetLeft(this) + offset.X;
                double newY = Canvas.GetTop(this) + offset.Y;

                Canvas.SetLeft(this, newX);
                Canvas.SetTop(this, newY);

                PositionChanged?.Invoke(this, new Point(newX + Width / 2, newY + Height / 2));
                e.Handled = true;
            }
        }

        public void SetPosition(Point center)
        {
            Canvas.SetLeft(this, center.X - Width / 2);
            Canvas.SetTop(this, center.Y - Height / 2);
        }

        public Point GetCenterPosition()
        {
            return new Point(Canvas.GetLeft(this) + Width / 2, Canvas.GetTop(this) + Height / 2);
        }
    }

}
