using NAudio.Wave;
using QuanshengDock.General;
using QuanshengDock.Serial;
using QuanshengDock.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QuanshengDock.Audio
{
    public static class QDNH
    {
        private static TcpClient? client = null;
        private static WaveOutEvent? playback = null;
        private static WaveIn? capture = null;
        private static BufferedWaveProvider? provider = null;
        private static Task skipTask = Task.Run(() => { });
        private static bool active = false;

        private static readonly ViewModel<double> latency = VM.Get<double>("AudioLatency");
        private static readonly ViewModel<double> port = VM.Get<double>("NPort");
        private static readonly ViewModel<string> host = VM.Get<string>("NHost");
        private static readonly ViewModel<string> pass = VM.Get<string>("NPass");


        public static void Start()
        {
            if(active)
                Stop(false);
            try
            {
                client = new TcpClient(host.Value, (int)port.Value);                
            }
            catch 
            {
                client?.Dispose();
                client = null;                
            }
            if (client == null)
            {
                try { Comms.QDNHClient?.Close(); } catch { }
                return;
            }
            Authenticate(client);
            try
            {
                capture = new()
                {
                    WaveFormat = new(22050, 16, 1),
                    DeviceNumber = Radio.AudioInID,
                    BufferMilliseconds = (int)latency.Value,
                };
                capture.DataAvailable += Capture_DataAvailable;
                capture.StartRecording();
            }
            catch
            {
                capture?.Dispose(); 
                capture = null;
            }
            try
            {
                provider = new(new(22050, 16, 1));
                playback = new()
                {
                    DeviceNumber = Radio.AudioOutID,
                    DesiredLatency = (int)latency.Value,
                };
                playback.Init(provider);
                playback.Play();
            }
            catch
            {
                playback?.Dispose(); 
                playback = null;
                provider?.ClearBuffer();
                provider = null;
            }
            active = true;
            _ = Pump();
        }

        private static async Task Pump()
        {
            var tcp = client;
            var prov = provider;
            if (tcp != null && prov != null)
            {
                while (true)
                {
                    byte[] b = new byte[4096];
                    int br;
                    try { br = await tcp.GetStream().ReadAsync(b); } catch { br = -1; }
                    if (br <= 0) break;
                    if (prov.BufferedDuration.TotalSeconds > latency.Value / 1000.0)
                        prov.ClearBuffer();
                    prov.AddSamples(b, 0, br);
                }
            }
            Stop();
        }

        private static async Task Send(byte[] data, int length)
        {
            var tcp = client;
            if (tcp != null)
            {
                try { await tcp.GetStream().WriteAsync(data.AsMemory(0, length)); } catch { tcp = null; }
                if (tcp == null)
                    Stop();
            }
        }

        private static void Capture_DataAvailable(object? sender, WaveInEventArgs e)
        {
            if(!skipTask.IsCompleted) return;
            using(skipTask)
            {
                skipTask = Send(e.Buffer, e.BytesRecorded);
            }
        }

        public static void Stop(bool aborted = true)
        {
            if(!active) return;
            active = false;
            using (client)
            {
                try { client?.Close(); } catch { }
                client = null;
            }
            using(playback)
            {
                try { playback?.Stop(); } catch { }
                playback = null;
            }
            using(capture)
            {
                try { capture?.StopRecording(); } catch { }
                if (capture != null)
                {
                    try { capture.DataAvailable -= Capture_DataAvailable; } catch { }
                }
                capture = null;
            }
            provider?.ClearBuffer();
            provider = null;
            if(aborted)
            {
                try { Comms.QDNHClient?.Close(); } catch { }
            }
        }


        public static void Authenticate(TcpClient tcp)
        {
            if (tcp != null)
            {
                byte[] salt = new byte[32];
                for (int i = 0; i < salt.Length; i++)
                {
                    try { salt[i] = (byte)tcp.GetStream().ReadByte(); } catch { }
                }
                using SHA256 sha = SHA256.Create();
                byte[] h = sha.ComputeHash(Encoding.ASCII.GetBytes(pass.Value).Concat(salt).ToArray());
                try { tcp.GetStream().Write(h, 0, h.Length); } catch { }
            }
        }

    }
}
