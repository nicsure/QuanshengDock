using QuanshengDock.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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

namespace QuanshengDock.RepeaterBook
{
    /// <summary>
    /// Interaction logic for RepeaterBookBrowser.xaml
    /// </summary>
    public partial class RepeaterBookBrowser : Window
    {
        private static readonly ViewModel<ObservableCollection<Repeater>> selected = VM.Get<ObservableCollection<Repeater>>("BookSelected");

        public RepeaterBookBrowser()
        {
            DataContext = BookContext.Instance;
            InitializeComponent();
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            if (Mouse.DirectlyOver is Border)
                this.DragMove();
        }

        private void Frame_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Close();
        }

        private void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            if(e.Row.Item is Repeater repeater)
            {
                e.Row.Opacity = repeater.Status.Equals("On-air") ? 1.0 : 0.6;
            }
        }

        private void DataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            selected.Value.Clear();
            foreach(var v in RepeaterGrid.SelectedItems)
            {
                if(v is Repeater repeater)
                {
                    selected.Value.Add(repeater);
                }
            }
        }
    }
}
