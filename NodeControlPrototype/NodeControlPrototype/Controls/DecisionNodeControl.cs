using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FlowEditor.Controls
{
    public class DecisionNodeControl : NodeControlBase
    {

        public DecisionNodeControl() : base()
        {
            FirstOutputIndex = 1; 
        }

            static DecisionNodeControl()
            {
                DefaultStyleKeyProperty.OverrideMetadata(typeof(DecisionNodeControl),
                    new FrameworkPropertyMetadata(typeof(DecisionNodeControl)));
        }

            public override List<Point> GetConnectionPoints() => new()
        {
            new Point(ActualWidth / 2, 0),               // oben (Eingang)
            new Point(0, ActualHeight / 2),              // links (Ausgang 1)
            new Point(ActualWidth, ActualHeight / 2)     // rechts (Ausgang 2)
        };
    }
}
