using QuanshengDock.Channels;
using QuanshengDock.Data;
using QuanshengDock.General;
using QuanshengDock.UI;
using QuanshengDock.View;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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

namespace QuanshengDock.ExtendedVFO
{
    /// <summary>
    /// Interaction logic for Scanner.xaml
    /// </summary>
    public partial class Scanner : Window
    {
        private static readonly ViewModel<ObservableCollection<ScanList>> Command = VM.Get<ObservableCollection<ScanList>>("ScanLists");
        private static readonly ViewModel<ObservableCollection<VFOPreset>> selected = VM.Get<ObservableCollection<VFOPreset>>("SelectedScanList");
        private static readonly ViewModel<bool> scanning = VM.Get<bool>("BusyXVFO");
        private static readonly ViewModel<string> scanMessage = VM.Get<string>("ScanMessage");
        private static readonly Brush upBrush = new SolidColorBrush(Color.FromArgb(255, 0x30, 0x30, 0x30));

        private static readonly Brush upButt = new SolidColorBrush(Color.FromArgb(255, 0x20, 0x20, 0x20));
        private static readonly Brush downButt = new SolidColorBrush(Color.FromArgb(255, 0x10, 0x10, 0x10));
        private static ButtonBorder? pressedButt = null;
        private static ScanList? selectedList = null;

        public static Scanner? Instance { get; private set; } = null;

        public static bool IsOpened { get; set; } = false;

        public Scanner()
        {
            IsOpened = true;
            Instance = this;
            DataContext = Context.Instance;
            InitializeComponent();
            selected.Value = null!;
            scanning.PropertyChanged += Scanning_PropertyChanged;
            Closing += Scanner_Closing;
        }

        private void Scanner_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            IsOpened = false;
            Instance = null;
            BK4819.StopScan();
        }

        private void Scanning_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(scanning.Value)
            {
                Presets.SelectedIndex = -1;
                InList.SelectedIndex = -1;
            }
        }

        protected override void OnActivated(EventArgs e)
        {
            MainWindow.Instance?.Focus();
            base.OnActivated(e);
        }

        private static void Dehighlight()
        {
            if (pressedButt != null)
            {
                pressedButt.Background = upButt;
                pressedButt = null;
                Radio.PulseTX = false;
            }
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            Dehighlight();
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            Dehighlight();
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            switch (e.Key)
            {
                case Key.Escape:
                    Close();
                    break;
                case Key.Space:
                    BK4819.StopScan();
                    break;
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            if (Mouse.DirectlyOver is ButtonBorder butt)
            {
                butt.Background = downButt;
                pressedButt = butt;
            }
            else
            if(Mouse.DirectlyOver is Image)
            {
                if (hoveredPreset != null)
                {
                    BK4819.ForceMonitor = hoveredPreset;
                    hoveredPreset = null;
                }
            }
            else
            if (Mouse.DirectlyOver is Border)
                this.DragMove();
            
        }

        private static void DisplayListMessage()
        {
            if (selectedList != null)
                scanMessage.Value = $"Scanning List {selectedList.Name}";
        }

        private void Lists_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach(var v in Lists.Items)
            {
                if(v is ScanList slist)
                {
                    if(slist.IsSelected = Lists.SelectedItems.Contains(slist))
                    {
                        selectedList = slist;
                        DisplayListMessage();
                    }
                }
            }
            Command.Execute(e.Source);
        }

        private void Presets_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach(var v in Presets.Items)
            {
                if(v is VFOPreset preset) 
                {
                    preset.IsSelected = Presets.SelectedItems.Contains(preset);
                }
            }
        }

        private void InList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(scanning.Value) InList.SelectedIndex = -1;
            foreach (var v in InList.Items)
            {
                if (v is VFOPreset preset)
                {
                    preset.IsSelectedInList = InList.SelectedItems.Contains(preset);
                }
            }
        }

        private void Border_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border b)
            {
                b.Background = new SolidColorBrush(Colors.Black);
                _ = ClearBG(b);
            }
        }

        private static async Task ClearBG(Border b)
        {
            await Task.Delay(250);
            b.Background = upBrush;
        }

        private void Exit_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Command.Execute("Stop");
            Close();
        }

        private void Drowdown_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                if (sender is FrameworkElement element)
                {
                    ContextMenu contextMenu = element.ContextMenu;
                    if (contextMenu != null)
                    {
                        contextMenu.PlacementTarget = element;
                        contextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                        contextMenu.IsOpen = true;
                    }
                }
            }
        }

        private VFOPreset? hoveredPreset = null;
        private void ScanGraph_MouseMove(object sender, MouseEventArgs e)
        {
            var sel = selected.Value;
            if (sel != null && sel.Count > 0)
            {
                Point p = Mouse.GetPosition(ScanGraph);
                int index = (int)((p.X * sel.Count) / ScanGraph.ActualWidth);
                var preset = sel[index.Clamp(0, sel.Count - 1)];
                hoveredPreset = preset;
                scanMessage.Value = $"{preset.PName} {preset.RX:F5}";
            }
        }

        private void ScanGraph_MouseLeave(object sender, MouseEventArgs e)
        {
            DisplayListMessage();
        }
    }
}
