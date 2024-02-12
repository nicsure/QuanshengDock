using QuanshengDock.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;

namespace QuanshengDock.UI
{
    public class RotaryEncoder : FrameworkElement
    {
        public Brush WheelColor
        {
            get { return (Brush)GetValue(WheelColorProperty); }
            set { SetValue(WheelColorProperty, value); }
        }
        public static readonly DependencyProperty WheelColorProperty =
            DependencyProperty.Register("WheelColor", typeof(Brush), typeof(RotaryEncoder), new PropertyMetadata(new SolidColorBrush(Colors.Beige)));

        public Brush NotchColor
        {
            get { return (Brush)GetValue(NotchColorProperty); }
            set { SetValue(NotchColorProperty, value); }
        }
        public static readonly DependencyProperty NotchColorProperty =
            DependencyProperty.Register("NotchColor", typeof(Brush), typeof(RotaryEncoder), new PropertyMetadata(new SolidColorBrush(Colors.Gray)));

        public Brush WheelBorder
        {
            get { return (Brush)GetValue(WheelBorderProperty); }
            set { SetValue(WheelBorderProperty, value); }
        }
        public static readonly DependencyProperty WheelBorderProperty =
            DependencyProperty.Register("WheelBorder", typeof(Brush), typeof(RotaryEncoder), new PropertyMetadata(new SolidColorBrush(Colors.Wheat), WheelBorderPropertyChanged));

        public double WheelBorderThickness
        {
            get { return (double)GetValue(WheelBorderThicknessProperty); }
            set { SetValue(WheelBorderThicknessProperty, value); }
        }
        public static readonly DependencyProperty WheelBorderThicknessProperty =
            DependencyProperty.Register("WheelBorderThickness", typeof(double), typeof(RotaryEncoder), new PropertyMetadata(1.0, WheelBorderPropertyChanged));

        private static void WheelBorderPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if(d is RotaryEncoder re)
            {
                re.wheelBorder=new(re.WheelBorder, re.WheelBorderThickness);
            }
        }

        public Brush NotchBorder
        {
            get { return (Brush)GetValue(NotchBorderProperty); }
            set { SetValue(NotchBorderProperty, value); }
        }
        public static readonly DependencyProperty NotchBorderProperty =
            DependencyProperty.Register("NotchBorder", typeof(Brush), typeof(RotaryEncoder), new PropertyMetadata(new SolidColorBrush(Colors.Silver), NotchBorderPropertyChanged));

        public double NotchBorderThickness
        {
            get { return (double)GetValue(NotchBorderThicknessProperty); }
            set { SetValue(NotchBorderThicknessProperty, value); }
        }
        public static readonly DependencyProperty NotchBorderThicknessProperty =
            DependencyProperty.Register("NotchBorderThickness", typeof(double), typeof(RotaryEncoder), new PropertyMetadata(1.0, NotchBorderPropertyChanged));

        private static void NotchBorderPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RotaryEncoder re)
            {
                re.notchBorder = new(re.NotchBorder, re.NotchBorderThickness);
            }
        }

        public double Sensitivity
        {
            get { return (double)GetValue(SensitivityProperty); }
            set { SetValue(SensitivityProperty, value); }
        }
        public static readonly DependencyProperty SensitivityProperty =
            DependencyProperty.Register("Sensitivity", typeof(double), typeof(RotaryEncoder), new PropertyMetadata(0.1));



        private static readonly double Tau = Math.PI * 2;
        public event EventHandler? JoggedUp;
        public event EventHandler? JoggedDown;
        private double notchAngle = 0, lastAngle = 0;
        private Pen? wheelBorder = null;// new(new SolidColorBrush(Colors.Wheat), 1.0);
        private Pen? notchBorder = null;// new(new SolidColorBrush(Colors.Silver), 1.0);
       
        private void SaveMouseAngle(bool trigger)
        {
            Point mp = Mouse.GetPosition(this);
            mp.X -= ActualWidth / 2;
            mp.Y -= ActualHeight / 2;
            notchAngle = Math.Atan2(mp.Y, mp.X);
            InvalidateVisual();
            if (trigger)
            {
                double diff = notchAngle - lastAngle;
                while (diff < -5) diff += Tau;
                while (diff > 5) diff -= Tau;
                if (Math.Abs(diff) >= Sensitivity)
                {
                    OnJogged(diff > 0);
                    lastAngle = notchAngle;
                }
            }
            else
                lastAngle = notchAngle;
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            if(e.ChangedButton == MouseButton.Left) 
            {
                PreviewMouseMove += RotaryEncoder_PreviewMouseMove;
                Mouse.Capture(this);
                SaveMouseAngle(false);
            }
        }

        private void RotaryEncoder_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            SaveMouseAngle(true);
        }

        protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                PreviewMouseMove -= RotaryEncoder_PreviewMouseMove;
                Mouse.Capture(null);
            }
        }

        protected override void OnRender(DrawingContext context)
        {
            Point center = new(ActualWidth / 2, ActualHeight / 2);
            double radius = Math.Min(ActualWidth, ActualHeight) / 2;
            context.DrawEllipse(WheelColor, wheelBorder, center, radius, radius);
            radius *= 0.5;
            center.X += Math.Cos(notchAngle) * radius;
            center.Y += Math.Sin(notchAngle) * radius;
            radius *= 0.7;
            context.DrawEllipse(NotchColor, notchBorder, center, radius, radius);
        }
        protected virtual void OnJogged(bool up)
        {
            if(up)
                (_ = JoggedUp)?.Invoke(this, EventArgs.Empty);
            else
                (_ = JoggedDown)?.Invoke(this, EventArgs.Empty);
        }
    }

}
