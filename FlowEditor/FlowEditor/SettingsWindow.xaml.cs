using FlowCodeInfrastructure;
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

namespace FlowEditor
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
        }

        public override void EndInit()
        {
            this.Height = this.Width = double.NaN; // Auto size
            Delay.Value = Node.Delay;
            base.EndInit();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            var yesKeyword = TextBoxYesKeyword.Text.Trim();
            var noKeyword = TextBoxNoKeyword.Text.Trim();
            if (string.IsNullOrEmpty(yesKeyword) || string.IsNullOrEmpty(noKeyword))
            {
                MessageBox.Show("Please enter both keywords.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            FlowCodeInfrastructure.Config.SetKeyWord(FlowCodeInfrastructure.Config.KeyWord.True, yesKeyword);
            FlowCodeInfrastructure.Config.SetKeyWord(FlowCodeInfrastructure.Config.KeyWord.False, noKeyword);
            FlowCodeInfrastructure.Config.SetKeyWord(FlowCodeInfrastructure.Config.KeyWord.Input, TextBoxInputKeyword.Text.Trim());
            FlowCodeInfrastructure.Config.SetKeyWord(FlowCodeInfrastructure.Config.KeyWord.Output, TextBoxOutputKeyword.Text.Trim());
            DialogResult = true;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void Delay_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Node.Delay = (int)e.NewValue;
        }
    }
}
