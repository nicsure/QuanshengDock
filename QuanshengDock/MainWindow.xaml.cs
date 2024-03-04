using NAudio.Wave;
using QuanshengDock.Data;
using QuanshengDock.ExtendedVFO;
using QuanshengDock.General;
using QuanshengDock.UI;
using QuanshengDock.User;
using QuanshengDock.View;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
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
using Xceed.Wpf.Toolkit;

namespace QuanshengDock
{
    // code by -nicsure- 2024
    // https://www.youtube.com/nicsure
    
    public partial class MainWindow : Window
    {
        public static ScrollViewer PresetRelative => presetRelative;
        private static ScrollViewer presetRelative = null!;

        private static readonly Brush upButt = new SolidColorBrush(Color.FromArgb(255, 0x20, 0x20, 0x20));
        private static readonly Brush downButt = new SolidColorBrush(Color.FromArgb(255, 0x10, 0x10, 0x10));
        private static ButtonBorder? pressedButt = null;
        public static MainWindow? Instance { get; private set; } = null;
        private readonly VM command;
        private readonly ViewModel<bool> txLockButtonLocked;
        private readonly ViewModel<bool> scanning;
        private readonly ViewModel<double> rxFreq;
        private readonly ViewModel<int> xvfoStep;
        private readonly ViewModel<bool> openSquelch;
        private Key lastKey = Key.None;
        private double mouseOverDigit = 0;

        public MainWindow()
        {
            Instance = this;
            DataContext = Context.Instance;
            openSquelch = VM.Get<bool>("OpenSquelch");
            command = VM.Get("MouseCommand");
            txLockButtonLocked = VM.Get<bool>("TxLockButtonLocked");
            scanning = VM.Get<bool>("BusyXVFO");
            rxFreq = VM.Get<double>("XVfoRxFreq");
            xvfoStep = VM.Get<int>("XVfoStep");
            InitializeComponent();
            presetRelative = PresetScroller;
            if (!Radio.DesignMode)
            {
                ToggleSpectrum();
                Width = 400;
                XvfoCol.Width= new(0, GridUnitType.Star);
            };
        }

        public static Point MouseRelative()
        {
            if(Instance == null) return new Point();
            return Mouse.GetPosition(Instance.SpectrumImage);
        }

        public static void Maximizer()
        {
            if (Instance != null)
            {
                switch (Instance.WindowState)
                {
                    case System.Windows.WindowState.Normal:
                        Instance.WindowState = System.Windows.WindowState.Maximized;
                        break;
                    case System.Windows.WindowState.Maximized:
                        Instance.WindowState = System.Windows.WindowState.Normal;
                        break;
                }
            }
        }

        public static void ToggleXvfo()
        {
            if (Instance != null)
            {
                if (Instance.SpectrumCol.Width.Value != 0) ToggleSpectrum();
                double d = Instance.MainCol.ActualWidth;
                if (d == 0)
                {
                    Instance.MainCol.Width = new(1.0, GridUnitType.Star);
                    Instance.XvfoCol.Width = new(0.0, GridUnitType.Star);
                    Instance.Width = Instance.ActualWidth / 1.5;
                    XVFO.Stop();
                    Scanner.Instance?.Close();
                    Messenger.Instance?.Close();
                }
                else
                {
                    Instance.MainCol.Width = new(0.0, GridUnitType.Star);
                    Instance.XvfoCol.Width = new(1.0, GridUnitType.Star);
                    Instance.Width = Instance.ActualWidth * 1.5;
                    XVFO.Start();
                }
            }
        }

        protected override void OnActivated(EventArgs e)
        {
            Scanner.Instance?.Focus();
            Messenger.Instance?.Focus();
            base.OnActivated(e);
        }

        public static void ToggleSpectrum()
        {
            if (Instance != null)
            {
                double d = Instance.MainCol.ActualWidth;
                switch (Instance.SpectrumCol.Width.Value)
                {
                    case 0.0:
                        Instance.SpectrumCol.Width = new(1, GridUnitType.Star);
                        Radio.SpectrumVisible = true;
                        Instance.Width = Instance.ActualWidth + d;
                        break;
                    default:
                        Instance.SpectrumCol.Width = new(0.0, GridUnitType.Star);
                        Radio.SpectrumVisible = false;
                        Instance.Width = Instance.ActualWidth - d;
                        break;
                }
            }
        }

