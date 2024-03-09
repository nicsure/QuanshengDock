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
using QuanshengDock.ExtendedVFO;

namespace QuanshengDock.Data
{
    // code by -nicsure- 2024
    // https://www.youtube.com/nicsure

    public class Context
    {
        public static Context Instance => instance;
        private static readonly Context instance = new();
        private SavedDictionary? fontAdj = null;

        public ViewModel<string> Version { get; } = new("0.32.21q", nameof(Version));
        public ViewModel<string> Title { get; } = new(string.Empty, nameof(Title), true);
        public ViewModel<string> TaskBar { get; } = new(string.Empty, nameof(TaskBar));
        public ViewModel<string> MessageInput { get; } = new(string.Empty, nameof(MessageInput));
        public ViewModel<RenderTargetBitmap> LcdImage { get; } = new(new(1024, 512, 96, 96, PixelFormats.Pbgra32), nameof(LcdImage));
        public ViewModel<RenderTargetBitmap> SpectrumImage { get; } = new(new(1024, 512, 96, 96, PixelFormats.Pbgra32), nameof(SpectrumImage));
        public ViewModel<RenderTargetBitmap> ScanImage { get; } = new(new(1, 1, 96, 96, PixelFormats.Pbgra32), nameof(ScanImage));
        public ViewModel<object> MouseCommand { get; } = new(nameof(MouseCommand));
        public ViewModel<ObservableCollection<Preset>> MenuCommand { get; } = new(new(), nameof(MenuCommand));
        public ViewModel<string> TxLockButtonText { get; } = new(nameof(TxLockButtonText));
        public ViewModel<bool> TxLockButtonLocked { get; } = new(true, nameof(TxLockButtonLocked));
        public ViewModel<string> ComPort { get; } = new(string.Empty, nameof(ComPort), true);
        public ViewModel<double> HOffset { get; } = new(0.0, name: nameof(HOffset));
        public ViewModel<double> VOffset { get; } = new(0.0, name: nameof(VOffset));
        public ViewModel<double> HSize { get; } = new(1.0, nameof(HSize));
        public ViewModel<double> VSize { get; } = new(1.0, nameof(VSize));
        public ViewModel<double> FStretch { get; } = new(0.23, nameof(FStretch));
        public ViewModel<double> Volume { get; } = new(0.75, nameof(Volume), true);
        public ViewModel<double> Boost { get; } = new(1.0, nameof(Boost), true);
        public ViewModel<double> MicLevel { get; } = new(1.0, nameof(MicLevel));
        public ViewModel<double> MicBarHeight { get; } = new(20.0, nameof(MicBarHeight));
        public ViewModel<bool> MicBarShown { get; } = new(true, nameof(MicBarShown), true);
        public ViewModel<bool> FMOnlyTX { get; } = new(true, nameof(FMOnlyTX), true);
        public ViewModel<Typeface> LCDFont { get; } = new(nameof(LCDFont));
        public ViewModel<Typeface> LCDBoldFont { get; } = new(nameof(LCDBoldFont));
        public ViewModel<string> LCDFontName { get; } = new("Consolas", nameof(LCDFontName), true);
        public ViewModel<ColorBrushPen> LCDBackColor { get; } = new(new(Colors.Black, 0), nameof(LCDBackColor), true);
        public ViewModel<ColorBrushPen> LCDForeColor { get; } = new(new(Colors.LimeGreen, 0), nameof(LCDForeColor), true);
        public ViewModel<ColorBrushPen> LEDColor { get; } = new(new(Colors.Black, 0), nameof(LEDColor));
        public ViewModel<string> AudioInDevice { get; } = new(string.Empty, nameof(AudioInDevice), true);
        public ViewModel<string> AudioOutDevice { get; } = new(string.Empty, nameof(AudioOutDevice), true);
        public ViewModel<string> RadioInDevice { get; } = new(string.Empty, nameof(RadioInDevice), true);
        public ViewModel<string> RadioOutDevice { get; } = new(string.Empty, nameof(RadioOutDevice), true);
        public ViewModel<bool> Passthrough { get; } = new(false, nameof(Passthrough), true);
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
        public ViewModel<Brush> OpenSquelchBrush { get; } = new(nameof(OpenSquelchBrush));
        public ViewModel<Brush> DTMFSendBrush { get; } = new(nameof(DTMFSendBrush));
        public ViewModel<double> WaterfallSpeed { get; } = new(2.0, nameof(WaterfallSpeed), true);
        public ViewModel<LinearGradientBrush> HeatBG { get; } = new(nameof(HeatBG));
        public ViewModel<LinearGradientBrush> SigXBG { get; } = new(nameof(HeatBG));
        public ViewModel<double> AudioLatency { get; } = new(100.0, nameof(AudioLatency), true);
        public ViewModel<double> AudioBuffers { get; } = new(15.0, nameof(AudioBuffers), true);
        public ViewModel<bool> OnTop { get; } = new(false, nameof(OnTop));
        public ViewModel<double> XVfoRxFreq { get; } = new(1234.56789, nameof(XVfoRxFreq));
        public ViewModel<double> XVfoTxFreq { get; } = new(0.0, nameof(XVfoTxFreq));
        public ViewModel<string> XVfoRxText { get; } = new(string.Empty, nameof(XVfoRxText));
        public ViewModel<bool> XVfoRxEnter { get; } = new(false, nameof(XVfoRxEnter));
        public ViewModel<Visibility> XVfoRxVisble { get; } = new(nameof(XVfoRxVisble));
        public ViewModel<Visibility> XVfoRxNotVisble { get; } = new(nameof(XVfoRxNotVisble));
        public ViewModel<string> XVfoRxHz { get; } = new(nameof(XVfoRxHz));
        public ViewModel<double> XVfoRssi { get; } = new(100.0, nameof(XVfoRssi));
        public ViewModel<double> Squelch { get; } = new(0.0, nameof(Squelch), true);
        public ViewModel<bool> OpenSquelch { get; } = new(false, nameof(OpenSquelch));
        public ViewModel<bool> DTMFSend { get; } = new(false, nameof(DTMFSend));
        public ViewModel<ushort> XVfoMode { get; } = new((ushort)1, nameof(XVfoMode), true);
        public ViewModel<string> XVfoModeName { get; } = new(nameof(XVfoModeName));
        public ViewModel<XBANDWIDTH> XVfoBandwidth { get; } = new(XBANDWIDTH.WIDE, nameof(XVfoBandwidth));
        public ViewModel<string> XVfoBandwidthName { get; } = new(nameof(XVfoBandwidthName));
        public ViewModel<string> XVfoStatus { get; } = new(string.Empty, nameof(XVfoStatus));
        public ViewModel<int> XVfoStep { get; } = new(13, nameof(XVfoStep), true);
        public ViewModel<string> XVfoStepName { get; } = new(nameof(XVfoStepName));
        public ViewModel<bool> TXisRX { get; } = new(true, nameof(TXisRX));
        public ViewModel<bool> TXisNotRX { get; } = new(true, nameof(TXisNotRX));
        public ViewModel<Brush> TXisRXBrush { get; } = new(nameof(TXisRXBrush));
        public ViewModel<int> VOX { get; } = new(0, nameof(VOX));
        public ViewModel<double> VOXSensitivity { get; } = new(28.0, nameof(VOXSensitivity), true);
        public ViewModel<Brush> VOXBrush { get; } = new(nameof(VOXBrush));
        public ViewModel<bool> RFGainOn { get; } = new(false, nameof(RFGainOn), true);
        public ViewModel<double> RFGain { get; } = new(0.0, nameof(RFGain), true);
        public ViewModel<string> RFGainName { get; } = new("AGC", nameof(RFGainName));
        public ViewModel<Brush> RFGainBrush { get; } = new(nameof(RFGainBrush));
        public ViewModel<XTONETYPE> XVfoToneType { get; } = new(XTONETYPE.NONE, nameof(XVfoToneType));
        public ViewModel<string> XVfoToneTypeName { get; } = new(nameof(XVfoToneTypeName));
        public ViewModel<int> XVfoCTCSS { get; } = new(0, nameof(XVfoCTCSS), true);
        public ViewModel<int> XVfoDCS { get; } = new(0, nameof(XVfoDCS), true);
        public ViewModel<string> XVfoToneName { get; } = new(nameof(XVfoToneName));
        public ViewModel<XTONETYPE> RXVfoToneType { get; } = new(XTONETYPE.NONE, nameof(RXVfoToneType));
        public ViewModel<string> RXVfoToneTypeName { get; } = new(nameof(RXVfoToneTypeName));
        public ViewModel<int> RXVfoCTCSS { get; } = new(0, nameof(RXVfoCTCSS), true);
        public ViewModel<int> RXVfoDCS { get; } = new(0, nameof(RXVfoDCS), true);
        public ViewModel<string> RXVfoToneName { get; } = new(nameof(RXVfoToneName));
        public ViewModel<XTXPOWER> XVfoTxPower { get; } = new(XTXPOWER.LOW, nameof(XVfoTxPower));
        public ViewModel<XCOMPANDER> XVfoCompander { get; } = new(XCOMPANDER.OFF, nameof(XVfoCompander));
        public ViewModel<string> XVfoCompanderName { get; } = new(nameof(XVfoCompanderName));
        public ViewModel<double> XVfoPower { get; } = new(0.0, nameof(XVfoPower));
        public ViewModel<string> XVfoPowerPct { get; } = new(nameof(XVfoPowerPct));
        public ViewModel<int> XMicGain { get; } = new(16, nameof(XMicGain), true);
        public ViewModel<string> XMicGainPct { get; } = new(nameof(XMicGainPct));
        public ViewModel<System.Windows.Controls.ContextMenu> CtcssConMenu { get; } = new(new CtcssMenu(true), nameof(CtcssConMenu));
        public ViewModel<System.Windows.Controls.ContextMenu> DcsConMenu { get; } = new(new DcsMenu(true), nameof(DcsConMenu));
        public ViewModel<System.Windows.Controls.ContextMenu> RxCtcssConMenu { get; } = new(new CtcssMenu(false), nameof(RxCtcssConMenu));
        public ViewModel<System.Windows.Controls.ContextMenu> RxDcsConMenu { get; } = new(new DcsMenu(false), nameof(RxDcsConMenu));
        public ViewModel<System.Windows.Controls.ContextMenu> ToneMenu { get; } = new(nameof(ToneMenu));
        public ViewModel<System.Windows.Controls.ContextMenu> RToneMenu { get; } = new(nameof(RToneMenu));
        public ViewModel<System.Windows.Controls.ContextMenu> SeenMenu { get; } = new(new(), nameof(SeenMenu));
        public ViewModel<ObservableCollection<VFOPreset>> VFOPresets { get; } = new(new(), nameof(VFOPresets));
        public ViewModel<string> VFOPresetInput { get; } = new(string.Empty, nameof(VFOPresetInput));
        public ViewModel<string> SelectedPreset { get; } = new(string.Empty, nameof(SelectedPreset));
        public ViewModel<string> DetectedCode { get; } = new(string.Empty, nameof(DetectedCode));
        public ViewModel<int> AutoSquelch { get; } = new(0, nameof(AutoSquelch));
        public ViewModel<string> AutoSquelchName { get; } = new(nameof(AutoSquelchName));
        public ViewModel<ObservableCollection<ScanList>> ScanLists { get; } = new(new(), nameof(ScanLists));
        public ViewModel<ObservableCollection<VFOPreset>> SelectedScanList { get; } = new(null!, nameof(SelectedScanList));
        public ViewModel<ObservableCollection<XMessage>> Messages { get; } = new(new(), nameof(Messages));
        public ViewModel<bool> BusyXVFO { get; } = new(false, nameof(BusyXVFO));
        public ViewModel<bool> NotBusyXVFO { get; } = new(nameof(NotBusyXVFO));
        public ViewModel<Visibility> VisibleWhenBusy { get; } = new(nameof(VisibleWhenBusy));
        public ViewModel<Visibility> VisibleWhenIdle { get; } = new(nameof(VisibleWhenIdle));
        public ViewModel<double> FadedWhenBusy { get; } = new(nameof(FadedWhenBusy));
        public ViewModel<double> FadedWhenIdle { get; } = new(nameof(FadedWhenIdle));
        public ViewModel<string> ScanMessage { get; } = new(string.Empty, nameof(ScanMessage));
        public ViewModel<string> ScanMonitoring { get; } = new(string.Empty, nameof(ScanMonitoring));
        public ViewModel<double> ScanRxTimeout { get; } = new(1.0, nameof(ScanRxTimeout), true);
        public ViewModel<double> ScanTotTimeout { get; } = new(10.0, nameof(ScanTotTimeout), true);
        public ViewModel<string> ScanRxTimeoutName { get; } = new(string.Empty, nameof(ScanRxTimeoutName));
        public ViewModel<string> ScanTotTimeoutName { get; } = new(string.Empty, nameof(ScanTotTimeoutName));
        public ViewModel<bool> ScanMonitor { get; } = new(false, nameof(ScanMonitor), true);
        public ViewModel<int> ScanSpeed { get; } = new(3, nameof(ScanSpeed), true);
        public ViewModel<string> ScanSpeedName { get; } = new(nameof(ScanSpeedName));
        public ViewModel<string> Monitoring { get; } = new(string.Empty, nameof(Monitoring));
        public ViewModel<string> XScanLCD { get; } = new(string.Empty, nameof(XScanLCD));
        public ViewModel<string> XVFOLCD { get; } = new("A", nameof(XVFOLCD));
        public ViewModel<bool> XWatch { get; } = new(false, nameof(XWatch), true);
        public ViewModel<string> XWatchName { get; } = new(nameof(XWatchName));
        public ViewModel<string> Register { get; } = new(string.Empty, nameof(Register));
        public ViewModel<string> Callsign { get; } = new("X9XXX", nameof(Callsign), true);
        public ViewModel<string> DTMFLog { get; } = new(string.Empty, nameof(DTMFLog));
        public ViewModel<double> SideTone { get; } = new(800.0, nameof(SideTone), true);
        public ViewModel<bool> Quantizing { get; } = new(true, nameof(Quantizing), true);
        public static string[] ComPorts => SerialPort.GetPortNames().Append("QDNH").ToArray();
        public static string[] AudioInDevices => AudioDevices.GetAudioInDevices();
        public static string[] AudioOutDevices => AudioDevices.GetAudioOutDevices();
        public ViewModel<string> UserButton1 { get; } = new("SQL", nameof(UserButton1), true);
        public ViewModel<string> UserButton2 { get; } = new("💡", nameof(UserButton2), true);
        public ViewModel<bool> LockPower { get; } = new(false, nameof(LockPower), true);
        public ViewModel<bool> TNCMode { get; } = new(false, nameof(TNCMode));
        public ViewModel<Brush> TNCModeBrush { get; } = new(nameof(TNCModeBrush));
        public ViewModel<string> PttComPort { get; } = new("COM998", nameof(PttComPort), true);
        public ViewModel<string> CatComPort { get; } = new("COM98", nameof(CatComPort), true);
        public ViewModel<string> NHost { get; } = new("127.0.0.1", nameof(NHost), true);
        public ViewModel<double> NPort { get; } = new(18822.0, nameof(NPort), true);
        public ViewModel<string> NPass { get; } = new(string.Empty, nameof(NPass), true);
        public ViewModel<bool> ScanLogger { get; } = new(false, nameof(ScanLogger), true);
        public ViewModel<bool> Dummy { get; } = new(false, nameof(Dummy), true);
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
            RadioInDevice.PropertyChanged += AudioDevice_PropertyChanged;
            RadioOutDevice.PropertyChanged += AudioDevice_PropertyChanged;
            Passthrough.PropertyChanged += AudioDevice_PropertyChanged;
            WaterfallCol1.PropertyChanged += WaterfallColour_PropertyChanged;
            WaterfallCol2.PropertyChanged += WaterfallColour_PropertyChanged;
            LCDFontName.ForceUpdate++;
            SpecMid.ForceUpdate++;
            SpecStep.ForceUpdate++;
            SpecSteps.ForceUpdate++;
            WaterfallCol1.ForceUpdate++;

