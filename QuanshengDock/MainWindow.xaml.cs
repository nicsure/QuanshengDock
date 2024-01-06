using QuanshengDock.Data;
using QuanshengDock.General;
using QuanshengDock.UI;
using QuanshengDock.View;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private static readonly Brush upButt = new SolidColorBrush(Color.FromArgb(255, 0x20, 0x20, 0x20));
        private static readonly Brush downButt = new SolidColorBrush(Color.FromArgb(255, 0x10, 0x10, 0x10));
        private static ButtonBorder? pressedButt = null;
        private static MainWindow? Instance = null;
        private readonly VM command;
        private readonly ViewModel<bool> txLockButtonLocked;
        private Key lastKey = Key.None;

        public MainWindow()
        {
            Instance = this;
            DataContext = Context.Instance;
            command = VM.Get("MouseCommand");
            txLockButtonLocked = VM.Get<bool>("TxLockButtonLocked");
            InitializeComponent();
            if (!Radio.DesignMode)
            {
                ToggleSpectrum();
                Width = 400;
            }
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

        public static void ToggleSpectrum()
        {
            if (Instance != null)
            {
                double d = Instance.MainCol.ActualWidth;
                switch (Instance.SpectrumCol.Width.Value)
                {
                    case 0.0:
                        Instance.SpectrumCol.Width = new(1, GridUnitType.Star);
                        Instance.Width = Instance.ActualWidth + d;
                        break;
                    default:
                        Instance.SpectrumCol.Width = new(0.0, GridUnitType.Star);
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
            Key key = e.Key;
            var fe = FocusManager.GetFocusedElement(this);
            if (fe is not WatermarkTextBox)
            {
                Defocusser.Focus();
                if (lastKey != key)
                {
                    lastKey = key;
                    if (key >= Key.NumPad0 && key <= Key.NumPad9) key -= 40;
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
                                    command.Execute("16");
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
                base.OnPreviewKeyDown(e);
        }

        protected override void OnPreviewKeyUp(KeyEventArgs e)
        {
            base.OnPreviewKeyUp(e);
            var fe = FocusManager.GetFocusedElement(this);
            if (fe is not WatermarkTextBox)
            {
                command.Execute("19");
                lastKey = Key.None;
            }
            else
            if (e.Key == Key.Enter) Defocusser.Focus();
        }

    }
}
