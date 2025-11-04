using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NodeControlPrototype
{
    internal class ProcessNodeControl : NodeControlBase
    {
        // New Control: ProcessControl
        public static readonly DependencyProperty TargetNodeProperty = DependencyProperty.Register(
nameof(TargetNode), typeof(Node), typeof(ProcessNodeControl), new PropertyMetadata(null));


        public Node TargetNode
        {
            get => (Node)GetValue(TargetNodeProperty);
            set => SetValue(TargetNodeProperty, value);
        }

        static ProcessNodeControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ProcessNodeControl),
            new FrameworkPropertyMetadata(typeof(ProcessNodeControl)));
        }


        public ProcessNodeControl()
        {
            Width = 180;
            Height = 80;
        }


        public override List<Point> GetConnectionPoints() => new()
        {
  
            new Point(ActualWidth / 2, 0),                // Top (Eingang)
            new Point(ActualWidth / 2, ActualHeight),     // Bottom (Ausgang)
            new Point(ActualWidth, ActualHeight / 2)      // Right (Prozedur-Ziel
        };
    }


    // Style Template for Generic.xaml
    /*
    <Style TargetType="{x:Type fc:ProcessControl}">
    <Setter Property="Template">
    <Setter.Value>
    <ControlTemplate TargetType="{x:Type fc:ProcessControl}">
    <Border Background="White"
    BorderBrush="Black"
    BorderThickness="1"
    Padding="5"
    SnapsToDevicePixels="True">
    <Grid>
    <Grid.ColumnDefinitions>
    <ColumnDefinition Width="3"/>
    <ColumnDefinition Width="*"/>
    <ColumnDefinition Width="3"/>
    </Grid.ColumnDefinitions>
    <Rectangle Grid.Column="0" Fill="Black" Width="3" HorizontalAlignment="Left" />
    <TextBlock Grid.Column="1"
    Text="{Binding NodeData.Title, RelativeSource={RelativeSource TemplatedParent}}"
    FontWeight="Bold"
    FontSize="14"
    HorizontalAlignment="Center"
    VerticalAlignment="Center"/>
    <Rectangle Grid.Column="2" Fill="Black" Width="3" HorizontalAlignment="Right" />
    </Grid>
    </Border>
    </ControlTemplate>
    </Setter.Value>
    </Setter>
    </Style>
    */


    // Integration into EdgeControl rendering (pseudo-logic)
    // if edge is from ProcessControl to its TargetNode => dashed style
    // inside OnRender of EdgeControl:
    /*
    bool isProcessCall = From is ProcessControl process && process.TargetNode == To?.NodeData;
    var pen = new Pen(Brushes.Black, 2);
    if (isProcessCall)
    {
    pen.DashStyle = DashStyles.Dash;
    }
    */
}