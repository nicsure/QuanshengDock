using QuanshengDock.Data;
using QuanshengDock.Serial;
using QuanshengDock.UI;
using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace QuanshengDock.Settings
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            GC.Collect();
            DataContext = Context.Instance;
            InitializeComponent();
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            if (Mouse.DirectlyOver is Border)
                this.DragMove();
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) Defocusser.Focus();
        }
        
        private void Register_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Defocusser.Focus();
                if (RegBox.Text.Length > 0)
                {
                    string s = RegBox.Text;
                    RegBox.Text = string.Empty;
                    if (int.TryParse(s, System.Globalization.NumberStyles.HexNumber, null, out int i))
                    {
                        Comms.SendCommand(Packet.ReadRegisters, (ushort)1, (ushort)i);
                    }
                }
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Clipboard.SetText(e.Uri.AbsoluteUri);
        }
    }
}
