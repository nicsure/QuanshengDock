using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using QuanshengDock.Serial;
using QuanshengDock.UI;
using QuanshengDock.View;
using System.Windows;
using QuanshengDock.General;
using QuanshengDock.Audio;
using QuanshengDock.Analyzer;
using System.Collections.ObjectModel;
using QuanshengDock.Channels;

namespace QuanshengDock.Data
{
    public class ContextTest
    {
        public static ContextTest Instance => instance;
        private static readonly ContextTest instance = new();
    }



    public class Context
    {
        public static Context Instance => instance;
        private static readonly Context instance = new();
        private SavedDictionary? fontAdj = null;


        public ViewModel<RenderTargetBitmap> LcdImage { get; } = new(new(1024, 512, 96, 96, PixelFormats.Pbgra32), nameof(LcdImage));
        public ViewModel<RenderTargetBitmap> SpectrumImage { get; } = new(new(1024, 512, 96, 96, PixelFormats.Pbgra32), nameof(SpectrumImage));
        public ViewModel<object> MouseCommand { get; } = new(nameof(MouseCommand));
        public ViewModel<ObservableCollection<Preset>> MenuCommand { get; } = new(new(), nameof(MenuCommand));
        public ViewModel<string> TxLockButtonText { get; } = new(nameof(TxLockButtonText));
        public ViewModel<bool> TxLockButtonLocked { get; } = new(true, nameof(TxLockButtonLocked));
        public ViewModel<string> ComPort { get; } = new(string.Empty, nameof(ComPort), true);
        public ViewModel<double> HOffset { get; } = new(0.0, name: nameof(HOffset));
        public ViewModel<double> VOffset { get; } = new(0.0, name: nameof(VOffset));
        public ViewModel<double> HSize { get; } = new(1.0, nameof(HSize));
        public ViewModel<double> VSize { get; } = new(1.0, nameof(VSize));
        public ViewModel<double> FStretch { get; } = new(1.17, nameof(FStretch));
        public ViewModel<double> Volume { get; } = new(0.75, nameof(Volume), true);
        public ViewModel<Typeface> LCDFont { get; } = new(nameof(LCDFont));
        public ViewModel<Typeface> LCDBoldFont { get; } = new(nameof(LCDBoldFont));
        public ViewModel<string> LCDFontName { get; } = new("Consolas", nameof(LCDFontName), true);
        public ViewModel<Brush> LCDBackBrush { get; } = new(new SolidColorBrush(Colors.Black), nameof(LCDBackBrush));
        public ViewModel<Brush> LCDForeBrush { get; } = new(new SolidColorBrush(Colors.LimeGreen), nameof(LCDForeBrush));
        public ViewModel<Color> LCDBackColor { get; } = new(Colors.Black, nameof(LCDBackBrush), true);
        public ViewModel<Color> LCDForeColor { get; } = new(Colors.LimeGreen, nameof(LCDForeColor), true);
        public ViewModel<Brush> LEDBrush { get; } = new(nameof(LEDBrush));
        public ViewModel<Color> LEDColor { get; } = new(Colors.Black, nameof(LEDColor));
        public ViewModel<string> AudioInDevice { get; } = new(string.Empty, nameof(AudioInDevice), true);
        public ViewModel<string> AudioOutDevice { get; } = new(string.Empty, nameof(AudioOutDevice), true);
        public ViewModel<double> SpecMid { get; } = new(144.0, nameof(SpecMid), true);
        public ViewModel<double> SpecStep { get; } = new(25.0, nameof(SpecStep), true);
        public ViewModel<double> SpecSteps { get; } = new(25.0, nameof(SpecSteps), true);
        public ViewModel<bool> SpecNorm { get; } = new(true, nameof(SpecNorm), true);
        public ViewModel<double> SpecAmp { get; } = new(1.0, nameof(SpecAmp), true);
        public ViewModel<double> SpecFloor { get; } = new(0.0, nameof(SpecFloor), true);
        public ViewModel<Brush> SpectBG { get; } = new(new SolidColorBrush(Colors.Black), nameof(SpectBG));
        public ViewModel<Brush> SpectBar { get; } = new(new SolidColorBrush(Colors.LimeGreen), nameof(SpectBar));
        public ViewModel<Color> SpectBGCol { get; } = new(Colors.Black, nameof(SpectBGCol), true);
        public ViewModel<Color> SpectBarCol { get; } = new(Colors.LimeGreen, nameof(SpectBarCol), true);
        public ViewModel<double> CursorX { get; } = new(0.0, nameof(CursorX));
        public ViewModel<double> CursorY { get; } = new(0.0, nameof(CursorY));
        public ViewModel<double> Trigger { get; } = new(0.0, nameof(Trigger));
        public ViewModel<string> CursorFreq { get; } = new(string.Empty, nameof(CursorFreq));
        public ViewModel<double> RXTimeout { get; } = new(2.0, nameof(RXTimeout));
        public ViewModel<double> TotalTimeout { get; } = new(999.0, nameof(TotalTimeout));
        public ViewModel<string> PresetName { get; } = new(string.Empty, nameof(PresetName));
        public ViewModel<Color> WaterfallCol1 { get; } = new(Colors.DarkBlue, nameof(WaterfallCol1), true);
        public ViewModel<Color> WaterfallCol2 { get; } = new(Colors.Orange, nameof(WaterfallCol2), true);
        public ViewModel<Brush[]> WaterFallPalette { get; } = new(nameof(WaterFallPalette));
        public ViewModel<Color> SpecLineCol { get; } = new(Colors.White, nameof(SpecLineCol), true);
        public ViewModel<double> SpecLineThickness { get; } = new(2.0, nameof(SpecLineThickness), true);
        public ViewModel<Pen> SpecLinePen { get; } = new(new(new SolidColorBrush(Colors.White), 2), nameof(SpecLinePen));
        public ViewModel<int> SpecStyle { get; } = new(0, nameof(SpecStyle), true);
        public ViewModel<ObservableCollection<GridChannel>> Channels { get; } = new(nameof(Channels));
        public ViewModel<ObservableCollection<GridChannel>> AllChannels { get; } = new(new(), nameof(AllChannels));
        public ViewModel<ObservableCollection<GridChannel>> UsedChannels { get; } = new(new(), nameof(UsedChannels));
        public ViewModel<string> ChEditMessage { get; } = new(string.Empty, nameof(ChEditMessage));
        public ViewModel<bool> EnableChanButtons { get; } = new(true, nameof(EnableChanButtons));
        public ViewModel<double> ChanButtonsOpacity { get; } = new(nameof(ChanButtonsOpacity));
        public ViewModel<bool> ShowAll { get; } = new(true, nameof(ShowAll));
        public ViewModel<Brush> ShowAllBrush { get; } = new(nameof(ShowAllBrush));

