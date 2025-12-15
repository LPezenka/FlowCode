using FlowEditor.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FlowEditor.Windows
{
    /// <summary>
    /// Interaktionslogik für TerminalDetailWindow.xaml
    /// </summary>
    public partial class TerminalDetailWindow : Window
    {
        public List<string> InParamsList { get; set; }
        public TerminalNodeControl Node { get; set; }
        public TerminalDetailWindow(NodeControlBase node)
        {
            Node = node as TerminalNodeControl;
            if (Node is null)
                return;

            InParamsList = new List<string>();
            if (!string.IsNullOrWhiteSpace(Node.InputVariables))
            {
                string[] inParams = Node.InputVariables.Split(",");
                foreach (string p in inParams)
                    InParamsList.Add(p);
            }
            
            InitializeComponent();
            InParams.ItemsSource = InParamsList;
            InParams.Items.Refresh();

            this.ResizeMode = ResizeMode.NoResize;
        }

        private void AddNewInParam(object sender, RoutedEventArgs e)
        {
            string paramName = NewInParam.Text;
            NewInParam.Text = string.Empty;
            InParamsList.Add(paramName);
            InParams.Items.Refresh();
        }

        private void Ok(object sender, RoutedEventArgs e)
        {
            Node.FunctionName = FunctionName.Text;
            Node.InputVariables = string.Join(",", InParamsList);
            DialogResult = true;
        }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
