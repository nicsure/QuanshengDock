using NAudio.Dmo;
using QuanshengDock.Data;
using QuanshengDock.General;
using QuanshengDock.Serial;
using QuanshengDock.UI;
using QuanshengDock.View;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Printing.IndexedProperties;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;

namespace QuanshengDock.Analyzer
{
    // code by -nicsure- 2024
    // https://www.youtube.com/nicsure

    public static class SpectrumAnalyzer
    {
        public static int Activator { get => 0; set { } }

        private const double floorScaler = 1000.0;

        private static readonly ViewModel<double> midVM = VM.Get<double>("SpecMid");
        private static readonly ViewModel<double> stepVM = VM.Get<double>("SpecStep");
        private static readonly ViewModel<double> stepsVM = VM.Get<double>("SpecSteps");
        private static readonly ViewModel<bool> normVM = VM.Get<bool>("SpecNorm");
        private static readonly ViewModel<double> ampVM = VM.Get<double>("SpecAmp");
        private static readonly ViewModel<double> floorVM = VM.Get<double>("SpecFloor");
        private static readonly ViewModel<double> waterfallSpeed = VM.Get<double>("WaterfallSpeed");
        private static readonly ViewModel<ColorBrushPen> bgVM = VM.Get<ColorBrushPen>("SpectBGCol");
        private static readonly ViewModel<ColorBrushPen> barVM = VM.Get<ColorBrushPen>("SpectBarCol");
        private static readonly ViewModel<ColorBrushPen> linePen = VM.Get<ColorBrushPen>("SpecLine");
        private static readonly ViewModel<Brush[]> wfPalette = VM.Get<Brush[]>("WaterFallPalette");
        private static readonly ViewModel<string> cursorFreq = VM.Get<string>("CursorFreq");
        private static readonly ViewModel<double> trigger = VM.Get<double>("Trigger");
        private static readonly ViewModel<ColorBrushPen> ledColor = VM.Get<ColorBrushPen>("LEDColor");
        private static readonly ViewModel<double> rxTimeout = VM.Get<double>("RXTimeout");
        private static readonly ViewModel<double> totalTimeout = VM.Get<double>("TotalTimeout");
        private static readonly ViewModel<int> specStyle = VM.Get<int>("SpecStyle");
        private static readonly ViewModel<LinearGradientBrush> heatBG = VM.Get<LinearGradientBrush>("HeatBG");

        private static uint mid = (uint)Math.Round(midVM.Value * 100000.0), step = (uint)Math.Round(stepVM.Value * 100.0);
        private static double amp = ampVM.Value, floor = floorVM.Value * floorScaler;
        private static ushort steps = (ushort)((int)stepsVM.Value | 1);
        private static bool norm = normVM.Value;
        private static readonly RenderTargetBitmap image = VM.Get<RenderTargetBitmap>("SpectrumImage").Value;
        private static readonly List<byte> sorter = new();
        private static uint lastHighFreq = 0, detectedHighFreq = 0;
        private static double detectedHighSignal = 0;
        private static readonly Dictionary<uint, int> blackList = new();
        private static readonly Pen triggerPen = new(new SolidColorBrush(Colors.SkyBlue), 1);
        private static readonly Brush fadeBrush = new SolidColorBrush(Color.FromArgb(127, 0, 0, 0));
        private static readonly Rect clearRect = new(0, 0, 1024, 512);

        static SpectrumAnalyzer()
        {
            midVM.PropertyChanged += Params_PropertyChanged;
            stepVM.PropertyChanged += Params_PropertyChanged;
            stepsVM.PropertyChanged += Params_PropertyChanged;
            normVM.PropertyChanged += Params_PropertyChanged;
            ampVM.PropertyChanged += Params_PropertyChanged;
            floorVM.PropertyChanged += Params_PropertyChanged;
        }

