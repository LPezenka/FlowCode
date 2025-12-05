using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
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

    internal class TipItem
    {
        public string Text { get; set; }
        public string Source { get; set; }
    }

    /// <summary>
    /// Interaktionslogik für WelcomeWindow.xaml
    /// </summary>
    public partial class WelcomeWindow : Window
    {
        int _tipCounter = 0;
        List<TipItem> _tips;

        public WelcomeWindow(string path)
        {
            InitializeComponent();
            this.ResizeMode = ResizeMode.NoResize;
            ParseJSON(path);
            DisplayHint();
            
        }

        private bool DisplayHint()
        {
            if (_tips.Count > _tipCounter)
            {
                HelpText.Text = _tips[_tipCounter].Text;
                var imgpath = System.IO.Path.Combine(Environment.CurrentDirectory, _tips[_tipCounter].Source);
                if (!File.Exists(imgpath)) return false;
                HelpImage.Source = new BitmapImage(new Uri(imgpath));
   
            }

            return true;
        }

        private void ParseJSON(string path)
        {
            string json = File.ReadAllText(path);
            _tips = JsonSerializer.Deserialize<List<TipItem>>(json);
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _tipCounter++;
            if (_tipCounter > _tips.Count)
                _tipCounter = 0;
            DisplayHint();
        }

        private void Previous_Click(object sender, RoutedEventArgs e)
        {
            _tipCounter--;
            if (_tipCounter < 0)
                _tipCounter = _tips.Count;
            DisplayHint();
        }
    }
}
