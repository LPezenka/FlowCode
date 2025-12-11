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
    /// Interaktionslogik für ProcessNodeDetailWindow.xaml
    /// </summary>
    public partial class ProcessNodeDetailWindow : Window
    {
        public static Dictionary<string, string> Signatures { get; set; }
        public ProcessNodeControl Node { get; set; }
        private List<string> registeredVariables;
        public static List<string> FunctionNames { get; set; }

        public ProcessNodeDetailWindow(NodeControlBase node)
        {
            Node = node as ProcessNodeControl;
            registeredVariables = new List<string>();
            InitializeComponent();
            RegisteredVariables.ItemsSource = registeredVariables;
            Functions.ItemsSource = Signatures.Keys;
            Functions.Items.Refresh();
        }

        private void Button_AddVariable_Click(object sender, RoutedEventArgs e)
        {
            registeredVariables.Add(NewVariable.Text);
            RegisteredVariables.Items.Refresh();
            UpdateCall();
        }

        private void UpdateCall()
        {
            if (Call is not null)
                Call.Text = $"{TargetName.Text}({string.Join(",", registeredVariables)})";
        }

        private void Button_Ok_Click(object sender, RoutedEventArgs e)
        {
            Node.Code = Call.Text;
            DialogResult = true;
        }

        private void TargetName_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateCall();
        }

        private void Button_RemoveVar_Click(object sender, RoutedEventArgs e)
        {
            var selected = RegisteredVariables.SelectedItem as string;
            registeredVariables.Remove(selected);
            RegisteredVariables.Items.Refresh();
            UpdateCall();
        }

        private void Functions_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            TargetName.Text = Functions.SelectedItem as string;
        }

        private void Functions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string function = Functions.SelectedItem as string;
            string signature = Signatures[function];
            Signature.Content = signature;
        }
    }
}