        private static void Params_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {            
            bool changed = mid != (mid = (uint)Math.Round(midVM.Value * 100000.0));
            changed |= step != (step = (uint)Math.Round(stepVM.Value * 100.0));
            changed |= steps != (steps = (ushort)((int)stepsVM.Value | 1));
            norm = normVM.Value;
            amp = ampVM.Value;
            floor = floorVM.Value * floorScaler;
            if (Radio.AnalyzerMode && changed)
                ScanCommand(true);
        }

        public static void Start()
        {
            image.Clear();
            if(!Radio.AnalyzerMode)
            {
                Resume();
                Radio.AnalyzerMode = true;
            }
        }

        public static void BlacklistMonitored()
        {
            if(Radio.Monitoring)
                Radio.MonitoredFreq.Blacklist(2);
        }

        public static void BlacklistCursor()
        {
            if(double.TryParse(MenuActions.SelectedFreq, out double value)) 
            {
                uint freq = (uint)Math.Round(value * 100000.0);
                freq.Blacklist(2);
            }
        }

        public static void ClearBlacklist()
        {
            blackList.Clear();
        }

        private static void Resume()
        {
            ScanCommand(false);
            Radio.Monitoring = false;
        }

        private static void Render(double[] sigs)
        {
            double trig = trigger.Value;
            if (!Radio.Closing)
            {
                Radio.Invoke(() =>
                {
                    Brush barBrush = barVM.Value.Brush;
                    Brush bgBrush = bgVM.Value.Brush;
                    Brush heat = heatBG.Value;
                    Pen line = linePen.Value.Pen;
                    int style = specStyle.Value;
                    double speed = waterfallSpeed.Value;
                    double w = 1024.0 / sigs.Length;
                    double wc = Math.Ceiling(w);
                    double w2 = w / 2.0;
                    double gap = w / 4.0;
                    double lastiy = -1;
                    DrawingVisual drawingVisual = new();
                    double hiSig = -1;
                    uint hiFreq = 0;
                    uint freq = mid - (uint)(step * (steps / 2));
                    List<Rect> ignored = new();
                    using (DrawingContext drawingContext = drawingVisual.RenderOpen())
                    {
                        if (Radio.WaterfallMode)
                            drawingContext.DrawImage(image, new(0, speed, 1024, 512));
                        else
                        {
                            switch (style)
                            {
                                case 0: // bar
                                case 1: drawingContext.DrawRectangle(bgBrush, null, clearRect); break; // line
                                case 2: drawingContext.DrawRectangle(heat, null, clearRect); break; // heat
                            }
                        }
                        for (int i = 0; i < sigs.Length; i++)
                        {
                            double x = Math.Floor(w * i);
                            double y = sigs[i].Clamp(0, 511);
                            switch(freq.Blacklist())
                            {
                                case 0:
                                    if (sigs[i] > hiSig)
                                    {
                                        hiFreq = freq;
                                        hiSig = sigs[i];
                                    }
                                    break;
                                case 1:
                                    if (sigs[i] < trig)
                                    {
                                        freq.Blacklist(0);
                                    }
                                    break;
                            }
                            if (Radio.SpectrumMode)
                            {
                                Rect col = new(x, 0, wc, 511);
                                if (freq.Blacklist() > 1)
                                    ignored.Add(col);
                                double iy = 511 - y;
                                switch (style)
                                {
                                    case 1: // line
                                        if (lastiy > -1)
                                            drawingContext.DrawLine(line, new(x - w2, lastiy), new(x + w2, iy));
                                        lastiy = iy;
                                        break;
                                    case 0: // bar
                                        Rect bar = new(x + gap, iy, wc - gap, 512 - iy);
                                        drawingContext.DrawRectangle(barBrush, null, bar);
                                        break;
                                    case 2: // heat
                                        Rect top = new(x, 0, wc, iy);
                                        drawingContext.DrawRectangle(bgBrush, null, top);
                                        break;
                                }
                            }
                            else
                            {
                                Brush col = wfPalette.Value[(int)y];
                                Rect rect = new(x, 0, w, speed);
                                drawingContext.DrawRectangle(col, null, rect);
                            }
                            freq += step;
                        }
                        foreach(var iRect in ignored)
                            drawingContext.DrawRectangle(fadeBrush, null, iRect);
                        if (trig > 0 && Radio.SpectrumMode)
                            drawingContext.DrawLine(triggerPen, new(0, 511-trig), new(1024, 511-trig));
                    }
                    image.Render(drawingVisual);
                    if (lastHighFreq == hiFreq)
                    {
                        detectedHighFreq = hiFreq;
                        detectedHighSignal = hiSig;
                    }
                    lastHighFreq = hiFreq;
                    Status(detectedHighFreq / 100000.0);
                });
                if (trig > 0 && detectedHighSignal >= trig)
                {
                    MonitorFreq(detectedHighFreq);
                    detectedHighFreq = 0;
                    detectedHighSignal = 0;
                }
            }            
        }

