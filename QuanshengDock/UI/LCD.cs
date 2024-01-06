using QuanshengDock.General;
using QuanshengDock.View;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace QuanshengDock.UI
{
    // code by -nicsure- 2024
    // https://www.youtube.com/nicsure

    public static class LCD
    {
        public static int Activator { get => 0; set { } }

        private static readonly ViewModel<ColorBrushPen> lcdForeColor = VM.Get<ColorBrushPen>("LCDForeColor");
        private static readonly ViewModel<ColorBrushPen> lcdBackColor = VM.Get<ColorBrushPen>("LCDBackColor");
        private static readonly ViewModel<Typeface> lcdFont = VM.Get<Typeface>("LCDFont");
        private static readonly ViewModel<Typeface> lcdBoldFont = VM.Get<Typeface>("LCDBoldFont");
        private static readonly ViewModel<double> hOffset = VM.Get<double>("HOffset");
        private static readonly ViewModel<double> vOffset = VM.Get<double>("VOffset");
        private static readonly ViewModel<double> hSize = VM.Get<double>("HSize");
        private static readonly ViewModel<double> vSize = VM.Get<double>("VSize");
        private static readonly ViewModel<double> fStretch = VM.Get<double>("FStretch");
        private static readonly ViewModel<RenderTargetBitmap> lcdImage = VM.Get<RenderTargetBitmap>("LcdImage");
        private static RenderTargetBitmap backBuffer;
        private static int updateCount = 0;
        private static bool clear = false;

        static LCD()
        {
            backBuffer = null!;
            Radio.Invoke(() => {
                backBuffer = new(1024, 512, 96, 96, PixelFormats.Pbgra32);
            });
        }

        public static Point Transform(double x, double line)
        {
            double left = x * 8;
            double top = line * 64;
            return new(left, top);
        }

        private static void Swap()
        {
            lcdImage.Value = backBuffer;
            backBuffer = (RenderTargetBitmap)backBuffer.Clone();
        }

        private static void ScreenDraw(Action<DrawingContext> action) // TODO perculiar thread ownership error occasionally on startup
        {
            if (!Radio.Closing)
            {
                Radio.Invoke(() =>
                {
                    DrawingVisual drawingVisual = new();
                    using (DrawingContext drawingContext = drawingVisual.RenderOpen())
                        action(drawingContext);
                    backBuffer.Render(drawingVisual);
                    _ = Update();
                });
            }
        }

        private static async Task Update()
        {
            updateCount++;
            using var task = Task.Delay(10);
            await task;
            updateCount--;
            if (updateCount == 0 && !clear)
                Swap();
        }

        public static void DrawText(double x, double line, double height, string text, bool bold = false, bool stretch = false)
        {
            var point = Transform(x, line);
            point.X += hOffset.Value * 8;
            point.Y += vOffset.Value * 8;
            double em = 64 * height;
            ScreenDraw((drawingContext) =>
            {
                FormattedText ft = new(
                    text,
                    CultureInfo.InvariantCulture,
                    FlowDirection.LeftToRight,
                    bold ? lcdBoldFont.Value : lcdFont.Value,
                    em,
                    lcdForeColor.Value.Brush,
                    96)
                {
                    Trimming = TextTrimming.CharacterEllipsis,
                    TextAlignment = TextAlignment.Left
                };
                Matrix matrix = new()
                {
                    M11 = hSize.Value,
                    M22 = vSize.Value
                };
                if (stretch) matrix.M11 *= fStretch.Value;
                point.X -= ((point.X * matrix.M11) - point.X) / matrix.M11;
                point.Y -= ((point.Y * matrix.M22) - point.Y) / matrix.M22;
                drawingContext.PushTransform(new MatrixTransform(matrix));
                drawingContext.DrawText(ft, point);
                clear = false;
            });
        }

        public static void ClearLines(int from, int to)
        {
            ScreenDraw((drawingContext) => {
                for (int i = from; i <= to; i++)
                {
                    Rect rect = new(0, i * 64, 1024, 64);
                    drawingContext.DrawRectangle(lcdBackColor.Value.Brush, null, rect);
                }
                clear = true;
            });
        }

        public static void DrawSignal(int slevel, int over)
        {
            ScreenDraw((drawingContext) => {
                double x = 0, y = 1, s = 1;
                for (; x < 86; x += 2, y += 0.154, s += 0.28)
                {
                    if (s > slevel + over) break;
                    Rect rect = new((x + 39.5) * 8, (39.5 - y) * 8, 8, y * 8);
                    drawingContext.DrawRectangle(lcdForeColor.Value.Brush, null, rect);
                }
                clear = false;
            });
        }

    }
}
