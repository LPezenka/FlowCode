using FlowCodeInfrastructure;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
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
using static FlowCodeInfrastructure.Config;

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
            TextBoxYesKeyword.Text = FlowCodeInfrastructure.Config.GetKeyword(FlowCodeInfrastructure.Config.KeyWord.True);
            TextBoxNoKeyword.Text = FlowCodeInfrastructure.Config.GetKeyword(FlowCodeInfrastructure.Config.KeyWord.False);
            TextBoxInputKeyword.Text = FlowCodeInfrastructure.Config.GetKeyword(FlowCodeInfrastructure.Config.KeyWord.Input);
            TextBoxOutputKeyword.Text = FlowCodeInfrastructure.Config.GetKeyword(FlowCodeInfrastructure.Config.KeyWord.Output);
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
            OkButton_Click(sender, e);
            
            Configuration configuration = ConfigurationManager.OpenExeConfiguration(Assembly.GetExecutingAssembly().Location);

            ConfigurationManager.AppSettings["KeyWordYes"] = FlowCodeInfrastructure.Config.GetKeyword(FlowCodeInfrastructure.Config.KeyWord.True);
            ConfigurationManager.AppSettings["KeyWordNo"] = FlowCodeInfrastructure.Config.GetKeyword(FlowCodeInfrastructure.Config.KeyWord.False);
            ConfigurationManager.AppSettings["KeyWordInput"] = FlowCodeInfrastructure.Config.GetKeyword(FlowCodeInfrastructure.Config.KeyWord.Input);
            ConfigurationManager.AppSettings["KeyWordOutput"] = FlowCodeInfrastructure.Config.GetKeyword(FlowCodeInfrastructure.Config.KeyWord.Output);

            configuration.Save(ConfigurationSaveMode.Full, true);
            ConfigurationManager.RefreshSection("appSettings");

            FlowCodeInfrastructure.Config.Save();


            string settingString = string.Empty;
            ConfigurationManager.AppSettings.AllKeys.ToList().ForEach(key =>
            {
                settingString += $"{key}: {ConfigurationManager.AppSettings[key]}\n";
            });
            if (!Directory.Exists("./config"))
                Directory.CreateDirectory("./config");
            if (!File.Exists("./config/settings.config"))
                File.Create("./config/settings.config").Close();

            File.WriteAllText("./config/settings.config", settingString);

        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void Delay_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Node.Delay = (int)e.NewValue;
            Config.Delay = Node.Delay;
        }
    }
}
