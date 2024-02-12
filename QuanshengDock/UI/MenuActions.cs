
using QuanshengDock.Analyzer;
using QuanshengDock.ExtendedVFO;
using QuanshengDock.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace QuanshengDock.UI
{
    public static class MenuActions
    {
        // code by -nicsure- 2024
        // https://www.youtube.com/nicsure

        public static int Activator { get => 0; set { } }

        private static readonly VM command = VM.Get("MenuCommand");

        private static readonly ViewModel<double> rxTimeout = VM.Get<double>("RXTimeout");
        private static readonly ViewModel<double> totalTimeout = VM.Get<double>("TotalTimeout");
        private static readonly ViewModel<double> trigger = VM.Get<double>("Trigger");
        private static readonly ViewModel<ObservableCollection<Preset>> presets = VM.Get<ObservableCollection<Preset>>("MenuCommand");
        private static readonly ViewModel<double> specMid = VM.Get<double>("SpecMid");
        private static readonly ViewModel<double> specStep = VM.Get<double>("SpecStep");
        private static readonly ViewModel<double> specSteps = VM.Get<double>("SpecSteps");
        private static readonly ViewModel<double> specAmp = VM.Get<double>("SpecAmp");
        private static readonly ViewModel<double> specFloor = VM.Get<double>("SpecFloor");
        private static readonly ViewModel<bool> specNorm = VM.Get<bool>("SpecNorm");
        private static readonly ViewModel<bool> quantizing = VM.Get<bool>("Quantizing");
        private static readonly ViewModel<string> presetName = VM.Get<string>("PresetName");
        private static readonly ViewModel<string> cursorFreq = VM.Get<string>("CursorFreq");
        private static readonly ViewModel<bool> onTop = VM.Get<bool>("OnTop");
        private static readonly ViewModel<bool> lockPower = VM.Get<bool>("LockPower");

        private static double openedPos = 0;

        public static string SelectedFreq { get; private set; } = string.Empty;

        static MenuActions()
        {
            command.CommandReceived += Command_CommandReceived;
        }

        private static void Command_CommandReceived(object sender, CommandReceievedEventArgs e)
        {
            if (e.Parameter is not string cmd) return;
            string[] p = cmd.Split(',');
            Preset? preset = p.Length > 1 ? (int.TryParse(p[1], out int idx) && idx > -1 && presets.Value.Count > idx ? presets.Value[idx] : null) : null;
            
            switch (p[0])
            {
                case "LockPower":
                    lockPower.Value = !lockPower.Value;
                    break;
                case "ImportAll":
                    VFOPreset.Import(true);
                    break;
                case "ImportUpdate":
                    VFOPreset.Import(false);
                    break;
                case "VFOPresetMenuOpened":
                    VFOPreset.MenuOpened();
                    break;
                case "VFOPresetMenuClosed":
                    VFOPreset.MenuClosed();
                    break;
                case "VFOOverwrite":
                    VFOPreset.MenuSelected?.Set();
                    break;
                case "VFOMoveUp":
                    VFOPreset.MoveUp();
                    break;
                case "VFOMoveDown":
                    VFOPreset.MoveDown();
                    break;
                case "VFODeletePreset":
                    VFOPreset.Delete();
                    break;
                case "RenameVFOPreset":
                    if (Keyboard.IsKeyDown(Key.Enter))
                        VFOPreset.Rename();
                    break;
                case "NewVFOPreset":
                    if (Keyboard.IsKeyDown(Key.Enter))
                        VFOPreset.CreateNew();
                    break;
                case "Quantize":
                    quantizing.Value = !quantizing.Value;
                    break;
                case "SetStep":
                    if (int.TryParse(p[1], out int newStepIndex))
                        XVFO.SetStep(newStepIndex);
                    break;
                case "ToggleOnTop":
                    onTop.Value = !onTop.Value;
                    break;
                case "DeletePreset":
                    if (preset != null)
                    {
                        if(presets.Value.Contains(preset))
                            presets.Value.Remove(preset);
                        presets.ForceUpdate++;
                    }
                    break;
                case "ReplacePreset":
                    if (preset != null)
                    {
                        preset.RXTimeout = rxTimeout.Value;
                        preset.TotalTimeout = totalTimeout.Value;
                        preset.Trigger = trigger.Value;
                        preset.SpecMid = specMid.Value;
                        preset.SpecStep = specStep.Value;
                        preset.SpecSteps = specSteps.Value;
                        preset.SpecAmp = specAmp.Value;
                        preset.SpecFloor = specFloor.Value;
                        preset.SpecNorm = specNorm.Value;
                    }
                    break;
                case "LoadPreset":
                    if (preset != null)
                    {
                        rxTimeout.Value = preset.RXTimeout;
                        totalTimeout.Value = preset.TotalTimeout;
                        trigger.Value = preset.Trigger;
                        specMid.Value = preset.SpecMid;
                        specStep.Value = preset.SpecStep;
                        specSteps.Value = preset.SpecSteps;
                        specAmp.Value = preset.SpecAmp;
                        specFloor.Value = preset.SpecFloor;
                        specNorm.Value = preset.SpecNorm;
                    }
                    break;
                case "SavePreset":
                    if(Keyboard.IsKeyDown(Key.Enter))
                    {
                        _ = new Preset(presetName.Value);
                        presets.ForceUpdate++;
                        presetName.Value = string.Empty;
                    }
                    break;
                case "BlacklistCursor":
                    SpectrumAnalyzer.BlacklistCursor();
                    break;
                case "BlacklistMonitor":
                    SpectrumAnalyzer.BlacklistMonitored();
                    break;
                case "BlacklistClear":
                    SpectrumAnalyzer.ClearBlacklist();
                    break;
                case "Closed":
                    break;
                case "Opened":
                    SelectedFreq = cursorFreq.Value = MouseActions.LastCursorFreq;
                    openedPos = 511 - ((MainWindow.MouseRelative().Y / MainWindow.SpectrumImageHeight()) * 511);
                    break;
                case "ClearTrigger":
                    trigger.Value = 0;
                    break;
                case "SetTrigger":
                    trigger.Value = openedPos;
                    break;
                case "RXTimeout":
                    { rxTimeout.Value = p[1].DoubleParse(out double d) ? d : 1.5; }
                    break;
                case "TotalTimeout":
                    { totalTimeout.Value = p[1].DoubleParse(out double d) ? d : 20.0; }
                    break;
            }
        }
    }
}
