using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Interfaces;

namespace NodeControlPrototype
{
    public class InputHandler : IInputHandler
    {
        public InputWindow Dialog { get; set; }
        public string Response { get; set; }

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public void OnNext(string value)
        {
            Response = value;
        }

        public string ReadInput(string prompt = "")
        {
            return Application.Current.Dispatcher.Invoke(() =>
            {
                using (Dialog = new InputWindow(prompt))
                {
                    Dialog.LabelOutput.Content = prompt;
                    var result = Dialog.ShowDialog();
                    if (result == true)
                    {
                        return Dialog.TextResult;
                    }
                }
                return string.Empty;
            });
        }
    }
}