        public static string[] ComPorts => SerialPort.GetPortNames();
        public static string[] AudioInDevices => AudioDevices.GetAudioInDevices();
        public static string[] AudioOutDevices => AudioDevices.GetAudioOutDevices();

        public Context()
        {
            LCDBackColor.PropertyChanged += LCDBackColor_PropertyChanged;
            LCDForeColor.PropertyChanged += LCDForeColor_PropertyChanged;
            LCDFontName.PropertyChanged += LCDFontName_PropertyChanged;
            HOffset.PropertyChanged += FontAdj_PropertyChanged;
            VOffset.PropertyChanged += FontAdj_PropertyChanged;
            HSize.PropertyChanged += FontAdj_PropertyChanged;
            VSize.PropertyChanged += FontAdj_PropertyChanged;
            FStretch.PropertyChanged += FontAdj_PropertyChanged;
            AudioInDevice.PropertyChanged += AudioDevice_PropertyChanged;
            AudioOutDevice.PropertyChanged += AudioDevice_PropertyChanged;
            SpectBGCol.PropertyChanged += SpectBGCol_PropertyChanged;
            SpectBarCol.PropertyChanged += SpectBarCol_PropertyChanged;
            WaterfallCol1.PropertyChanged += WaterfallColour_PropertyChanged;
            WaterfallCol2.PropertyChanged += WaterfallColour_PropertyChanged;
            SpecLineCol.PropertyChanged += SpecLine_PropertyChanged;
            SpecLineThickness.PropertyChanged += SpecLine_PropertyChanged;
            LCDFontName.ForceUpdate++;
            LCDBackColor.ForceUpdate++;
            LCDForeColor.ForceUpdate++;
            SpectBGCol.ForceUpdate++;
            SpectBarCol.ForceUpdate++;
            SpecMid.ForceUpdate++;
            SpecStep.ForceUpdate++;
            SpecSteps.ForceUpdate++;
            WaterfallCol1.ForceUpdate++;
            SpecLineCol.ForceUpdate++;
            ShowAllBrush.SetConverter(() => new SolidColorBrush(ShowAll.Value ? Colors.LimeGreen : Colors.DarkSlateGray), ShowAll);
            ChanButtonsOpacity.SetConverter(() => EnableChanButtons.Value ? 1.0 : 0.3, EnableChanButtons);
            LEDBrush.SetConverter(() => new SolidColorBrush(LEDColor.Value), LEDColor);
            TxLockButtonText.SetConverter(() => $"TX {(TxLockButtonLocked.Value ? "🔒" : "🔓")}", TxLockButtonLocked);
            if (ComPort.Value.Length == 0 && ComPorts.Length > 0)
                ComPort.Value = ComPorts[0];
            if(AudioInDevice.Value.Length == 0 && AudioInDevices.Length > 0)
                AudioInDevice.Value = AudioInDevices[0];
            if (AudioOutDevice.Value.Length == 0 && AudioOutDevices.Length > 0)
                AudioOutDevice.Value = AudioOutDevices[0];
            MouseActions.Activator++;
            MenuActions.Activator++;
            Comms.Activator++;
            Sound.Activator++;
            SpectrumAnalyzer.Activator++;
            Preset.Activator++;
            Channel.Activator++;
            AudioInDevice.ForceUpdate++;
        }

