using NAudio.CoreAudioApi;
using NAudio.Wave;
using QuanshengDock.General;
using QuanshengDock.View;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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

        public static float[] SinTable { get; private set; }

        private static WaveOutEvent? outEvent = null;
        private static WaveOutEvent? toneEvent = null;
        private static WaveOutEvent? toneEvent2 = null;
        private static WaveIn? input = null;
        private static WaveInProvider? provider = null;
        private static SineStream? sineStream = null;
        private static SineStream? sineStream2 = null;

        private static readonly ViewModel<double> volume = VM.Get<double>("Volume");
        private static readonly ViewModel<bool> passthrough = VM.Get<bool>("Passthrough");
        private static readonly ViewModel<double> latency = VM.Get<double>("AudioLatency");
        private static readonly ViewModel<double> buffers = VM.Get<double>("AudioBuffers");

        static Sound()
        {
            SinTable = CalcSinWave();
            volume.PropertyChanged += (object? sender, PropertyChangedEventArgs e) =>
            {
                if (outEvent is WaveOutEvent waveOutEvent && waveOutEvent != null)
                    waveOutEvent.Volume = (float)volume.Value;
            };
        }

        public static void SetToneVolume(bool on)
        {
            if (sineStream != null) sineStream.Volume = (float)(on ? volume.Value / 2 : 0);
            if (sineStream2 != null) sineStream2.Volume = (float)(on ? volume.Value / 2 : 0);
        }

        private static float[] CalcSinWave()
        {
            List<float> sinTable = new();
            for (int i = 0; i < 360; i+=8)
            {
                float sin = (float)Math.Sin(i * 0.017453);
                sinTable.Add(sin);
            }
            return sinTable.ToArray();
        }

        public static void StopTone()
        {
            try { toneEvent?.Stop(); } catch { }
            using (toneEvent)
                toneEvent = null;
            using (sineStream)
                sineStream = null;
        }

        public static void StartTone(int outputID, int toneFreq)
        {
            if (toneEvent == null && outputID > -1)
            {
                try { toneEvent?.Stop(); } catch { }
                try
                {
                    using (toneEvent)
                        toneEvent = new();
                    using (sineStream)
                        sineStream = new(toneFreq * 45);
                    toneEvent.Volume = 1;
                    toneEvent.DesiredLatency = (int)latency.Value;
                    toneEvent.NumberOfBuffers = (int)buffers.Value;
                    toneEvent.DeviceNumber = outputID;
                    toneEvent.Init(sineStream);
                    toneEvent.Play();
                }
                catch { }
            }
        }

        public static void StopTone2()
        {
            try { toneEvent2?.Stop(); } catch { }
            using (toneEvent2)
                toneEvent2 = null;
            using (sineStream2)
                sineStream2 = null;
        }

        public static void StartTone2(int outputID, int toneFreq)
        {
            if (toneEvent2 == null && outputID > -1)
            {
                try { toneEvent2?.Stop(); } catch { }
                try
                {
                    using (toneEvent2)
                        toneEvent2 = new();
                    using (sineStream2)
                        sineStream2 = new(toneFreq * 45);
                    toneEvent2.Volume = 1;
                    toneEvent2.DesiredLatency = (int)latency.Value;
                    toneEvent2.NumberOfBuffers = (int)buffers.Value;
                    toneEvent2.DeviceNumber = outputID;
                    toneEvent2.Init(sineStream2);
                    toneEvent2.Play();
                }
                catch { }
            }
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
                    outEvent.DesiredLatency = (int)latency.Value;
                    outEvent.DeviceNumber = outputID;
                    outEvent.NumberOfBuffers = (int)buffers.Value;
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

    public class SineStream : WaveStream
    {
        private readonly WaveFormat format;

        public override WaveFormat WaveFormat => format;

        public override long Length => long.MaxValue;

        public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        private int cnt = 0;

        public float Volume { get; set; } = 0;

        public SineStream(int sps)
        {
            format = WaveFormat.CreateIeeeFloatWaveFormat(sps, 1);
        }

        private float waveVolume = 0;

        public override int Read(byte[] buffer, int offset, int count)
        {
            for (int i = 0; i < count; i+=4)
            {
                float f = Sound.SinTable[cnt++] * waveVolume;
                waveVolume += (Volume - waveVolume) * 0.005f;
                if (cnt >= Sound.SinTable.Length) cnt = 0;
                byte[] b = BitConverter.GetBytes(f);
                Array.Copy(b, 0, buffer, i + offset, 4);
            }
            return count;
        }
    }


}
