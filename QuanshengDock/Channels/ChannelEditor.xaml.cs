using QuanshengDock.Data;
using QuanshengDock.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace QuanshengDock.Channels
{
    // code by -nicsure- 2024
    // https://www.youtube.com/nicsure

    public partial class ChannelEditor : Window
    {
        private static readonly Brush upButt = new SolidColorBrush(Color.FromArgb(255, 0x20, 0x20, 0x20));
        private static readonly Brush downButt = new SolidColorBrush(Color.FromArgb(255, 0x10, 0x10, 0x10));
        private static ButtonBorder? pressedButt = null;
        private static readonly Brush groupedBrush = new SolidColorBrush(Colors.DarkBlue);
        private static readonly Brush transBrush = new SolidColorBrush(Colors.Transparent);
        private static readonly Brush whiteBrush = new SolidColorBrush(Colors.White);
        private static readonly Brush greyBrush = new SolidColorBrush(Colors.DarkGray);


        public ChannelEditor()
        {
            DataContext = Context.Instance;
            InitializeComponent();
        }
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            if (Mouse.DirectlyOver is ButtonBorder butt)
            {
                switch (butt.Tag)
                {
                    case "Group":
                        GroupChannels(true);
                        break;
                    case "Ungroup":
                        GroupChannels(false);
                        break;
                }
                butt.Background = downButt;
                pressedButt = butt;
            }
            else
            if (Mouse.DirectlyOver is Border)
                this.DragMove();
        }

        private void GroupChannels(bool group)
        {
            foreach(var v in ChannelGrid.SelectedItems)
            {
                if (v is GridChannel channel)
                {
                    channel.Group(group);
                }
            }
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            Dehighlight();
        }

        private static void Dehighlight()
        {
            if (pressedButt != null)
            {
                pressedButt.Background = upButt;
                pressedButt = null;
            }
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            Dehighlight();
        }

        private void DataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            e.Column.CanUserReorder = false;
            e.Column.CanUserResize = false;
            e.Column.CanUserSort = false;
            switch (e.Column)
            {
                case DataGridTextColumn:
                    {
                        DataGridTextColumn col = (DataGridTextColumn)e.Column;
                        switch (e.PropertyName)
                        {
                            case "G":
                                col.Header = null;
                                break;
                            case "TX":
                            case "RX":
                                col.Binding = new Binding(e.PropertyName) { StringFormat = "F5" };
                                break;
                            case "Number":
                                col.Binding = new Binding(e.PropertyName) { StringFormat = "D3" };
                                break;
                        }
                    }
                    break;
                case DataGridComboBoxColumn:
                    {
                        DataGridComboBoxColumn col = (DataGridComboBoxColumn)e.Column;
                        switch (e.PropertyName)
                        {
                            case "RxCTCSS":
                            case "TxCTCSS":
                                col.ItemsSource = Beautifiers.Ctcss.Strings;
                                col.SelectedItemBinding = new Binding(e.PropertyName) { Converter = Beautifiers.Ctcss };
                                break;
                            case "RxDCS":
                            case "TxDCS":
                                col.ItemsSource = Beautifiers.Dcs.Strings;
                                col.SelectedItemBinding = new Binding(e.PropertyName) { Converter = Beautifiers.Dcs };
                                break;
                            case "Step":
                                col.ItemsSource = Beautifiers.Step.Strings;
                                col.SelectedItemBinding = new Binding(e.PropertyName) { Converter = Beautifiers.Step };
                                break;
                            case "Scramble":
                                col.ItemsSource = Beautifiers.Scramble.Strings;
                                col.SelectedItemBinding = new Binding(e.PropertyName) { Converter = Beautifiers.Scramble };
                                break;
                        }
                    }
                    break;
            }
        }

        private void Frame_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Close();
        }

        private void ChannelGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            if(e.Row.Item is GridChannel channel)
            {
                channel.SetRow(e.Row);
                e.Row.Background = channel.IsGrouped() ? groupedBrush : transBrush;
                e.Row.Foreground = channel.IsInUse() ? whiteBrush : greyBrush;
            }
        }

        private void Magnify_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (ChannelGrid != null && ChannelGrid.Columns != null)
            {
                foreach (var col in ChannelGrid.Columns)
                {
                    if (col == null) continue;
                    col.Width = 0;
                    col.Width = DataGridLength.Auto;
                }
            }
        }

        private void ChannelGrid_UnloadingRow(object sender, DataGridRowEventArgs e)
        {
            if (e.Row.Item is GridChannel channel)
                channel.SetRow(null);
        }
    }
}