        private static void MonitorFreq(uint freq)
        {
            freq.Blacklist(1);
            Radio.MonitoredFreq = freq;
            Radio.Monitoring = true;
            Comms.SendCommand(Packet.ScanAdjust, freq, (uint)0, (ushort)0);
            _ = MonitorTimer();
        }

        private static async Task MonitorTimer()
        {
            double total = 0, norx = 0;
            while (Radio.AnalyzerMode && Radio.MonitoredFreq.Blacklist() < 2)
            {
                await Task.Delay(500);
                total += 0.5;
                norx = Radio.State == RState.RX ? 0 : norx + 0.5;
                if (norx >= rxTimeout.Value || total >= totalTimeout.Value)
                    break;
            }
            if (Radio.AnalyzerMode)
                Resume();
        }

        private static int Blacklist(this uint freq, int newstatus = -1)
        {
            int status = blackList.TryGetValue(freq, out int s) ? s : 0;
            if (newstatus > -1)
            {
                if (newstatus == 0)
                    blackList.Remove(freq);
                else
                    blackList[freq] = newstatus;
            }
            return status;
        }

        private static void Status(double hi)
        {
            ledColor.Value.Color = Radio.SpectrumMode ? Colors.Purple : Colors.DarkOrange;
            LCD.ClearLines(0, 7);
            LCD.DrawText(8, 1, 1, Radio.SpectrumMode ? "Spectrum Analyzer" : "Waterfall", true);
            int spread = (steps / 2) * (int)step;
            double start = (mid - spread) / 100000.0;
            double end = (mid + spread) / 100000.0;
            LCD.DrawText(8, 2, 1, $"{start:F5} to {end:F5}");
            if (hi > 0)
                LCD.DrawText(8, 3, 0.75, $"High Signal: {hi:F5}");
            if (trigger.Value > 0)
            {
                LCD.DrawText(8, 4, 1, "Monitor Active");
                LCD.DrawText(8, 5, 0.5, $"Signal Timeout {rxTimeout.Value:F1} secs");
                LCD.DrawText(8, 5.5, 0.5, $" Total Timeout {totalTimeout.Value:F1} secs");
            }
            if (cursorFreq.Value.Length > 0)
                LCD.DrawText(8, 6.25, 0.75, $"Cursor: {cursorFreq.Value}");
        }

        public static void Data(byte[] data)
        {
            if(Radio.AnalyzerMode && !Radio.Monitoring)
            {
                byte sync = data[5];
                byte len = data[4];
                if (sync == 0 && sorter.Count > 0)
                {
                    byte[] b = sorter.ToArray();
                    if(norm) sorter.Sort();
                    double median = norm ? sorter[sorter.Count / 3] : 0;
                    sorter.Clear();
                    double[] sigs = new double[b.Length];
                    for (int i = 0; i < b.Length; i++)
                    {
                        sigs[i] = ((b[i] - median) * amp) + floor;
                    }
                    Render(sigs);
                }
                int start = 6;
                int end = start + len;
                while (start < end)
                {
                    sorter.Add(data[start++]);
                }
            }
        }

        private static void ScanCommand(bool adjust)
        {
            Comms.SendCommand(adjust ? Packet.ScanAdjust : Packet.Scan, mid, step, steps);
        }

    }
}
