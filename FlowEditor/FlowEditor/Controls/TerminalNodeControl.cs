using FlowEditor.Windows;
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

namespace FlowEditor.Controls
{
    public class TerminalNodeControl : NodeControlBase
    {
        public static readonly DependencyProperty TerminalTypeProperty =
            DependencyProperty.Register(nameof(TerminalType), typeof(string), typeof(TerminalNodeControl),
                new FrameworkPropertyMetadata("Start", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnTerminalTypeChanged));

        public static readonly DependencyProperty ReturnVariableProperty =
            DependencyProperty.Register(nameof(ReturnVariable), typeof(string), typeof(TerminalNodeControl),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty FunctionNameProperty =
            DependencyProperty.Register(nameof(FunctionName), typeof(string), typeof(TerminalNodeControl),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty InputVariablesProperty =
            DependencyProperty.Register(nameof(InputVariables), typeof(string), typeof(TerminalNodeControl),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));


        public string TerminalType
        {
            get => (string)GetValue(TerminalTypeProperty);
            set
            {
                //Console.WriteLine($"Setting Type to {value}");
                SetValue(TerminalTypeProperty, value);

                var t = Template;
                if (t is not null)
                {

                    var tbe = (TextBox)t.FindName("ReturnVariableBox", this);
                    var lble = (Label)t.FindName("lblReturnVariable", this);

                    var tbf = (TextBox)t.FindName("FunctionNameBox", this);
                    var lblf = (Label)t.FindName("lblFunctionName", this);

                    var tbiv = (TextBox)t.FindName("InputVariablesTextBox", this);
                    var lbliv = (Label)t.FindName("lblInputVariables", this);

                    var cmbtype = (ComboBox)t.FindName("cmbType", this);

                    if (value == "End")
                    {
                        //Height = 90;
                        lble.Visibility= tbe.Visibility = Visibility.Visible;
                        lblf.Visibility = tbf.Visibility = Visibility.Collapsed;
                        tbiv.Visibility = lbliv.Visibility = Visibility.Collapsed;
                        //Height = (lble.ActualHeight + cmbtype.ActualHeight) * 2;
                        Height = Width = double.NaN;
                    }
                    else
                    {
                        lble.Visibility = tbe.Visibility = Visibility.Collapsed;
                        lblf.Visibility = tbf.Visibility = Visibility.Visible;
                        tbiv.Visibility = lbliv.Visibility = Visibility.Visible;
                        //Height = lblf.ActualHeight + lbliv.ActualHeight + cmbtype.ActualHeight;
                        Height = Width = double.NaN;

                    }
                }
            }
        }

        public string InputVariables
        {
            get => (string)GetValue(InputVariablesProperty);
            set => SetValue(InputVariablesProperty, value);
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

        public TerminalNodeControl():base()
        {
            DefaultStyleKey = typeof(TerminalNodeControl);
            //ReturnVariable = "No Return Variable set";
            //Background = OriginalBackground;
        }

        static TerminalNodeControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TerminalNodeControl),
                new FrameworkPropertyMetadata(typeof(TerminalNodeControl)));
        }

        public override List<Point> GetConnectionPoints() => new()
        {
            new Point(ActualWidth / 2, -1),               // Eingang oben
            new Point(ActualWidth / 2, ActualHeight + 1)     // Ausgang unten
        };

        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            if (TerminalType?.ToLower() == "start")
            {
                IsRoot = !IsRoot;
                //Background = IsRoot ? Brushes.LightGreen : Brushes.Transparent;
            }
            base.OnMouseDoubleClick(e);
        }

        private static void OnTerminalTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TerminalNodeControl node)
            {
                node.InvalidateVisual();
                node.TerminalType = e.NewValue as string;
            }
        }

        public override void ShowDetailWindow()
        {
            TerminalDetailWindow sndWnd = new TerminalDetailWindow(this);
            sndWnd.ShowDialog();
        }
    }
}