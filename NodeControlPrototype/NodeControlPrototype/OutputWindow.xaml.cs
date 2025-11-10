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

namespace NodeControlPrototype
{
    /// <summary>
    /// Interaktionslogik für OutputWindow.xaml
    /// </summary>
    public partial class OutputWindow : Window, IDisposable
    {
        public OutputWindow(string prompt)
        {
            InitializeComponent();
            LabelOutput.Content = prompt;
        }

        public void Dispose()
        {
            
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
