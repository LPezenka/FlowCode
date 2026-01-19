using FlowEditor.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
    public partial class TerminalDetailWindow : Window, INotifyPropertyChanged
    {
        public List<string> InParamsList { get; set; }
        public TerminalNodeControl Node { get; set; }

        public string Signature 
        { 
            get
            {
                return $"{FunctionName.Text}({string.Join(",",InParamsList)})";
            } 
        }
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

            if (!string.IsNullOrWhiteSpace(Node.FunctionName))
                FunctionName.Text = Node.FunctionName;

            InParams.ItemsSource = InParamsList;
            InParams.Items.Refresh();
            this.DataContext = this;
            this.ResizeMode = ResizeMode.NoResize;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
    => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private void AddNewInParam(object sender, RoutedEventArgs e)
        {
            string paramName = NewInParam.Text;
            NewInParam.Text = string.Empty;
            InParamsList.Add(paramName);
            FocusManager.SetFocusedElement(this, NewInParam);
            InParams.Items.Refresh();
            OnPropertyChanged(nameof(Signature));
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

        private void FunctionName_TextChanged(object sender, TextChangedEventArgs e)
        {
            OnPropertyChanged(nameof(Signature));
        }
    }
}
