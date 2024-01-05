using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace QuanshengDock.UI
{
    public class Indicator : Border
    {
        private Frame innerFrame = new();

        public Indicator()
        {            
            Child = innerFrame;
        }

        public Brush OnBrush
        {
            get { return (Brush)GetValue(OnBrushProperty); }
            set { SetValue(OnBrushProperty, value); }
        }
        public static readonly DependencyProperty OnBrushProperty =
            DependencyProperty.Register("OnBrush", typeof(Brush), typeof(Indicator), new PropertyMetadata(new SolidColorBrush(Colors.Red)));

        public Brush OffBrush
        {
            get { return (Brush)GetValue(OffBrushProperty); }
            set { SetValue(OffBrushProperty, value); }
        }
        public static readonly DependencyProperty OffBrushProperty =
            DependencyProperty.Register("OffBrush", typeof(Brush), typeof(Indicator), new PropertyMetadata(new SolidColorBrush(Colors.Black)));


        public bool IsOn
        {
            get { return (bool)GetValue(IsOnProperty); }
            set { SetValue(IsOnProperty, value); }
        }
        public static readonly DependencyProperty IsOnProperty =
            DependencyProperty.Register("IsOn", typeof(bool), typeof(Indicator), new PropertyMetadata(IsOnChanged));

        private static void IsOnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if(d is Indicator indicator)
            {
                indicator.innerFrame.Background = (bool)e.NewValue ? indicator.OnBrush : indicator.OffBrush;
            }
        }




    }
}
