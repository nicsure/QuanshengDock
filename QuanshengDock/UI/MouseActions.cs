using QuanshengDock.Analyzer;
using QuanshengDock.Channels;
using QuanshengDock.ExtendedVFO;
using QuanshengDock.General;
using QuanshengDock.RepeaterBook;
using QuanshengDock.Serial;
using QuanshengDock.Settings;
using QuanshengDock.View;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Packaging;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Input;

namespace QuanshengDock.UI
{
    // code by -nicsure- 2024
    // https://www.youtube.com/nicsure

    public static class MouseActions
    {
        public static int Activator { get => 0; set { } }
        private static readonly VM command = VM.Get("MouseCommand");
        private static readonly ViewModel<bool> txLockButtonLocked = VM.Get<bool>("TxLockButtonLocked");
        private static readonly ViewModel<bool> specNorm = VM.Get<bool>("SpecNorm");
        private static readonly ViewModel<bool> showAll = VM.Get<bool>("ShowAll");
        private static readonly ViewModel<bool> openSquelch = VM.Get<bool>("OpenSquelch");
        private static readonly ViewModel<string> lcdFontName = VM.Get<string>("LCDFontName");
        private static readonly ViewModel<double> cursorX = VM.Get<double>("CursorX");
        private static readonly ViewModel<double> cursorY = VM.Get<double>("CursorY");
        private static readonly ViewModel<double> mid = VM.Get<double>("SpecMid");
        private static readonly ViewModel<double> step = VM.Get<double>("SpecStep");
        private static readonly ViewModel<double> steps = VM.Get<double>("SpecSteps");
        //private static readonly ViewModel<double> rfGain = VM.Get<double>("RFGain");
        private static readonly ViewModel<bool> rfGainOn = VM.Get<bool>("RFGainOn");
        private static readonly ViewModel<string> cursorFreq = VM.Get<string>("CursorFreq");
        private static readonly ViewModel<int> specStyle = VM.Get<int>("SpecStyle");

        private static readonly ViewModel<double> HOffset = VM.Get<double>("HOffset");
        private static readonly ViewModel<double> HSize = VM.Get<double>("HSize");
        private static readonly ViewModel<double> FStretch = VM.Get<double>("FStretch");
        private static readonly ViewModel<double> VOffset = VM.Get<double>("VOffset");
        private static readonly ViewModel<double> VSize = VM.Get<double>("VSize");
        //private static readonly ViewModel<bool> tncMode = VM.Get<bool>("TNCMode");

        public static string LastCursorFreq { get; private set; } = string.Empty;

        private static string? currentButton = null;

        static MouseActions()
        {
            command.CommandReceived += Command_CommandReceived;
        }

        private static void Command_CommandReceived(object sender, CommandReceievedEventArgs e)
        {
            BK4819.Interaction();
            if (e.Parameter is not string cmd) return;
            var v = Mouse.DirectlyOver;
            if (ushort.TryParse(cmd, out ushort key))
            {
                //if (key == 16)
                //    _ = TxPulser();
                //else
                    Comms.SendCommand(Packet.KeyPress, key);
            }
            else
            {
                switch (cmd)
                {
                    case "SetRFGain":
                        BK4819.SetRFGain(false);
                        break;
                    case "ApplyFont":
                        Comms.SendCommand(Packet.KeyPress, (ushort)13);
                        Thread.Sleep(10);
                        Comms.SendCommand(Packet.KeyPress, (ushort)19);
                        break;
                    case "ResetFont":
                        HOffset.Value = 0;
                        VOffset.Value = 0;
                        HSize.Value = 1;
                        FStretch.Value = 0.23;
                        VSize.Value = 1;
                        break;
                    case "Preset-":
                        VFOPreset.StepPreset(-1);
                        break;
                    case "Preset+":
                        VFOPreset.StepPreset(1);
                        break;
                    case "RxJogUp":
                        XVFO.Jog(true, true);
                        break;
                    case "RxJogDown":
                        XVFO.Jog(true, false);
                        break;
                    case "LeftUp":
                        if (currentButton != null)
                        {
                            if (currentButton?.Equals("XPTT") ?? false)
                            {
                                XVFO.PttUp(PttMode.Normal);
                            }
                            else
                            {
                                Comms.SendCommand(Packet.KeyPress, (ushort)19);
                                currentButton = null;
                                Radio.PulseTX = false;
                            }
                        }
                        break;
                    case "LeftDown":
                        if (v is ButtonBorder button && button.Tag is string func)
                        {
                            currentButton = func;
                            ButtonActionDown(func);
                        }
                        break;
                    case "Close":
                        CloseApp();
                        break;
                    case "FontBrowse":
                        var dlg = new System.Windows.Forms.FontDialog()
                        {
                            FontMustExist = true,
                            ShowEffects = false,
                            ShowApply = false,
                            ShowHelp = false,
                            ShowColor = false
                        };
                        if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            lcdFontName.Value = dlg.Font.Name;
                        }
                        break;
                    case "CursorOff":
                        cursorX.Value = 0;
                        cursorY.Value = 0;
                        cursorFreq.Value = string.Empty;
                        break;
                    case "Cursor":
                        Point p = MainWindow.MouseRelative();
                        cursorX.Value = p.X;
                        cursorY.Value = p.Y;
                        int oddSteps = (int)steps.Value | 1;
                        double w = MainWindow.SpectrumImageWidth() / oddSteps;
                        if (w > 0)
                        {
                            double x = Math.Floor(p.X / w);
                            x -= oddSteps >> 1;
                            x *= step.Value / 1000.0;
                            x += mid.Value;
                            LastCursorFreq = cursorFreq.Value = $"{x:F5}";
                        }
                        break;
                }
            }
        }

