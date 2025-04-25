using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NodeControlPrototype
{
    public class RectangleNodeControl : NodeControlBase
    {
        static RectangleNodeControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RectangleNodeControl),
                new FrameworkPropertyMetadata(typeof(RectangleNodeControl)));
        }

        public override List<Point> GetConnectionPoints() => new()
        {
            new Point(ActualWidth / 2, 0),             // Eingang: Mitte oben
                new Point(ActualWidth / 2, ActualHeight)   // Ausgang: Mitte unten

            //new Point(ActualWidth / 2, 0),
            //new Point(ActualWidth / 2, ActualHeight),
            //new Point(0, ActualHeight / 2),
            //new Point(ActualWidth, ActualHeight / 2)
        };
    }
}
