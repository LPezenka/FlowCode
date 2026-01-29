using FlowCodeInfrastructure;
using FlowEditor.Controls;
using System;
using System.Collections.Generic;
using System.IO;
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
    /// Interaktionslogik für ActionNodeDetailWindow.xaml
    /// </summary>
    /// 

    public partial class SequenceNodeDetailWindow : Window
    {
        public List<string> Snippets { get; set; }
        public NodeControlBase Node { get; set; }

 
        public static List<string> StaticSnippets { get; set; }

        /// <summary>
        /// Load Code Snippets from File
        /// </summary>
        /// <param name="path">path of the snippet file</param>
        public static void Load(string path)
        {
            string[] lines = File.Exists(path) ? File.ReadAllLines(path) : null;
            if (lines is not null)
            {
                foreach (string line in lines)
                {
                    var s = line.Trim().Replace(";", "");
                    if (!StaticSnippets.Contains(s))
                    {
                        StaticSnippets.Add(s);
                    }
                }
            }
        }

        public SequenceNodeDetailWindow(NodeControlBase node, List<string> snippets = null)
        {
            if (snippets is not null)
                Snippets = snippets;
            else
                Snippets = new List<string>();

            foreach (string s in StaticSnippets)
                if (!Snippets.Contains(s))
                    Snippets.Add(s);
    
                InitializeComponent();

            if (node is not null)
            {
                this.Node = node;
                this.DataContext = Node;
            }

            CodeFragments.ItemsSource = Snippets;
            CodeFragments.Items.Refresh();
            CodeFragments.MouseDoubleClick += CodeFragments_MouseDoubleClick;
        }

        private void CodeFragments_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            string fragment = CodeFragments.SelectedItem as string;
            Code.Text = fragment;
        }

        private void Button_Ok_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Fix binding
            Node.Code = Code.Text;
            DialogResult = true;
        }

        private void Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