        private void SpecLine_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            SpecLinePen.Value = new(new SolidColorBrush(SpecLineCol.Value), SpecLineThickness.Value)
            {
                EndLineCap = PenLineCap.Round
            };
        }

        private void WaterfallColour_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var palette = WaterFallPalette.Value = new Brush[512];
            double dr = (WaterfallCol2.Value.R - (double)WaterfallCol1.Value.R) / 512.0;
            double dg = (WaterfallCol2.Value.G - (double)WaterfallCol1.Value.G) / 512.0;
            double db = (WaterfallCol2.Value.B - (double)WaterfallCol1.Value.B) / 512.0;
            for (int i = 0; i < 512; i++)
            {
                byte r = (byte)(WaterfallCol1.Value.R + (dr * i)).Clamp(0, 255);
                byte g = (byte)(WaterfallCol1.Value.G + (dg * i)).Clamp(0, 255);
                byte b = (byte)(WaterfallCol1.Value.B + (db * i)).Clamp(0, 255);
                palette[i] = new SolidColorBrush(Color.FromArgb(255, r, g, b));
            }
        }

        private void SpectBarCol_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            SpectBar.Value = new SolidColorBrush(SpectBarCol.Value);
        }

        private void SpectBGCol_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            SpectBG.Value = new SolidColorBrush(SpectBGCol.Value);
        }

        private void AudioDevice_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            int inId = Array.IndexOf(AudioInDevices, AudioInDevice.Value);
            int outId = Array.IndexOf(AudioOutDevices, AudioOutDevice.Value);
            if (inId != -1 && outId != -1)
                Sound.Start(inId, outId);
        }

        private void FontAdj_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (fontAdj != null)
            {
                fontAdj[nameof(HOffset)] = HOffset.Value.ToString();
                fontAdj[nameof(VOffset)] = VOffset.Value.ToString();
                fontAdj[nameof(HSize)] = HSize.Value.ToString();
                fontAdj[nameof(VSize)] = VSize.Value.ToString();
                fontAdj[nameof(FStretch)] = FStretch.Value.ToString();
                fontAdj.Save();
            }
        }

        private void LCDFontName_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            LCDFont.Value = new(LCDFontName.Value);
            LCDBoldFont.Value = new(LCDFont.Value.FontFamily, FontStyles.Normal, FontWeights.Black, FontStretches.Normal);
            fontAdj = new($"{LCDFontName.Value}.font");
            FStretch.Value = fontAdj.GetValue(nameof(FStretch), "1.17").ToDouble();
            VSize.Value = fontAdj.GetValue(nameof(VSize), "1.0").ToDouble();
            HSize.Value = fontAdj.GetValue(nameof(HSize), "1.0").ToDouble();
            VOffset.Value = fontAdj.GetValue(nameof(VOffset), "0.0").ToDouble();
            HOffset.Value = fontAdj.GetValue(nameof(HOffset), "0.0").ToDouble();
        }

        private void LCDForeColor_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            LCDForeBrush.Value = new SolidColorBrush(LCDForeColor.Value);
        }

        private void LCDBackColor_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            LCDBackBrush.Value = new SolidColorBrush(LCDBackColor.Value);
        }
    }
}
