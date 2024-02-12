using QuanshengDock.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace QuanshengDock.UI
{
    public class SMeter : FrameworkElement
    {

        protected override void OnRender(DrawingContext context)
        {
            context.DrawRectangle(MeterColor, null, new(0, 0, ActualWidth, ActualHeight));
            double w = ActualWidth / 100;
            double wc = ActualWidth / 90;
            double h = ActualWidth * 0.7;
            for (int i = 0; i < 100; i++)
            {
                if ((i & 1) != 0 || i > Value)
                {
                    Rect r = new(i * w, 0, wc, ActualHeight);
                    context.DrawRectangle(MaskColor, null, r);
                }
                else
                {
                    Rect r = new(i * w, 0, wc, h);
                    h -= w * 1.5;
                    if (h < 0) h = 0;
                    context.DrawRectangle(MaskColor, null, r);
                }
            }
        }

        public Brush MeterColor
        {
            get { return (Brush)GetValue(MeterColorProperty); }
            set { SetValue(MeterColorProperty, value); }
        }
        public static readonly DependencyProperty MeterColorProperty =
            DependencyProperty.Register("MeterColor", typeof(Brush), typeof(SMeter), new PropertyMetadata(new SolidColorBrush(Colors.LimeGreen)));

        public Brush MaskColor
        {
            get { return (Brush)GetValue(MaskColorProperty); }
            set { SetValue(MaskColorProperty, value); }
        }
        public static readonly DependencyProperty MaskColorProperty =
            DependencyProperty.Register("MaskColor", typeof(Brush), typeof(SMeter), new PropertyMetadata(new SolidColorBrush(Colors.Black)));

        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(double), typeof(SMeter), new PropertyMetadata(0.0, ValueChanged));

        private static void ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SMeter s)
                s.InvalidateVisual();
        }
    }
}
