using QuanshengDock.Data;
using QuanshengDock.Serial;
using QuanshengDock.UI;
using QuanshengDock.View;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        private static readonly ViewModel<string> pass = VM.Get<string>("NPass");
        public SettingsWindow()
        {
            GC.Collect();
            DataContext = Context.Instance;
            InitializeComponent();
            NPass.Password = pass.Value;
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            if (Mouse.DirectlyOver is Border)
                this.DragMove();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            pass.Value = NPass.Password;
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) Defocusser.Focus();
            if(sender is PasswordBox pb)
                pass.Value = pb.Password;
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