        public static void Minimizer()
        {
            if(Instance != null)
            {
                Instance.WindowState = System.Windows.WindowState.Minimized;
            }
        }

        public static double SpectrumImageWidth() => Instance?.SpectrumImage.ActualWidth ?? 0;
        public static double SpectrumImageHeight() => Instance?.SpectrumImage.ActualHeight ?? 0;

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

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            if (Mouse.DirectlyOver is ButtonBorder butt)
            {
                butt.Background = downButt;
                pressedButt = butt;
            }
            else
            if (Mouse.DirectlyOver is Border)
            {
                this.DragMove();
                Defocusser.Focus();
            }
            else
                Defocusser.Focus();
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            BK4819.Interaction();
            Key key = e.Key;
            var fe = FocusManager.GetFocusedElement(this);
            if ((fe is not WatermarkTextBox) && (fe is not TextBox))
            {
                Defocusser.Focus();
                if (key >= Key.NumPad0 && key <= Key.NumPad9) key -= 40;
                if (Instance?.XvfoCol.Width.Value == 0)
                {
                    if (key != lastKey)
                    {
                        if (key >= Key.D0 && key <= Key.D9)
                            command.Execute($"{(int)key - 34}");
                        else
                        {
                            switch (key)
                            {
                                case Key.Back:
                                case Key.Escape:
                                case Key.D:
                                    command.Execute("13");
                                    break;
                                case Key.F:
                                case Key.LeftShift:
                                case Key.RightShift:
                                case Key.Oem7:
                                    command.Execute("15");
                                    break;
                                case Key.A:
                                case Key.Enter:
                                case Key.RightAlt:
                                case Key.LeftAlt:
                                    command.Execute("10");
                                    break;
                                case Key.B:
                                case Key.Up:
                                    command.Execute("11");
                                    break;
                                case Key.C:
                                case Key.Down:
                                    command.Execute("12");
                                    break;
                                case Key.Space:
                                    if (!txLockButtonLocked.Value)
                                    {

                                        command.Execute("16");
                                    }
                                    break;
                                case Key.Multiply:
                                    command.Execute("14");
                                    break;
                                case Key.Tab:
                                    command.Execute("15");
                                    Thread.Sleep(50);
                                    command.Execute("19");
                                    command.Execute("2");
                                    Thread.Sleep(50);
                                    command.Execute("19");
                                    break;
                                case Key.Q:
                                    command.Execute("18");
                                    break;
                                case Key.W:
                                    command.Execute("17");
                                    break;
                            }
                        }
                    }
                }
                else
                {
                    if (key == Key.Escape || key == Key.Space)
                        BK4819.StopScan();
                    if (!scanning.Value)
                    {
                        if (key >= Key.D0 && key <= Key.D9)
                            XVFO.Button(key.ToString()[1..]);
                        else
                        {
                            switch (key)
                            {
                                case Key.V:
                                    XVFO.ToggleVOX();
                                    break;
                                case Key.A:
                                case Key.B:
                                case Key.C:
                                case Key.D:
                                    XVFO.Button(key.ToString());
                                    break;
                                case Key.PageUp:
                                    VFOPreset.StepPreset(1);
                                    break;
                                case Key.PageDown:
                                    VFOPreset.StepPreset(-1);
                                    break;
                                case Key.Up:
                                case Key.Right:
                                    XVFO.Jog(true, true);
                                    break;
                                case Key.Down:
                                case Key.Left:
                                    XVFO.Jog(true, false);
                                    break;
                                case Key.Tab:
                                    VFOPreset.ToggleMainVFO();
                                    break;
                                case Key.Decimal:
                                case Key.Multiply:
                                case Key.OemPeriod:
                                case Key.OemComma:
                                    XVFO.Button("Dot");
                                    break;
                                case Key.Enter:
                                    XVFO.Button("Ret");
                                    break;
                                case Key.Subtract:
                                case Key.Oem7:
                                case Key.Back:
                                    XVFO.Button("Del");
                                    break;
                                case Key.Escape:
                                    XVFO.Button("Abort");
                                    break;
                                case Key.M:
                                    XVFO.ToggleMode();
                                    break;
                                case Key.W:
                                    XVFO.ToggleBandwidth();
                                    break;
                                case Key.Oem4: // [
                                    XVFO.ToggleStep(-1);
                                    break;
                                case Key.Oem6: // ]
                                    XVFO.ToggleStep(1);
                                    break;
                                case Key.Space:
                                    XVFO.Ptt();
                                    break;
                                case Key.S:
                                    XVFO.ToggleAutoSquelch();
                                    break;
                                case Key.Q:
                                    openSquelch.Value = !openSquelch.Value;
                                    break;
                                case Key.P:
                                    XVFO.ToggleTxPower();
                                    break;
                                case Key.F:
                                    XVFO.ToggleMode(0);
                                    XVFO.ToggleBandwidthWN();
                                    break;
                                case Key.G:
                                    XVFO.ToggleMicGain(0);
                                    break;
                                case Key.T:
                                    _ = BK4819.Send1750();
                                    break;
                            }
                        }
                    }
                }
            }
            else
            {
                if(e.Key == Key.Enter && (fe is TextBox tb))
                {
                    Defocusser.Focus();
                    XVFO.Button("Ret");
                    tb.Focus();
                }
                base.OnPreviewKeyDown(e);
            }
            lastKey = key;
        }

