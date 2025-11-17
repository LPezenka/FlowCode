using Interfaces;
using System;
using System.Collections.Generic;
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

namespace NodeControlPrototype.Controls
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
    ///     <MyNamespace:DeleteZone/>
    ///
    /// </summary>
    public class DeleteZone : Control, IHighlightable, INotifyPropertyChanged
    {

        public Brush BackgroundColor
        {
            get => Background;
            set
            {
                Background = value;
                NotifyPropertyChanged("Background");
            }
        }

        static DeleteZone()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DeleteZone), new FrameworkPropertyMetadata(typeof(DeleteZone)));
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public void SetActive(bool active)
        {
            throw new NotImplementedException();
        }

        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "Background")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Not what I wanted. I need simple intersection rather than Drag and Drop...
        //public void OnDragEnter(object sender, DragEventArgs e)
        //{
        //    if (e.Data.GetDataPresent(typeof(FrameworkElement)))
        //    {
        //        e.Effects = DragDropEffects.Move;
        //        Background = Brushes.Red;
        //        (FindName("DeleteZoneText") as TextBox).Text = "Release to delete";
        //    }
        //    else
        //    {
        //        e.Effects = DragDropEffects.None;
        //    }
        //}
    }
}
