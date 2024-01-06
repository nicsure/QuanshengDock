using NAudio.CoreAudioApi;
using NAudio.Wave;
using QuanshengDock.General;
using QuanshengDock.View;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanshengDock.Audio
{
    // code by -nicsure- 2024
    // https://www.youtube.com/nicsure

    public static class Sound
    {
        public static int Activator { get => 0; set { } }

        private static WaveOutEvent? outEvent = null;
        private static WaveIn? input = null;
        private static WaveInProvider? provider = null;

        private static readonly ViewModel<double> volume = VM.Get<double>("Volume");
        private static readonly ViewModel<bool> passthrough = VM.Get<bool>("Passthrough");

        static Sound()
        {
            volume.PropertyChanged += (object? sender, PropertyChangedEventArgs e) =>
            {
                if (outEvent is WaveOutEvent waveOutEvent && waveOutEvent != null)
                    waveOutEvent.Volume = (float)volume.Value;
            };
        }

        public static void Start(int inputID, int outputID)
        {
            try { input?.StopRecording(); } catch { }
            try { outEvent?.Stop(); } catch { }
            if (!passthrough.Value || Radio.DesignMode) return;
            try
            {
                using (outEvent)
                    outEvent = new();
                using (input)
                    input = new();
                provider = null;
                outEvent.Volume = (float)volume.Value;
                if (inputID != -1 && outputID != -1)
                {
                    outEvent.DesiredLatency = 40;
                    outEvent.DeviceNumber = outputID;
                    outEvent.NumberOfBuffers = 15;
                    input.DeviceNumber = inputID;
                    input.WaveFormat = new(44100, 24, 1);
                    input.BufferMilliseconds = 50;
                    provider = new(input);
                    outEvent.Init(provider);
                    outEvent.Play();
                    input.StartRecording();
                }
            }
            catch { }
        }

    }
}
