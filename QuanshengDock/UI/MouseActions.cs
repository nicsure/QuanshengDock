using QuanshengDock.Analyzer;
using QuanshengDock.Channels;
using QuanshengDock.General;
using QuanshengDock.Serial;
using QuanshengDock.Settings;
using QuanshengDock.View;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
        private static readonly ViewModel<string> lcdFontName = VM.Get<string>("LCDFontName");
        private static readonly ViewModel<double> cursorX = VM.Get<double>("CursorX");
        private static readonly ViewModel<double> cursorY = VM.Get<double>("CursorY");
        private static readonly ViewModel<double> mid = VM.Get<double>("SpecMid");
        private static readonly ViewModel<double> step = VM.Get<double>("SpecStep");
        private static readonly ViewModel<double> steps = VM.Get<double>("SpecSteps");
        private static readonly ViewModel<string> cursorFreq = VM.Get<string>("CursorFreq");
        private static readonly ViewModel<int> specStyle = VM.Get<int>("SpecStyle");

        public static string LastCursorFreq { get; private set; } = string.Empty;

        private static string? currentButton = null;

        static MouseActions()
        {
            command.CommandReceived += Command_CommandReceived;
        }

        private static void Command_CommandReceived(object sender, CommandReceievedEventArgs e)
        {
            if (e.Parameter is not string cmd) return;
            var v = Mouse.DirectlyOver;
            if(ushort.TryParse(cmd, out ushort key))
            {
                Comms.SendCommand(Packet.KeyPress, key);
            }
            else
            switch (cmd)
            {
                case "LeftUp":
                    if (currentButton != null)
                    {
                        Comms.SendCommand(Packet.KeyPress, (ushort)19);
                        currentButton = null;
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

        private static void CloseApp()
        {
            Radio.Closing = true;
            VM.SaveBacking();
            Preset.Save();
            _ = ShutDown();
        }

        private static async Task ShutDown()
        {
            await Task.Delay(250);
            Application.Current.Shutdown();
        }

        private static void ButtonActionDown(string func)
        {
            switch (func)
            {
                case "ToggleSpectrum":
                    MainWindow.ToggleSpectrum();
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
                    Comms.SendCommand(Packet.KeyPress, ushort.Parse(func));
                    break;
                case "13":
                    if (Radio.AnalyzerMode)
                    {
                        Radio.AnalyzerMode = false;
                        Radio.Monitoring = false;
                        Comms.SendCommand(Packet.KeyPress, (ushort)19);
                        Thread.Sleep(100);
                    }
                    Comms.SendCommand(Packet.KeyPress, (ushort)13);
                    break;
                case "16":
                    if(!txLockButtonLocked.Value)
                        Comms.SendCommand(Packet.KeyPress, (ushort)16);
                    break;
            }

        }
    }
}
