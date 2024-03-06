using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows;
using System.Diagnostics;

namespace QuanshengDock.UI
{
    public class Potentiometer : FrameworkElement
    {
        public Brush WheelColor
        {
            get { return (Brush)GetValue(WheelColorProperty); }
            set { SetValue(WheelColorProperty, value); }
        }
        public static readonly DependencyProperty WheelColorProperty =
            DependencyProperty.Register("WheelColor", typeof(Brush), typeof(Potentiometer), new PropertyMetadata(new SolidColorBrush(Colors.Beige)));

        public Brush NotchColor
        {
            get { return (Brush)GetValue(NotchColorProperty); }
            set { SetValue(NotchColorProperty, value); }
        }
        public static readonly DependencyProperty NotchColorProperty =
            DependencyProperty.Register("NotchColor", typeof(Brush), typeof(Potentiometer), new PropertyMetadata(new SolidColorBrush(Colors.Gray)));

        public Brush WheelBorder
        {
            get { return (Brush)GetValue(WheelBorderProperty); }
            set { SetValue(WheelBorderProperty, value); }
        }
        public static readonly DependencyProperty WheelBorderProperty =
            DependencyProperty.Register("WheelBorder", typeof(Brush), typeof(Potentiometer), new PropertyMetadata(new SolidColorBrush(Colors.Wheat), WheelBorderPropertyChanged));

        public double WheelBorderThickness
        {
            get { return (double)GetValue(WheelBorderThicknessProperty); }
            set { SetValue(WheelBorderThicknessProperty, value); }
        }
        public static readonly DependencyProperty WheelBorderThicknessProperty =
            DependencyProperty.Register("WheelBorderThickness", typeof(double), typeof(Potentiometer), new PropertyMetadata(1.0, WheelBorderPropertyChanged));

        private static void WheelBorderPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Potentiometer p)
            {
                p.wheelBorder = new(p.WheelBorder, p.WheelBorderThickness);
            }
        }

        public Brush NotchBorder
        {
            get { return (Brush)GetValue(NotchBorderProperty); }
            set { SetValue(NotchBorderProperty, value); }
        }
        public static readonly DependencyProperty NotchBorderProperty =
            DependencyProperty.Register("NotchBorder", typeof(Brush), typeof(Potentiometer), new PropertyMetadata(new SolidColorBrush(Colors.Silver), NotchBorderPropertyChanged));

        public double NotchBorderThickness
        {
            get { return (double)GetValue(NotchBorderThicknessProperty); }
            set { SetValue(NotchBorderThicknessProperty, value); }
        }
        public static readonly DependencyProperty NotchBorderThicknessProperty =
            DependencyProperty.Register("NotchBorderThickness", typeof(double), typeof(Potentiometer), new PropertyMetadata(1.0, NotchBorderPropertyChanged));

        private static void NotchBorderPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Potentiometer p)
            {
                p.notchBorder = new(p.NotchBorder, p.NotchBorderThickness);
            }
        }

        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(double), typeof(Potentiometer), new PropertyMetadata(0.0, ValueChanged));

        private static void ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Potentiometer p)
            {
                p.UpdateNotch();
            }
        }

        public double Minimum
        {
            get { return (double)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }
        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register("Minimum", typeof(double), typeof(Potentiometer), new PropertyMetadata(0.0));

        public double Maximum
        {
            get { return (double)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }
        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register("Maximum", typeof(double), typeof(Potentiometer), new PropertyMetadata(10.0));


        private static readonly double Tau = Math.PI * 2.0, Down = Math.PI / 2.0, Indent = 0.6;
        private static readonly double Min = Down + Indent, Max = Down - Indent, Full = Tau - (Indent * 2);
        private double notchAngle = Min;
        private Pen? wheelBorder = null;
        private Pen? notchBorder = null;
        private bool dontUpdateNotch = false;

        private void ProcessMouseAngle()
        {
            Point mp = Mouse.GetPosition(this);
            mp.X -= ActualWidth / 2;
            mp.Y -= ActualHeight / 2;
            notchAngle = Math.Atan2(mp.Y, mp.X);
            while (notchAngle < 0) notchAngle += Tau;
            while (notchAngle > Tau) notchAngle -= Tau;
            if (notchAngle > Max && notchAngle < Min)
                notchAngle = notchAngle > Down ? Min : Max;
            //Debug.WriteLine($"Down:{Down} Min:{Min} Max:{Max} NA:{notchAngle}");
            InvalidateVisual();
            double aa = (notchAngle < Down ? notchAngle + Tau : notchAngle) - Min;
            aa /= Full;
            aa *= Maximum - Minimum;
            aa += Minimum;
            //Debug.WriteLine($"AA:{aa}");
            dontUpdateNotch = true;
            Value = aa.Clamp(Minimum, Maximum);
            dontUpdateNotch = false;
        }

        private void UpdateNotch()
        {
            if (!dontUpdateNotch && Mouse.Captured != this)
            {
                notchAngle = Value - Minimum;
                notchAngle /= Maximum - Minimum;
                notchAngle *= Full;
                notchAngle += Min;
                InvalidateVisual();
            }
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                PreviewMouseMove += Potentiometer_PreviewMouseMove;
                Mouse.Capture(this);
                ProcessMouseAngle();
            }
        }

        private void Potentiometer_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            ProcessMouseAngle();
        }

        protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                PreviewMouseMove -= Potentiometer_PreviewMouseMove;
                Mouse.Capture(null);
            }
        }

        protected override void OnRender(DrawingContext context)
        {
            Point center = new(ActualWidth / 2, ActualHeight / 2);
            double radius = Math.Min(ActualWidth, ActualHeight) / 2;
            context.DrawEllipse(WheelColor, wheelBorder, center, radius, radius);
            radius *= 0.65;
            center.X += Math.Cos(notchAngle) * radius;
            center.Y += Math.Sin(notchAngle) * radius;
            radius *= 0.35;
            context.DrawEllipse(NotchColor, notchBorder, center, radius, radius);
        }

    }
}
