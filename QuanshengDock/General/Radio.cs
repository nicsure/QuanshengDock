using QuanshengDock.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace QuanshengDock.General
{
    // code by -nicsure- 2024
    // https://www.youtube.com/nicsure

    public enum RState { TX, RX, None }

    public static class Radio
    {
        public static bool Closing { get; set; } = false;
        public static bool SpectrumVisible { get; set; } = false;
        public static bool AnalyzerMode { get; set; } = false;
        public static bool SpectrumMode { get; set; } = false;
        public static bool WaterfallMode => !SpectrumMode;
        public static uint MonitoredFreq { get; set; } = 0;
        public static bool Monitoring { get; set; } = false;
        public static RState State { get; set; } = RState.None;
        public static bool DesignMode { get; } = DesignerProperties.GetIsInDesignMode(new DependencyObject());
        public static bool PulseTX { get; set; } = false;
        public static bool IsXVFO { get; set; } = false;
        public static bool UsedXVFO { get; set; } = false;
        public static bool UseCommas { get; } = (0.5).ToString()[1] == ',';
        public static int AudioOutID { get; set; } = -1;
        public static int AudioInID { get; set; } = -1;
        public static int RadioOutID { get; set; } = -1;
        public static int RadioInID { get; set; } = -1;
        public static string NowFF => DateTime.Now.ToString("ddMMMyyyy-HH_mm_ss");
        public static string Now => DateTime.Now.ToString("dd/MMM/yyyy,HH:mm.ss");

        public static void Invoke(Action action)
        {
            if (!Closing)
                (_ = Application.Current)?.Dispatcher.Invoke(action);
        }

        public static string? Prompt(string prompt, bool canbeempty, string initial = "")
        {
            var window = new TextPrompt(prompt, canbeempty, initial);
            return (window.ShowDialog() ?? false) ? window.InputText : null;
        }
    }
}