        private static void CloseApp()
        {
            Radio.Closing = true;
            VM.SaveBacking();
            VFOPreset.Save();
            Preset.Save();
            XVFO.Stop();
            ScanList.Save();
            _ = ShutDown();
        }

        private static async Task ShutDown()
        {
            await Task.Delay(250);
            Application.Current.Shutdown();
        }

        private static bool CheckScope()
        {
            bool b = Radio.AnalyzerMode || Radio.Monitoring;
            if (b)
            {
                SystemSounds.Asterisk.Play();
                MessageBox.Show("The Analyzer is active. The only available button on this panel is 'EXIT'");
            }
            return !b;
        }

        private static void ButtonActionDown(string func)
        {
            string[] p = func.Split(",");
            switch (p[0])
            {
                //case "TNC":
                //   tncMode.Value = !tncMode.Value;
                //    break;
                case "VOX":
                    XVFO.ToggleVOX();
                    break;
                case "RFGain":
                    rfGainOn.Value = !rfGainOn.Value;
                    BK4819.SetRFGain(true);
                    break;
                case "Messenger":
                    Messenger.Open();
                    break;
                case "Preset-":
                    VFOPreset.StepPreset(-1);
                    break;
                case "Preset+":
                    VFOPreset.StepPreset(1);
                    break;
                case "XWatch":
                    XVFO.ToggleWatch();
                    break;
                case "VfoABCD":
                    VFOPreset.ToggleMainVFO();
                    break;
                case "XScan":
                    ScanList.Open();
                    break;
                case "XPTT":
                    if(CheckTxAllowed())
                        XVFO.Ptt(PttMode.Normal);
                    break;
                case "CompanderX":
                    XVFO.ToggleCompander();
                    break;
                case "MicX":
                    XVFO.ToggleMicGain(0);
                    break;
                case "MicX-":
                    XVFO.ToggleMicGain(-1);
                    break;
                case "MicX+":
                    XVFO.ToggleMicGain(1);
                    break;
                case "TXPowerX":
                    XVFO.ToggleTxPower();
                    break;
                case "ToneX-":
                    XVFO.ToggleTone(-1, true);
                    break;
                case "ToneX+":
                    XVFO.ToggleTone(1, true);
                    break;
                case "ToneTypeX":
                    XVFO.ToggleToneType(true);
                    break;
                case "ToneRX-":
                    XVFO.ToggleTone(-1, false);
                    break;
                case "ToneRX+":
                    XVFO.ToggleTone(1, false);
                    break;
                case "ToneTypeRX":
                    XVFO.ToggleToneType(false);
                    break;
                case "TXisRX":
                    XVFO.ToggleTXisRX();
                    break;
                case "X":
                    if (p.Length > 1)
                        XVFO.Button(p[1]);
                    break;
                case "ToggleXvfo":
                    MainWindow.ToggleXvfo();
                    break;
                case "StepX+":
                    XVFO.ToggleStep(1);
                    break;
                case "StepX-":
                    XVFO.ToggleStep(-1);
                    break;
                case "SquelchX":
                    openSquelch.Value = !openSquelch.Value;
                    break;
                case "SquelchA":
                    XVFO.ToggleAutoSquelch();
                    break;
                case "BandwidthX":
                    XVFO.ToggleBandwidth();
                    break;
                case "ModeX":
                    XVFO.ToggleMode();
                    break;
                case "RepeaterBook":
                    _ = BookContext.Instance;
                    Repeater.Open();
                    break;
                case "PasteChannels":
                    Channel.PasteChannels(Channel.SelectedChannel);
                    break;
                case "CopyChannels":
                    Channel.CopyChannels();
                    break;
                case "ChannelsClear":
                    Channel.ClearAll();
                    break;
                case "ToggleSpectrum":
                    MainWindow.ToggleSpectrum();
                    if (!Radio.SpectrumVisible)
                        ExitAnalyzer();
                    break;
                case "Minimize":
                    MainWindow.Minimizer();
                    break;
                case "Maximize":
                    MainWindow.Maximizer();
                    break;
                case "Bar":
                    specStyle.Value = 0;
                    break;
                case "Line":
                    specStyle.Value = 1;
                    break;
                case "Heat":
                    specStyle.Value = 2;
                    break;
                case "WriteChannels":
                    Channel.WriteToRadio();
                    break;
                case "ReadChannels":
                    Channel.ReadFromRadio();
                    break;
                case "ShowAll":
                    showAll.Value = !showAll.Value;
                    Channel.SetDisplayed(showAll.Value);
                    break;
                case "LoadChannels":
                    Channel.LoadFromDisk();
                    break;
                case "SaveChannels":
                    Channel.SaveToDisk();
                    break;
                case "ChannelEdit":
                    if (CheckScope())
                        new ChannelEditor().ShowDialog();
                    break;
                case "Spectrum":
                    Radio.SpectrumMode = true;
                    SpectrumAnalyzer.Start();
                    break;
                case "Waterfall":
                    Radio.SpectrumMode = false;
                    SpectrumAnalyzer.Start();
                    break;
                case "Normalize":
                    specNorm.Value = !specNorm.Value;
                    break;
                case "Settings":
                    if (CheckScope())
                        new SettingsWindow().ShowDialog();
                    break;
                case "Close":
                    CloseApp();
                    break;
                case "TxLock":
                    txLockButtonLocked.Value = !txLockButtonLocked.Value;
                    break;
                case "0":
                case "1":
                case "2":
                case "3":
                case "4":
                case "5":
                case "6":
                case "7":
                case "8":
                case "9":
                case "10":
                case "11":
                case "12":
                case "14":
                case "15":
                case "17":
                case "18":
                    if(CheckScope()) 
                        Comms.SendCommand(Packet.KeyPress, ushort.Parse(func));
                    break;
                case "13":
                    ExitAnalyzer();
                    Comms.SendCommand(Packet.KeyPress, (ushort)13);
                    break;
                case "16":
                    if (CheckTxAllowed() && CheckScope())
                        Comms.SendCommand(Packet.KeyPress, ushort.Parse(func));
                        //_ = TxPulser();
                    break;
            }

        }

        public static bool CheckTxAllowed()
        {
            if (txLockButtonLocked.Value)
                MessageBox.Show("TX is Locked. To transmit you must unlock TX.");
            return !txLockButtonLocked.Value;
        }

        private static void ExitAnalyzer()
        {
            if (Radio.AnalyzerMode)
            {
                Radio.AnalyzerMode = false;
                Radio.Monitoring = false;
                Comms.SendCommand(Packet.KeyPress, (ushort)19);
                Thread.Sleep(100);
            }
        }

        public static async Task TxPulser_DISABLE()
        {
            if (Radio.PulseTX) return;
            Radio.PulseTX = true;
            while (Radio.PulseTX)
            {
                Comms.SendCommand(Packet.KeyPress, (ushort)16);
                await Task.Delay(250); 
            }
        }
    }
}
