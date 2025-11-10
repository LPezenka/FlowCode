using Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NodeControlPrototype
{
    class OutputHandler : IOutputHandler
    {
        public void ShowOutput(string prompt)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                using (var Dialog = new OutputWindow(prompt))
                {
                    return Dialog.ShowDialog();

                }
            });
        }
    }
}
