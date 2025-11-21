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

namespace NodeControlPrototype.Controls
{
    public class SequenceNodeControl : NodeControlBase
    {
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
            new Point(ActualWidth / 2, 0),             // Eingang: Mitte oben
                new Point(ActualWidth / 2, ActualHeight),   // Ausgang: Mitte unten
            new Point(0, ActualHeight / 2),
            new Point(ActualWidth, ActualHeight / 2)
            //new Point(ActualWidth / 2, 0),
            //new Point(ActualWidth / 2, ActualHeight),
            //new Point(0, ActualHeight / 2),
            //new Point(ActualWidth, ActualHeight / 2)
        };
    }
}
