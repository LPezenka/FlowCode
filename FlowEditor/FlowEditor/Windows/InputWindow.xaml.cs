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
    /// Interaktionslogik für InputWindow.xaml
    /// </summary>
    public partial class InputWindow : Window, IDisposable //, IObservable<string>, IDisposable
    {
//        public IObserver<string> Observer { get; set; }

        public string TextResult { get; set; }

        public InputWindow(string prompt)
        {
            InitializeComponent();
            FocusManager.SetFocusedElement(this, TextBoxInput);
            this.SizeToContent = SizeToContent.WidthAndHeight;
            this.ResizeMode = ResizeMode.NoResize;
        }

        private void ButtonConfirm_Click(object sender, RoutedEventArgs e)
        {
            //Observer?.OnNext(TextBoxInput.Text);
            TextResult = TextBoxInput.Text;
            DialogResult = true;
        }

        public void Dispose()
        {

        }

        private void TextBoxInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
                ButtonConfirm_Click(null, null);
        }
    }
}
