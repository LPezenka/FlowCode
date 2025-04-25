using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NodeControlPrototype
{
    public class RhombusNodeControl : NodeControlBase
    {

        public RhombusNodeControl()
        {
            FirstOutputIndex = 1; 
        }

            static RhombusNodeControl()
            {
                DefaultStyleKeyProperty.OverrideMetadata(typeof(RhombusNodeControl),
                    new FrameworkPropertyMetadata(typeof(RhombusNodeControl)));
        }

            public override List<Point> GetConnectionPoints() => new()
        {
            new Point(ActualWidth / 2, 0),               // oben (Eingang)
            new Point(0, ActualHeight / 2),              // links (Ausgang 1)
            new Point(ActualWidth, ActualHeight / 2)     // rechts (Ausgang 2)
        };
    }
}
