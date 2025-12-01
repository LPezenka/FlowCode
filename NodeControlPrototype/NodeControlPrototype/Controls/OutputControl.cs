using Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FlowEditor.Controls
{
    /// <summary>
    /// Führen Sie die Schritte 1a oder 1b und anschließend Schritt 2 aus, um dieses benutzerdefinierte Steuerelement in einer XAML-Datei zu verwenden.
    ///
    /// Schritt 1a) Verwenden des benutzerdefinierten Steuerelements in einer XAML-Datei, die im aktuellen Projekt vorhanden ist.
    /// Fügen Sie dieses XmlNamespace-Attribut dem Stammelement der Markupdatei 
    /// an der Stelle hinzu, an der es verwendet werden soll:
    ///
    ///     xmlns:MyNamespace="clr-namespace:NodeControlPrototype.Controls"
    ///
    ///
    /// Schritt 1b) Verwenden des benutzerdefinierten Steuerelements in einer XAML-Datei, die in einem anderen Projekt vorhanden ist.
    /// Fügen Sie dieses XmlNamespace-Attribut dem Stammelement der Markupdatei 
    /// an der Stelle hinzu, an der es verwendet werden soll:
    ///
    ///     xmlns:MyNamespace="clr-namespace:NodeControlPrototype.Controls;assembly=NodeControlPrototype.Controls"
    ///
    /// Darüber hinaus müssen Sie von dem Projekt, das die XAML-Datei enthält, einen Projektverweis
    /// zu diesem Projekt hinzufügen und das Projekt neu erstellen, um Kompilierungsfehler zu vermeiden:
    ///
    ///     Klicken Sie im Projektmappen-Explorer mit der rechten Maustaste auf das Zielprojekt und anschließend auf
    ///     "Verweis hinzufügen"->"Projekte"->[Navigieren Sie zu diesem Projekt, und wählen Sie es aus.]
    ///
    ///
    /// Schritt 2)
    /// Fahren Sie fort, und verwenden Sie das Steuerelement in der XAML-Datei.
    ///
    ///     <MyNamespace:OutputWindow/>
    ///
    /// </summary>
    public class OutputControl : Control, IOutputHandler, INotifyPropertyChanged, IVariableLogger
    {
        public List<string> OutputMessages { get; set; } = new List<string>();

        private bool _isDragging;
        private Point _dragStart;

        static OutputControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(OutputControl), new FrameworkPropertyMetadata(typeof(OutputControl)));
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "ListContent")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void ShowOutput(string output)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var t = Template;
                if (t is not null)
                {
                    OutputMessages.Add(output);
                    var messages = t.FindName("Messages", this) as ListBox;
                    messages?.Items.Refresh();
                }
            });
        }

        public void Reset()
        {
            var t = Template;
            if (t is not null)
            {
                OutputMessages.Clear();
                var messages = t.FindName("Messages", this) as ListBox;
                messages?.Items.Refresh();
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            var pos = e.GetPosition(this);
            _isDragging = true;
            _dragStart = e.GetPosition(Parent as UIElement);
            CaptureMouse();
            e.Handled = true;
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            _isDragging = false;
            ReleaseMouseCapture();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (_isDragging && e.LeftButton == MouseButtonState.Pressed)
            {
                var parent = Parent as Canvas;
                if (parent != null)
                {
                    Point currentPos = e.GetPosition(parent);
                    double offsetX = currentPos.X - _dragStart.X;
                    double offsetY = currentPos.Y - _dragStart.Y;

                    double newLeft = Canvas.GetLeft(this) + offsetX;
                    double newTop = Canvas.GetTop(this) + offsetY;

                    Canvas.SetLeft(this, newLeft);
                    Canvas.SetTop(this, newTop);
                    _dragStart = currentPos;

                }
            }
            else
            {
                var pos = e.GetPosition(this);
                InvalidateVisual();
            }
        }

        public Brush BackgroundColor
        {
            get
            {
                return Background;
            }
            set
            {
                Background = value;
                NotifyPropertyChanged("Background");
            }
        }

        public void SetTitle(string title)
        {
            var t = Template;
            if (t is not null)
            {
                var heading = t.FindName("Heading", this) as Label;
                heading.Content = title;
                NotifyPropertyChanged("Heading");
            }
        }

        public void LogVariables(IEnumerable variables)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                OutputMessages.Clear();
                foreach (var v in variables)
                {
                    var variable = (v as Microsoft.CodeAnalysis.Scripting.ScriptVariable);
                    ShowOutput($"{variable.Name} : {variable.Value} ({variable.Type})");
                }
            });
        }
    }
}