            MicBarHeight.SetConverter(() => MicBarShown.Value ? 50.0 : 0.0, MicBarShown);
            TaskBar.SetConverter(() => Title.Value.Length > 0 ? Title.Value : "QD", Title);
            XWatchName.SetConverter(() => XWatch.Value?" WR":string.Empty, XWatch);
            XScanLCD.SetConverter(() => BusyXVFO.Value ? "SCAN" : string.Empty, BusyXVFO);
            ScanSpeedName.SetConverter(() => $"Scan Speed {11 - ScanSpeed.Value}", ScanSpeed);
            ScanRxTimeoutName.SetConverter(() => $"RX Time {ScanRxTimeout.Value:F1} sec" + (ScanRxTimeout.Value == 1 ? string.Empty : "s"), ScanRxTimeout);
            ScanTotTimeoutName.SetConverter(() => $"Tot Time {ScanTotTimeout.Value:F1} secs", ScanTotTimeout);
            FadedWhenBusy.SetConverter(() => BusyXVFO.Value ? 0.3 : 1, BusyXVFO);
            FadedWhenIdle.SetConverter(() => BusyXVFO.Value ? 1 : 0.3, BusyXVFO);
            VisibleWhenBusy.SetConverter(() => BusyXVFO.Value ? Visibility.Visible : Visibility.Hidden, BusyXVFO);
            VisibleWhenIdle.SetConverter(() => BusyXVFO.Value ? Visibility.Hidden : Visibility.Visible, BusyXVFO);
            NotBusyXVFO.SetConverter(() => !BusyXVFO.Value, BusyXVFO);
            XMicGainPct.SetConverter(() => $"🎤{((XMicGain.Value * 100) / 31).Clamp(0, 100)}%", XMicGain);
            AutoSquelchName.SetConverter(() => AutoSquelch.Value == 0 ? "SQ-R" : $"SQ-{AutoSquelch.Value}", AutoSquelch);
            ToneMenu.SetConverter(() =>
                XVfoToneType.Value switch
                {
                    XTONETYPE.CTCSS => CtcssConMenu.Value,
                    XTONETYPE.DCS => DcsConMenu.Value,
                    XTONETYPE.RDCS => DcsConMenu.Value,
                    _ => null!
                }
            , XVfoToneType);
            RToneMenu.SetConverter(() =>
                RXVfoToneType.Value switch
                {
                    XTONETYPE.CTCSS => RxCtcssConMenu.Value,
                    XTONETYPE.DCS => RxDcsConMenu.Value,
                    XTONETYPE.RDCS => RxDcsConMenu.Value,
                    _ => null!
                }
            , RXVfoToneType);
            XVfoCompanderName.SetConverter(() =>
                XVfoCompander.Value switch
                {
                    XCOMPANDER.RX => "C-RX",
                    XCOMPANDER.TX => "C-TX",
                    XCOMPANDER.BOTH => "C-RT",
                    _ => string.Empty,
                }, XVfoCompander);

