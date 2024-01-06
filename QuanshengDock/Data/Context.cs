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
using QuanshengDock.User;

namespace QuanshengDock.Data
{
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
        public ViewModel<ColorBrushPen> LCDBackColor { get; } = new(new(Colors.Black, 0), nameof(LCDBackColor), true);
        public ViewModel<ColorBrushPen> LCDForeColor { get; } = new(new(Colors.LimeGreen, 0), nameof(LCDForeColor), true);
        public ViewModel<ColorBrushPen> LEDColor { get; } = new(new(Colors.Black, 0), nameof(LEDColor));
        public ViewModel<string> AudioInDevice { get; } = new(string.Empty, nameof(AudioInDevice), true);
        public ViewModel<string> AudioOutDevice { get; } = new(string.Empty, nameof(AudioOutDevice), true);
        public ViewModel<bool> Passthrough { get; } = new(true, nameof(Passthrough), true);
        public ViewModel<double> SpecMid { get; } = new(144.0, nameof(SpecMid), true);
        public ViewModel<double> SpecStep { get; } = new(25.0, nameof(SpecStep), true);
        public ViewModel<double> SpecSteps { get; } = new(25.0, nameof(SpecSteps), true);
        public ViewModel<bool> SpecNorm { get; } = new(true, nameof(SpecNorm), true);
        public ViewModel<double> SpecAmp { get; } = new(1.0, nameof(SpecAmp), true);
        public ViewModel<double> SpecFloor { get; } = new(0.0, nameof(SpecFloor), true);
        public ViewModel<ColorBrushPen> SpectBGCol { get; } = new(new(Colors.Black, 0), nameof(SpectBGCol), true);
        public ViewModel<ColorBrushPen> SpectBarCol { get; } = new(new(Colors.LimeGreen, 0), nameof(SpectBarCol), true);
        public ViewModel<double> CursorX { get; } = new(0.0, nameof(CursorX));
        public ViewModel<double> CursorY { get; } = new(0.0, nameof(CursorY));
        public ViewModel<double> Trigger { get; } = new(0.0, nameof(Trigger));
        public ViewModel<string> CursorFreq { get; } = new(string.Empty, nameof(CursorFreq));
        public ViewModel<double> RXTimeout { get; } = new(2.0, nameof(RXTimeout), true);
        public ViewModel<double> TotalTimeout { get; } = new(999.0, nameof(TotalTimeout), true);
        public ViewModel<string> PresetName { get; } = new(string.Empty, nameof(PresetName));
        public ViewModel<Color> WaterfallCol1 { get; } = new(Colors.DarkBlue, nameof(WaterfallCol1), true);
        public ViewModel<Color> WaterfallCol2 { get; } = new(Colors.Orange, nameof(WaterfallCol2), true);
        public ViewModel<Brush[]> WaterFallPalette { get; } = new(nameof(WaterFallPalette));
        public ViewModel<ColorBrushPen> SpecLine { get; } = new(new(Colors.White, 5.0), nameof(SpecLine), true);
        public ViewModel<int> SpecStyle { get; } = new(0, nameof(SpecStyle), true);
        public ViewModel<ObservableCollection<GridChannel>> Channels { get; } = new(nameof(Channels));
        public ViewModel<ObservableCollection<GridChannel>> AllChannels { get; } = new(new(), nameof(AllChannels));
        public ViewModel<ObservableCollection<GridChannel>> UsedChannels { get; } = new(new(), nameof(UsedChannels));
        public ViewModel<string> ChEditMessage { get; } = new(string.Empty, nameof(ChEditMessage));
        public ViewModel<bool> EnableChanButtons { get; } = new(true, nameof(EnableChanButtons));
        public ViewModel<double> ChanButtonsOpacity { get; } = new(nameof(ChanButtonsOpacity));
        public ViewModel<bool> ShowAll { get; } = new(true, nameof(ShowAll));
        public ViewModel<Brush> ShowAllBrush { get; } = new(nameof(ShowAllBrush));
        public ViewModel<double> WaterfallSpeed { get; } = new(2.0, nameof(WaterfallSpeed), true);
        public ViewModel<LinearGradientBrush> HeatBG { get; } = new(nameof(HeatBG));

        public static string[] ComPorts => SerialPort.GetPortNames();
        public static string[] AudioInDevices => AudioDevices.GetAudioInDevices();
        public static string[] AudioOutDevices => AudioDevices.GetAudioOutDevices();

        public Context()
        {
            LCDFontName.PropertyChanged += LCDFontName_PropertyChanged;
            HOffset.PropertyChanged += FontAdj_PropertyChanged;
            VOffset.PropertyChanged += FontAdj_PropertyChanged;
            HSize.PropertyChanged += FontAdj_PropertyChanged;
            VSize.PropertyChanged += FontAdj_PropertyChanged;
            FStretch.PropertyChanged += FontAdj_PropertyChanged;
            AudioInDevice.PropertyChanged += AudioDevice_PropertyChanged;
            AudioOutDevice.PropertyChanged += AudioDevice_PropertyChanged;
            Passthrough.PropertyChanged += AudioDevice_PropertyChanged;
            WaterfallCol1.PropertyChanged += WaterfallColour_PropertyChanged;
            WaterfallCol2.PropertyChanged += WaterfallColour_PropertyChanged;
            LCDFontName.ForceUpdate++;
            SpecMid.ForceUpdate++;
            SpecStep.ForceUpdate++;
            SpecSteps.ForceUpdate++;
            WaterfallCol1.ForceUpdate++;
            ShowAllBrush.SetConverter(() => new SolidColorBrush(ShowAll.Value ? Colors.LimeGreen : Colors.DarkSlateGray), ShowAll);
            ChanButtonsOpacity.SetConverter(() => EnableChanButtons.Value ? 1.0 : 0.3, EnableChanButtons);
            TxLockButtonText.SetConverter(() => $"TX {(TxLockButtonLocked.Value ? "🔒" : "🔓")}", TxLockButtonLocked);
            if (ComPort.Value.Length == 0 && ComPorts.Length > 0)
                ComPort.Value = ComPorts[0];
            if(AudioInDevice.Value.Length == 0 && AudioInDevices.Length > 0)
                AudioInDevice.Value = AudioInDevices[0];
            if (AudioOutDevice.Value.Length == 0 && AudioOutDevices.Length > 0)
                AudioOutDevice.Value = AudioOutDevices[0];
            LCD.Activator++;
            MouseActions.Activator++;
            MenuActions.Activator++;
            Sound.Activator++;
            SpectrumAnalyzer.Activator++;
            Preset.Activator++;
            Channel.Activator++;
            Comms.Activator++;
            AudioInDevice.ForceUpdate++;
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
            HeatBG.Value = new(WaterfallCol2.Value, WaterfallCol1.Value, 90.0);
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
            fontAdj = new(UserFolder.File($"{LCDFontName.Value}.font"));
            FStretch.Value = fontAdj.GetValue(nameof(FStretch), "1.17").ToDouble();
            VSize.Value = fontAdj.GetValue(nameof(VSize), "1.0").ToDouble();
            HSize.Value = fontAdj.GetValue(nameof(HSize), "1.0").ToDouble();
            VOffset.Value = fontAdj.GetValue(nameof(VOffset), "0.0").ToDouble();
            HOffset.Value = fontAdj.GetValue(nameof(HOffset), "0.0").ToDouble();
        }


    }
}