        protected override void OnPreviewKeyUp(KeyEventArgs e)
        {
            base.OnPreviewKeyUp(e);
            var fe = FocusManager.GetFocusedElement(this);
            if (fe is not WatermarkTextBox)
            {
                if (Instance?.XvfoCol.Width.Value == 0)
                    command.Execute("19");
                else
                {
                    if (e.Key == Key.Space)
                        XVFO.PttUp();
                }
                lastKey = Key.None;
            }
            else
            if (e.Key == Key.Enter)
                Defocusser.Focus();
            Radio.PulseTX = false;
        }

        private void XVFOFreq_MouseMove(object sender, MouseEventArgs e)
        {
            /*
            14
            19.7
            26.9
            34
            40
            45.5
            51.2
            */
            var p = Mouse.GetPosition(XVFOFreq);
            mouseOverDigit =
                p.X < 14 ? 100 :
                p.X < 19.7 ? 10 :
                p.X < 26.9 ? 1:
                p.X < 34 ? 0.1 :
                p.X < 40 ? 0.01 :
                p.X < 45.5 ? 0.001 :
                p.X < 51.2 ? 0.0001 : 0.00001;
        }

        private void XVFOFreq_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            Defocusser.Focus();
            BK4819.Interaction();
            double freq=rxFreq.Value;
            if (e.Delta > 0) freq += mouseOverDigit;
            if (e.Delta < 0) freq -= mouseOverDigit;
            rxFreq.Value = freq.Clamp(0.01, 1300);
        }

        private void JogWheel_MouseWheel(object sender, MouseWheelEventArgs e)
        {            
            BK4819.Interaction();
            double freq = rxFreq.Value;
            double step = Defines.StepValues[xvfoStep.Value];
            if (e.Delta > 0) freq += step;
            if (e.Delta < 0) freq -= step;
            rxFreq.Value = freq.Clamp(0.01, 1300);
        }

        private void PresetWheel_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            BK4819.Interaction();
            if (e.Delta < 0)
                VFOPreset.StepPreset(-1);
            if (e.Delta > 0)
                VFOPreset.StepPreset(1);
        }

        private void XVFOFreq_KeyDown(object sender, KeyEventArgs e)
        {
            e.Handled= true;
            Defocusser.Focus();
            OnPreviewKeyDown(e);
        }

        private void XVFOFreq_GotFocus(object sender, RoutedEventArgs e)
        {
            Defocusser.Focus();
        }
    }
}
