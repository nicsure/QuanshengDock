using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;

namespace QuanshengDock.General
{
    // code by -nicsure- 2024
    // https://www.youtube.com/nicsure

    public class ColorBrushPen : INotifyPropertyChanged
    {
        private SolidColorBrush brush;
        private Pen pen;
        private Color color;
        private double thickness;

        public event PropertyChangedEventHandler? PropertyChanged;

        public SolidColorBrush Brush => brush;
        public Pen Pen => pen;
        public Color Color
        { 
            get => color;
            set
            {
                Radio.Invoke(() =>
                {
                    color = value;
                    brush = new SolidColorBrush(color);
                    pen = new(brush, thickness) { EndLineCap = PenLineCap.Round };
                    OnPropertyChanged(nameof(Color));
                    OnPropertyChanged(nameof(Brush));
                    OnPropertyChanged(nameof(Pen));
                });
            }
        }
        public double Thickness 
        {
            get => thickness;
            set
            {
                Radio.Invoke(() =>
                {
                    thickness = value;
                    pen = new(brush, thickness) { EndLineCap = PenLineCap.Round};
                    OnPropertyChanged(nameof(Thickness));
                    OnPropertyChanged(nameof(Pen));
                });
            }
        }

        public ColorBrushPen(Color col, double lineThickness)
        {
            color = col;
            brush = new(col);
            pen = new(brush, lineThickness) { EndLineCap = PenLineCap.Round };
            thickness = lineThickness;
        }

        public override string ToString()
        {
            return thickness.ToString() + color.ToString();
        }

        public static ColorBrushPen FromString(string s)
        {
            string[] p = s.Split('#');
            if (p.Length == 2 && double.TryParse(p[0], out double t))
                return new((Color)ColorConverter.ConvertFromString("#" + p[1]), t);
            return new(Colors.Gray, 1);
        }

        protected virtual void OnPropertyChanged(string propertyName) => (_ = PropertyChanged)?.Invoke(this, new(propertyName));
    }
}
