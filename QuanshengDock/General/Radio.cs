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
        public const string Version = "0.27.5q";
        public static bool Closing { get; set; } = false;
        public static bool AnalyzerMode { get; set; } = false;
        public static bool SpectrumMode { get; set; } = false;
        public static bool WaterfallMode => !SpectrumMode;
        public static uint MonitoredFreq { get; set; } = 0;
        public static bool Monitoring { get; set; } = false;
        public static RState State { get; set; } = RState.None;
        public static bool DesignMode { get; } = DesignerProperties.GetIsInDesignMode(new DependencyObject());

        public static void Invoke(Action action)
        {
            if (!Closing)
                (_ = Application.Current)?.Dispatcher.Invoke(action);
        }
    }
}
