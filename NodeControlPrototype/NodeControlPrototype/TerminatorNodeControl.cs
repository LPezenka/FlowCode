using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace NodeControlPrototype
{
    public class TerminatorNodeControl : NodeControlBase
    {
        public static readonly DependencyProperty TerminalTextProperty =
            DependencyProperty.Register(
                nameof(TerminalText),
                typeof(string),
                typeof(TerminatorNodeControl),
                new FrameworkPropertyMetadata("Start", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
            );

        public string TerminalText
        {
            get => (string)GetValue(TerminalTextProperty);
            set => SetValue(TerminalTextProperty, value);
        }

        public TerminatorNodeControl()
        {
            DefaultStyleKey = typeof(TerminatorNodeControl);
        }

        static TerminatorNodeControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TerminatorNodeControl),
                new FrameworkPropertyMetadata(typeof(TerminatorNodeControl)));
        }

        public override List<Point> GetConnectionPoints() => new()
        {
            new Point(ActualWidth / 2, 0),               // oben (Eingang)
            new Point(ActualWidth / 2, ActualHeight)     // unten (Ausgang)
        };

        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            if (TerminalText.Trim().ToLower() == "start")
            {
                IsRoot = !IsRoot;
                this.Background = IsRoot ? Brushes.LightGreen : Brushes.Transparent;
            }
            base.OnMouseDoubleClick(e);
        }
    }
}
