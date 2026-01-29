using FlowEditor.Windows;
using System.Windows;
using System.Windows.Shapes;

namespace FlowEditor.Controls
{
    public class DecisionNodeControl : NodeControlBase
    {
        public int StrokeThickness { get; set; }
        public DecisionNodeControl() : base()
        {
            FirstOutputIndex = 1;
            StrokeThickness = 1;
        }

            static DecisionNodeControl()
            {
                DefaultStyleKeyProperty.OverrideMetadata(typeof(DecisionNodeControl),
                    new FrameworkPropertyMetadata(typeof(DecisionNodeControl)));
        }

        public override void ShowDetailWindow()
        {
            List<string> snippets = ["CellHasBox()", "CanMove()", "TruckIsEmpty()", "i < n", "i > 0"];
            
            
            SequenceNodeDetailWindow sndWnd = new SequenceNodeDetailWindow(this, snippets);
            sndWnd.ShowDialog();
        }

            public override List<Point> GetConnectionPoints() => new()
        {
            new Point(ActualWidth / 2, 0),               // oben (Eingang)
            new Point(0, ActualHeight / 2),              // links (Ausgang 1)
            new Point(ActualWidth, ActualHeight / 2)     // rechts (Ausgang 2)
        };

        public override void SetActive(bool active)
        {
            base.SetActive(active);
            Application.Current.Dispatcher.Invoke(() =>
            {
                var t = this.Template;
                if (t is not null)
                {
                    if (t.FindName("BorderPolygon", this) is not Polygon polygon) return;
                    if (active)
                    {
                        polygon.StrokeThickness = 9;
                    }
                    else
                    {
                        polygon.StrokeThickness = 1;
                    }
                }

                //if (active)
                //    StrokeThickness = 9;
                //else
                //    StrokeThickness = 1;
                NotifyPropertyChanged(nameof(StrokeThickness));
            });
            }
    }
}
