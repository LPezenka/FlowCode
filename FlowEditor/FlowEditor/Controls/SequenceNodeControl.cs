using FlowCodeInfrastructure;
using FlowEditor.Windows;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace FlowEditor.Controls
{
    public class SequenceNodeControl : NodeControlBase
    {

        //public static Window DetailWindow { get; set; }
        public override void ShowDetailWindow()
        {
            List<string> snippets = ["Forward()", "Left()", "PickUpBox()", "PlaceBox()"];
            snippets.Add($"v = {Config.GetKeyword(Config.KeyWord.Input)}");
            snippets.Add($"{Config.GetKeyword(Config.KeyWord.Output)}: v");
            snippets.Add("i = 0");
            snippets.Add("s = \"\"");

            SequenceNodeDetailWindow sdWnd = new SequenceNodeDetailWindow(this, snippets);
            sdWnd.ShowDialog();
            //DetailWindow?.ShowDialog();
        }

        static SequenceNodeControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SequenceNodeControl),
                new FrameworkPropertyMetadata(typeof(SequenceNodeControl)));
        }

        public SequenceNodeControl():base()
        {
            //Background = OriginalBackground;
        }

        public override List<Point> GetConnectionPoints() => new()
        {
            new Point(ActualWidth / 2, -1),             // Eingang: Mitte oben
                new Point(ActualWidth / 2, ActualHeight + 1),   // Ausgang: Mitte unten
            //new Point(0, ActualHeight / 2),
            //new Point(ActualWidth, ActualHeight / 2)
            //new Point(ActualWidth / 2, 0),
            //new Point(ActualWidth / 2, ActualHeight),
            //new Point(0, ActualHeight / 2),
            //new Point(ActualWidth, ActualHeight / 2)
        };
    }
}
