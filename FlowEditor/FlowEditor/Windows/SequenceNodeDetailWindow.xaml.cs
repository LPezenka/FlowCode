using FlowCodeInfrastructure;
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
    /// Interaktionslogik für ActionNodeDetailWindow.xaml
    /// </summary>
    /// 

    public partial class SequenceNodeDetailWindow : Window
    {
        public List<string> Snippets = ["Forward()", "Left()", "PickupBox()", "PlaceBox()"];
        public SequenceNodeControl Node { get; set; }

        public SequenceNodeDetailWindow(SequenceNodeControl node)
        {
            InitializeComponent();

            /*foreach (var f in FlowCodeInfrastructure.Config.KeywordMapper)
            {
                Snippets.Add(f.Value);
            }*/

            if (node is not null)
            {
                this.Node = node;
                this.DataContext = Node;
            }

            Snippets.Add($"v = {Config.GetKeyword(Config.KeyWord.Input)}");
            Snippets.Add($"{Config.GetKeyword(Config.KeyWord.Output)}: v");
            Snippets.Add("i = 0");
            Snippets.Add("s = \"\"");

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
