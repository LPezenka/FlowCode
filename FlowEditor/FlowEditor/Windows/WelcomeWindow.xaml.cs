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
            ParseJSON(path);
            if (_tips.Count > 0)
            {
                HelpText.Text = _tips[0].Text;
                var imgpath = System.IO.Path.Combine(Environment.CurrentDirectory, "res", "terminal.png");
                if (!File.Exists(imgpath)) return;
                HelpImage.Source = new BitmapImage(new Uri(imgpath));
                //new System.Uri(_tips[0].Source));
            }
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
    }
}
