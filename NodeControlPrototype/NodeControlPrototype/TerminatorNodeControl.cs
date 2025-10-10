using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

//namespace NodeControlPrototype
//{
//    public class TerminatorNodeControl : NodeControlBase
//    {
//        public static readonly DependencyProperty TerminalTextProperty =
//            DependencyProperty.Register(
//                nameof(TerminalText),
//                typeof(string),
//                typeof(TerminatorNodeControl),
//                new FrameworkPropertyMetadata("Start", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
//            );

//        public string TerminalText
//        {
//            get => (string)GetValue(TerminalTextProperty);
//            set => SetValue(TerminalTextProperty, value);
//        }

//        public TerminatorNodeControl()
//        {
//            DefaultStyleKey = typeof(TerminatorNodeControl);
//        }

//        static TerminatorNodeControl()
//        {
//            DefaultStyleKeyProperty.OverrideMetadata(typeof(TerminatorNodeControl),
//                new FrameworkPropertyMetadata(typeof(TerminatorNodeControl)));
//        }

//        public override List<Point> GetConnectionPoints() => new()
//        {
//            new Point(ActualWidth / 2, 0),               // oben (Eingang)
//            new Point(ActualWidth / 2, ActualHeight)     // unten (Ausgang)
//        };

//        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
//        {
//            if (TerminalText.Trim().ToLower() == "start")
//            {
//                IsRoot = !IsRoot;
//                this.Background = IsRoot ? Brushes.LightGreen : Brushes.Transparent;
//            }
//            base.OnMouseDoubleClick(e);
//        }
//    }
//}

namespace NodeControlPrototype
{
    public class TerminatorNodeControl : NodeControlBase
    {
        public static readonly DependencyProperty TerminalTypeProperty =
            DependencyProperty.Register(nameof(TerminalType), typeof(string), typeof(TerminatorNodeControl),
                new FrameworkPropertyMetadata("Start", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnTerminalTypeChanged));

        public static readonly DependencyProperty ReturnVariableProperty =
            DependencyProperty.Register(nameof(ReturnVariable), typeof(string), typeof(TerminatorNodeControl),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty FunctionNameProperty =
            DependencyProperty.Register(nameof(FunctionName), typeof(string), typeof(TerminatorNodeControl),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));


        public string TerminalType
        {
            get => (string)GetValue(TerminalTypeProperty);
            set
            {
                Console.WriteLine($"Setting Type to {value}");
                SetValue(TerminalTypeProperty, value);

                var t = this.Template;
                if (t is not null)
                {

                    var tbe = (TextBox)t.FindName("ReturnVariableBox", this);
                    var lble = (Label)t.FindName("lblReturnVariable", this);

                    var tbf = (TextBox)t.FindName("FunctionNameBox", this);
                    var lblf = (Label)t.FindName("lblFunctionName", this);

                    if (value == "End")
                    {

                        lble.Visibility= tbe.Visibility = Visibility.Visible;
                        lblf.Visibility = tbf.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        lble.Visibility = tbe.Visibility = Visibility.Collapsed;
                        lblf.Visibility = tbf.Visibility = Visibility.Visible;

                    }
                }
            }
        }

        public string FunctionName
        {
            get => (string)GetValue(FunctionNameProperty);
            set => SetValue(FunctionNameProperty, value);
        }

        public string ReturnVariable
        {
            get => (string)GetValue(ReturnVariableProperty);
            set => SetValue(ReturnVariableProperty, value);
        }

        public TerminatorNodeControl()
        {
            DefaultStyleKey = typeof(TerminatorNodeControl);
            //ReturnVariable = "No Return Variable set";
        }

        static TerminatorNodeControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TerminatorNodeControl),
                new FrameworkPropertyMetadata(typeof(TerminatorNodeControl)));
        }

        public override List<Point> GetConnectionPoints() => new()
        {
            new Point(ActualWidth / 2, 0),               // Eingang oben
            new Point(ActualWidth / 2, ActualHeight)     // Ausgang unten
        };

        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            if (TerminalType?.ToLower() == "start")
            {
                IsRoot = !IsRoot;
                this.Background = IsRoot ? Brushes.LightGreen : Brushes.Transparent;
            }
            base.OnMouseDoubleClick(e);
        }

        private static void OnTerminalTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TerminatorNodeControl node)
            {
                node.InvalidateVisual();
                node.TerminalType = e.NewValue as string;
            }
        }
    }
}