            XVfoModeName.SetConverter(() => 
                XVfoMode.Value switch
                {
                    0 => "FM",
                    1 => "AM",
                    2 => "USB",
                    3 => "BYP",
                    4 => "RAW",
                    100 => "CW1",
                    101 => "CW2",
                    102 => "DTMF",
                    _ => "???",
                }, XVfoMode
            );
            XVfoToneName.SetConverter(() =>
                XVfoToneType.Value switch
                {
                    XTONETYPE.NONE => string.Empty,
                    XTONETYPE.CTCSS => Beautifiers.CtcssStrings[XVfoCTCSS.Value],
                    XTONETYPE.DCS => Beautifiers.DcsStrings[XVfoDCS.Value] + "N",
                    _ => Beautifiers.DcsStrings[XVfoDCS.Value] + "I",
                }, XVfoToneType, XVfoCTCSS, XVfoDCS);
            RXVfoToneName.SetConverter(() =>
                RXVfoToneType.Value switch
                {
                    XTONETYPE.NONE => string.Empty,
                    XTONETYPE.CTCSS => Beautifiers.CtcssStrings[RXVfoCTCSS.Value],
                    XTONETYPE.DCS => Beautifiers.DcsStrings[RXVfoDCS.Value] + "N",
                    _ => Beautifiers.DcsStrings[RXVfoDCS.Value] + "I",
                }, RXVfoToneType, RXVfoCTCSS, RXVfoDCS);
            XVfoPowerPct.SetConverter(() => $"{XVfoPower.Value:F0}%", XVfoPower);
            XVfoToneTypeName.SetConverter(() => XVfoToneType.Value == XTONETYPE.NONE ? string.Empty : XVfoToneType.Value.ToString(), XVfoToneType);
            RXVfoToneTypeName.SetConverter(() => RXVfoToneType.Value == XTONETYPE.NONE ? string.Empty : RXVfoToneType.Value.ToString(), RXVfoToneType);
            TXisNotRX.SetConverter(() => !TXisRX.Value, TXisRX);
            XVfoRxVisble.SetConverter(() => XVfoRxEnter.Value ? Visibility.Visible : Visibility.Hidden, XVfoRxEnter);
            XVfoRxNotVisble.SetConverter(() => XVfoRxEnter.Value ? Visibility.Hidden : Visibility.Visible, XVfoRxEnter);
            XVfoStepName.SetConverter(() => Defines.StepNames[XVfoStep.Value], XVfoStep);
            XVfoBandwidthName.SetConverter(() => XVfoBandwidth.Value.ToString(), XVfoBandwidth);
            XVfoRxHz.SetConverter(() => ((int)Math.Round(XVfoRxFreq.Value * 100000.0) % 100).ToString("D2"), XVfoRxFreq);
            RFGainBrush.SetConverter(() => new SolidColorBrush(RFGainOn.Value ? Colors.LimeGreen : Colors.DarkSlateGray), RFGainOn);
            OpenSquelchBrush.SetConverter(() => new SolidColorBrush(OpenSquelch.Value ? Colors.LimeGreen : Colors.DarkSlateGray), OpenSquelch);
            DTMFSendBrush.SetConverter(() => new SolidColorBrush(DTMFSend.Value ? Colors.LimeGreen : Colors.DarkSlateGray), DTMFSend);
            TNCModeBrush.SetConverter(() => new SolidColorBrush(TNCMode.Value ? Colors.LimeGreen : Colors.DarkSlateGray), TNCMode);
            ShowAllBrush.SetConverter(() => new SolidColorBrush(ShowAll.Value ? Colors.LimeGreen : Colors.DarkSlateGray), ShowAll);
            TXisRXBrush.SetConverter(() => new SolidColorBrush(TXisRX.Value ? Colors.DarkSlateGray : Colors.LimeGreen), TXisRX);
            VOXBrush.SetConverter(() => VOX.Value == 0 ? Brushes.DarkSlateGray : VOX.Value == 1 ? Brushes.LimeGreen : Brushes.RoyalBlue, VOX);
            ChanButtonsOpacity.SetConverter(() => EnableChanButtons.Value ? 1.0 : 0.3, EnableChanButtons);
            TxLockButtonText.SetConverter(() => $"TX {(TxLockButtonLocked.Value ? "🔒" : "✔")}", TxLockButtonLocked);
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
            VFOPreset.Activator++;
            ScanList.Activator++;
            PTT.Activator++;
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
            SigXBG.Value = new(WaterfallCol1.Value, WaterfallCol2.Value, 0.0);
        }

        private void AudioDevice_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            int inId = Array.IndexOf(AudioInDevices, AudioInDevice.Value);
            int outId = Array.IndexOf(AudioOutDevices, AudioOutDevice.Value);
            Radio.AudioOutID = outId;
            Radio.AudioInID = inId;
            inId = Array.IndexOf(AudioInDevices, RadioOutDevice.Value);
            outId = Array.IndexOf(AudioOutDevices, RadioInDevice.Value);
            Radio.RadioOutID = inId;
            Radio.RadioInID = outId;
            Sound.Start();
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
            FStretch.Value = fontAdj.GetValue(nameof(FStretch), "0.23").ToDouble();
            VSize.Value = fontAdj.GetValue(nameof(VSize), "1.0").ToDouble();
            HSize.Value = fontAdj.GetValue(nameof(HSize), "1.0").ToDouble();
            VOffset.Value = fontAdj.GetValue(nameof(VOffset), "0.0").ToDouble();
            HOffset.Value = fontAdj.GetValue(nameof(HOffset), "0.0").ToDouble();
        }


    }
}
