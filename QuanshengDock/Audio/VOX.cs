using NAudio.Wave;
using QuanshengDock.ExtendedVFO;
using QuanshengDock.General;
using QuanshengDock.View;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace QuanshengDock.Audio
{
    public static class VOX
    {
        private static readonly ViewModel<int> vox = VM.Get<int>("VOX");
        private static readonly ViewModel<bool> txLock = VM.Get<bool>("TxLockButtonLocked");
        private static readonly ViewModel<double> sensitivity = VM.Get<double>("VOXSensitivity");
        private static WaveIn? capture = null;

        private static int tank = 0;
        private static bool active = false;

        public static void Init()
        {
            Close();
            if (vox.Value != 0)
            {
                try
                {
                    capture = new()
                    {
                        WaveFormat = new(256, 16, 1),
                        DeviceNumber = Radio.AudioInID,
                        BufferMilliseconds = 10,
                    };
                    capture.DataAvailable += Capture_DataAvailable;
                    capture.StartRecording();
                    txLock.Value = false;
                }
                catch
                {
                    capture?.Dispose();
                    capture = null;
                    vox.Value = 0;
                }
            }
        }

        private static void Close()
        {
            using (capture)
            {
                capture?.StopRecording();
                if (capture != null)
                    capture.DataAvailable -= Capture_DataAvailable;
            }
            capture = null;
            tank = 0;
            KeyDown();
        }

        private static void Capture_DataAvailable(object? sender, WaveInEventArgs e)
        {
            int sense = 3000 - (int)(sensitivity.Value * 100.0);
            int max = 0;
            for (int i = 0; i < e.BytesRecorded; i += 2)
            {
                int lev = Math.Abs((int)BitConverter.ToInt16(e.Buffer, i));
                if (lev > max) max = lev;
            }
            tank = (tank + (max > sense ? 10 : -1)).Clamp(0, 200);
            //Debug.WriteLine($"Max:{max} Tank:{tank}");
            if (tank == 200 && !active)
                KeyUp();
            if (tank == 0 && active)
                KeyDown();
        }

        private static void KeyUp()
        {
            if (!active)
            {
                vox.Value = 2;
                active = true;
                XVFO.Ptt();
            }
        }

        private static void KeyDown()
        {
            if (vox.Value == 2)
                vox.Value = 1;
            if (active)
            {
                active = false;
                XVFO.PttUp();
            }
        }

    }
}
