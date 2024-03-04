using Microsoft.VisualBasic.Logging;
using QuanshengDock.Audio;
using QuanshengDock.Channels;
using QuanshengDock.General;
using QuanshengDock.Serial;
using QuanshengDock.UI;
using QuanshengDock.User;
using QuanshengDock.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.RightsManagement;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Xps.Serialization;

namespace QuanshengDock.ExtendedVFO
{
    public static class BK4819
    {
        private static readonly ViewModel<ObservableCollection<VFOPreset>> selected = VM.Get<ObservableCollection<VFOPreset>>("SelectedScanList");
        private static readonly ViewModel<double> rxFreq = VM.Get<double>("XVfoRxFreq");
        private static readonly ViewModel<double> txFreq = VM.Get<double>("XVfoTxFreq");
        private static readonly ViewModel<double> vfoRssi = VM.Get<double>("XVfoRssi");
        private static readonly ViewModel<ushort> vfoMode = VM.Get<ushort>("XVfoMode");
        private static readonly ViewModel<double> squelch = VM.Get<double>("Squelch");
        private static readonly ViewModel<XBANDWIDTH> bandwidth = VM.Get<XBANDWIDTH>("XVfoBandwidth");
        private static readonly ViewModel<string> status = VM.Get<string>("XVfoStatus");
        private static readonly ViewModel<bool> openSquelch = VM.Get<bool>("OpenSquelch");
        private static readonly ViewModel<ColorBrushPen> led = VM.Get<ColorBrushPen>("LEDColor");
        private static readonly ViewModel<bool> txisRX = VM.Get<bool>("TXisRX");
        private static readonly ViewModel<bool> scanning = VM.Get<bool>("BusyXVFO");
        private static readonly ViewModel<double> power = VM.Get<double>("XVfoPower");
        private static readonly ViewModel<XTONETYPE> tonetype = VM.Get<XTONETYPE>("XVfoToneType");
        private static readonly ViewModel<int> ctcss = VM.Get<int>("XVfoCTCSS");
        private static readonly ViewModel<int> dcs = VM.Get<int>("XVfoDCS");
        private static readonly ViewModel<XCOMPANDER> compander = VM.Get<XCOMPANDER>("XVfoCompander");
        private static readonly ViewModel<string> detected = VM.Get<string>("DetectedCode");
        private static readonly ViewModel<int> autoSq = VM.Get<int>("AutoSquelch");
        private static readonly ViewModel<XTONETYPE> rxToneType = VM.Get<XTONETYPE>("RXVfoToneType");
        private static readonly ViewModel<int> rxCtcss = VM.Get<int>("RXVfoCTCSS");
        private static readonly ViewModel<int> rxDcs = VM.Get<int>("RXVfoDCS");
        private static readonly ViewModel<int> micGain = VM.Get<int>("XMicGain");
        private static readonly ViewModel<bool> monitor = VM.Get<bool>("ScanMonitor");
        private static readonly ViewModel<double> rxTimeout = VM.Get<double>("ScanRxTimeout");
        private static readonly ViewModel<double> totTimeout = VM.Get<double>("ScanTotTimeout");
        private static readonly ViewModel<double> sideTone = VM.Get<double>("SideTone");
        private static readonly ViewModel<int> scanSpeed = VM.Get<int>("ScanSpeed");
        private static readonly ViewModel<RenderTargetBitmap> scanImage = VM.Get<RenderTargetBitmap>("ScanImage");
        private static readonly ViewModel<ColorBrushPen> bgCol = VM.Get<ColorBrushPen>("LCDBackColor");
        private static readonly ViewModel<ColorBrushPen> fgCol = VM.Get<ColorBrushPen>("LCDForeColor");
        private static readonly ViewModel<string> monLCD = VM.Get<string>("Monitoring");
        private static readonly ViewModel<bool> watch = VM.Get<bool>("XWatch");
        private static readonly ViewModel<string> dtmfLog = VM.Get<string>("DTMFLog");
        private static readonly ViewModel<string> scanMonitoring = VM.Get<string>("ScanMonitoring");
        private static readonly ViewModel<double> rfGain = VM.Get<double>("RFGain");
        private static readonly ViewModel<string> rfGainName = VM.Get<string>("RFGainName");
        private static readonly ViewModel<bool> rfGainOn = VM.Get<bool>("RFGainOn");
        private static readonly ViewModel<bool> logger = VM.Get<bool>("ScanLogger");
        private static readonly ViewModel<string> modeName = VM.Get<string>("XVfoModeName");

        private static bool TxMute => txMute || vfoMode.Value == 1 || vfoMode.Value >= 100;

        private static ushort reg33;
        private static ushort reg30 = 0;
        private static ushort rssi;
        private static ushort reg7e;
        private static ushort reg31;
        private static ushort reg69;
        private static ushort reg6a;
        private static ushort reg65;
        private static ushort reg0c;
        private static readonly ushort reg10 = 0x7a;
        private static readonly ushort reg11 = 0x27b;
        private static readonly ushort reg12 = 0x37b;
        private static readonly ushort reg13 = 0x3be;
        private static uint currentFreq;
        public static event EventHandler? Aquired;
        private const int hysteresis = 2;
        private static bool squelchOpen = false, txMute = false;
        private static bool transmit = false, transmitPending = false;
        private static double backupRxFreq;
        private static readonly Dictionary<ushort, ushort> backupRegisters = new();
        private static int rxTone = 0;
        private static bool skipDrop = false, skipChange = false, monitoring = false, scanActive = false;
        private static int dtmfA, dtmfB;

        public static bool Transmitting => transmit;

        public static bool RxBusy => squelchOpen;

        public static bool Ready { get; private set; } = false;

        public static VFOPreset? ForceMonitor { get; set; } = null;

        public static ushort Rssi => rssi;

        public static void SetFrequency(double freq)
            => SetFrequency((uint)Math.Round(freq * 100000.0));

        public static void SetFrequency2(uint freq)
        {
            if (Ready && !transmit)
            {
                currentFreq = freq;
                reg33 &= 0b1111111111100111;
                if (freq < 28000000) reg33 |= 0b100; else reg33 |= 0b1000;
                SendCommand(Packet.WriteRegisters, (ushort)5,
                    (ushort)0x38, (ushort)(freq & 0xffff),
                    (ushort)0x39, (ushort)((freq >> 16) & 0xffff),
                    (ushort)0x33, reg33,
                    (ushort)0x30, (ushort)0,
                    (ushort)0x30, reg30
                );
            }
        }

        public static void SetFrequency(uint freq)
        {
            if (Ready && !transmit)
            {
                currentFreq = freq;
                ushort o33 = reg33;
                reg33 &= 0b1111111111100111;
                if (freq < 28000000) reg33 |= 0b100; else reg33 |= 0b1000;
                if (o33 != reg33)
                {
                    SendCommand(Packet.WriteRegisters, (ushort)5,
                        (ushort)0x38, (ushort)(freq & 0xffff),
                        (ushort)0x39, (ushort)((freq >> 16) & 0xffff),
                        (ushort)0x33, reg33,
                        (ushort)0x30, (ushort)0,
                        (ushort)0x30, reg30
                    );
                }
                else
                {
                    ushort o30 = reg30;
                    o30 &= 0b111111111111110;
                    SendCommand(Packet.WriteRegisters, (ushort)4,
                        (ushort)0x38, (ushort)(freq & 0xffff),
                        (ushort)0x39, (ushort)((freq >> 16) & 0xffff),
                        (ushort)0x30, o30,
                        (ushort)0x30, reg30
                    );
                }
            }
        }

        private static void SendCommand(ushort cmd, params object[] args)
        {
            if(!block) Comms.SendCommand(cmd, args);
        }

        public static void Aquire()
        {
            currentFreq = 0;
            Ready = false;
            Radio.IsXVFO = true;
            Radio.UsedXVFO = true;
            squelchOpen = false;
            transmit = false;
            transmitPending = false;
            SendCommand(Packet.EnterHardwareMode);
            Comms.SendHello();
            Modulation();
            Bandwidth();
            oldGi = -1;
            SendCommand(Packet.ReadRegisters, (ushort)7, 
                (ushort)0x38,
                (ushort)0x39,
                (ushort)0x33,
                (ushort)0x73,
                (ushort)0x30,
                (ushort)0x31,
                (ushort)0);
        }

        private static int oldGi = -1;
        public static void SetRFGain(bool force)
        {
            if(rfGainOn.Value)
            {
                int gi = (int)rfGain.Value;
                if (gi != oldGi || force)
                {
                    var (reg, gain) = FixAM.GainTable[gi];
                    rfGainName.Value = $"{gain}dB";
                    oldGi = gi;
                    SendCommand(Packet.WriteRegisters, (ushort)4,
                        (ushort)0x10, (ushort)reg,
                        (ushort)0x11, (ushort)reg,
                        (ushort)0x12, (ushort)reg,
                        (ushort)0x13, (ushort)reg);
                }
            }
            else
            {
                rfGainName.Value = "AGC";
                SendCommand(Packet.WriteRegisters, (ushort)4,
                    (ushort)0x10, reg10,
                    (ushort)0x11, reg11,
                    (ushort)0x12, reg12,
                    (ushort)0x13, reg13);
            }
        }

        public static void Bandwidth()
        {
            if(!transmit)
                SendCommand(Packet.WriteRegisters, (ushort)1, (ushort)0x43, (ushort)bandwidth.Value);
        }

        public static void Modulation()
        {
            ushort vfom = (ushort)(vfoMode.Value == 100 ? 2 : vfoMode.Value >= 101 ? 0 : vfoMode.Value);
            SendCommand(0x872, (ushort)1, vfom);
            SendCommand(Packet.WriteRegisters, (ushort)1, (ushort)0x50, (ushort)(vfoMode.Value >= 100 ? 0xbb20 : 0x3b20));
            //SendCommand(Packet.WriteRegisters, (ushort)1, (ushort)0x13, (ushort)0x03BE);
            SetRFGain(true);
        }

        public static void Release()
        {
            SendCommand(Packet.ExitHardwareMode);
            Ready = false;
            transmit = false;
            Radio.IsXVFO = false;
        }

        public static void SetCompander()
        {
            if (compander.Value == XCOMPANDER.OFF)
            {
                reg31 &= 0xfff7;
                SendCommand(Packet.WriteRegisters, (ushort)1, (ushort)0x31, reg31);
            }
            else
            {
                ushort v29 = 0x2b40;
                ushort v28 = 0x2b38;
                ushort v31 = reg31;
                v31 |= 8;
                if (transmit && (compander.Value == XCOMPANDER.TX || compander.Value == XCOMPANDER.BOTH))
                    v29 |= 0x8000;
                if (!transmit && (compander.Value == XCOMPANDER.RX || compander.Value == XCOMPANDER.BOTH))
                    v28 |= 0x4000;
                SendCommand(Packet.WriteRegisters, (ushort)3, (ushort)0x29, v29, (ushort)0x28, v28, (ushort)0x31, v31);
            }
        }

        public static void PrepareFSK()
        {
            Comms.SendCommand(Packet.WriteRegisters, (ushort)5,
                    (ushort)0x70, (ushort)0xe0,
                    (ushort)0x72, (ushort)0x3065,
                    (ushort)0x58, (ushort)0xc1,
                    (ushort)0x5c, (ushort)0x5665,
                    (ushort)0x5d, (ushort)0x4700
                );
            Comms.SendCommand(Packet.WriteRegisters, (ushort)2,
                    (ushort)0x3f, (ushort)0,
                    (ushort)0x59, (ushort)0x68
                );
            Thread.Sleep(30);
            Comms.SendCommand(Packet.WriteRegisters, (ushort)1,
                    (ushort)0x30, (ushort)0
                );
            Thread.Sleep(200);
            Comms.SendCommand(Packet.WriteRegisters, (ushort)8,
                    (ushort)0x02, (ushort)0,
                    (ushort)0x3f, (ushort)0,
                    (ushort)0x37, (ushort)0x1f0f,
                    (ushort)0x30, (ushort)0,
                    (ushort)0x30, (ushort)0xbff1,
                    (ushort)0x3f, (ushort)0x3000,
                    (ushort)0x59, (ushort)0x4068,
                    (ushort)0x59, (ushort)0x3068
                );
        }

        private static readonly Queue<ushort[]> fskQueue = new();
        public static async Task SendFSK(ushort[] data)
        {
            lock(fskQueue)
                fskQueue.Enqueue(data);
            await WaitFSK();
        }

        private static async Task WaitFSK()
        {
            while (true)
            {
                ushort[]? data = null;
                lock (fskQueue)
                {
                    if (fskQueue.Count == 0)
                        break;
                    if (!squelchOpen && !transmit)
                        data = fskQueue.Dequeue();
                }
                if (data != null)
                    await SendFSK2(data);
                await Task.Delay(250 + RandomNumberGenerator.GetInt32(500));
            }
        }

        private static async Task SendFSK2(ushort[] data)
        {
            if (transmit) return;
            _ = Transmit();
            await Task.Delay(250);
            int ex = 33 - (data.Length % 33);
            if (ex < 33) data = data.Concat(new ushort[ex]).ToArray();
            for (int start = 0, end = 33; start < data.Length; start += 33, end += 33)
            {
                reg0c &= 0xfffe;
                await Task.Delay(20);
                SendCommand(Packet.WriteRegisters, (ushort)8,
                        (ushort)0x70, (ushort)0xe0,
                        (ushort)0x72, (ushort)0x3065,
                        (ushort)0x58, (ushort)0xc1,
                        (ushort)0x5c, (ushort)0x5665,
                        (ushort)0x5d, (ushort)0x4700,
                        (ushort)0x3f, (ushort)0x8000,
                        (ushort)0x59, (ushort)0x8068,
                        (ushort)0x59, (ushort)0x68
                    );
                await Task.Delay(250);
                SendCommand(Packet.WriteRegisters, (ushort)1, (ushort)0x5f, (ushort)0xabcd);
                int crc = 0;
                for (int i = start; i < end; i++)
                {
                    ushort u = data[i];
                    SendCommand(Packet.WriteRegisters, (ushort)1, (ushort)0x5f, u);
                    crc = Comms.Crc16(u & 0xff, crc);
                    crc = Comms.Crc16((u >> 8) & 0xff, crc);
                }
                SendCommand(Packet.WriteRegisters, (ushort)1, (ushort)0x5f, (ushort)crc);
                SendCommand(Packet.WriteRegisters, (ushort)1, (ushort)0x5f, (ushort)0xdcba);
                await Task.Delay(20);
                SendCommand(Packet.WriteRegisters, (ushort)1, (ushort)0x59, (ushort)0x2868);
                int timeout = 10;
                while ((reg0c & 1) == 0 && timeout-- > 0)
                {
                    SendCommand(Packet.ReadRegisters, (ushort)1, (ushort)0x0c);
                    await Task.Delay(125);
                }
                SendCommand(Packet.WriteRegisters, (ushort)1, (ushort)0x02, (ushort)0);
                await Task.Delay(20);
                SendCommand(Packet.WriteRegisters, (ushort)2,
                        (ushort)0x3f, (ushort)0x0,
                        (ushort)0x59, (ushort)0x68
                    );
                await Task.Delay(500);
            }
            _ = TransmitOff();
            PrepareFSK();
            txMute = false;
        }

        public static void Chat(string from, string to, string message)
        {
            if (from.Length > 10) from = from[..10];
            if (to.Length > 10) to = to[..10];
            string head = from + ((char)0) + to + ((char)0);
            int mLen = 66 - head.Length;
            int ex = mLen - (message.Length % mLen);
            if (ex < mLen) message += new string((char)0, ex);
            List<ushort> data = new();
            for (int i = 0; i < message.Length; i += mLen)
            {
                byte[] thisMess = Encoding.ASCII.GetBytes(string.Concat(head, message.AsSpan(i, mLen)));
                for (int j = 0; j < 66; j+=2)
                    data.Add(BitConverter.ToUInt16(thisMess, j));
            }
            _ = SendFSK(data.ToArray());
            data.Clear(); // help the garbage collector a little
        }

        private static void TransmitEnd()
        {
            Sound.StopTone();
            if (transmit)
            {
                ushort r30 = 0;
                foreach (ushort reg in backupRegisters.Keys)
                {
                    if (reg == 0x30)
                    {
                        r30 = backupRegisters[reg];
                        continue;
                    }
                    if (reg == 0x33) reg33 = backupRegisters[reg];
                    SendCommand(Packet.WriteRegisters, (ushort)1, reg, backupRegisters[reg]);
                }
                SendCommand(Packet.WriteRegisters, (ushort)1, (ushort)0x70, (ushort)0);
                SendCommand(Packet.WriteRegisters, (ushort)2, (ushort)0x30, (ushort)0, (ushort)0x30, r30);
                SendCommand(0x876);
                rxFreq.Value = backupRxFreq;
                transmit = false;
                SetCompander();
                Mute();
                if (lastVfoMode > -1)
                {
                    vfoMode.Value = (ushort)lastVfoMode;
                    lastVfoMode = -1;
                    dtmfLog.Value = string.Empty;
                }
            }
        }

        public static async Task Transmit()
        {
            dtmfLog.Value = string.Empty;
            transmitComplete = false;
            TransmitStart();
            int cnt = 0;
            while (transmitPending && cnt++ < 100)
                await Task.Delay(25);
            if (!transmitPending && vfoMode.Value >= 100)
            {
                if(vfoMode.Value == 102)
                {
                    Sound.StartTone(Radio.AudioOutID, dtmfA);
                    Sound.StartTone2(Radio.AudioOutID, dtmfB);
                }
                else
                    Sound.StartTone(Radio.AudioOutID, (int)sideTone.Value);
                if (!isKeyed)
                {
                    SendCommand(0x873);
                    isKeyed = true;
                    Sound.SetToneVolume(true);
                    key++;
                    int freq = (int)Math.Round(txFreq.Value * 100000.0);
                    vfoRssi.Value = power.Value;
                    SetPower(freq, power.Value);
                    if (vfoMode.Value >= 101)
                    {
                        SendCommand(Packet.WriteRegisters, (ushort)1,
                            (ushort)0x50, (ushort)0x3b20
                        );
                    }
                    if(vfoMode.Value ==102)
                    {
                        SendCommand(Packet.WriteRegisters, (ushort)2,
                            (ushort)0x71, ScaleFreq(dtmfA),
                            (ushort)0x72, ScaleFreq(dtmfB)
                        );
                    }
                }
            }
            transmitComplete = true;
        }

        private static ushort ScaleFreq(long freq)
        {
            return (ushort)(((freq * 1353245) + 65536) >> 17);
        }

        private static bool transmitComplete = false;

        public static async Task TransmitOff()
        {
            int cnt = 0;
            while (!transmitComplete && cnt++ < 100)
                await Task.Delay(25);
            if (vfoMode.Value >= 100)
            {
                SendCommand(0x874);
                isKeyed = false;
                vfoRssi.Value = 0;
                Sound.SetToneVolume(false);
                if (vfoMode.Value == 102)
                {
                    await Task.Delay(50);
                    Sound.StopTone();
                    Sound.StopTone2();
                }
                int freq = (int)Math.Round(txFreq.Value * 100000.0);
                if(vfoMode.Value == 100)
                    SetPower(freq, 0);
                else
                {
                    SendCommand(Packet.WriteRegisters, (ushort)1,
                        (ushort)0x50, (ushort)0xbb20
                    );
                }
                _ = UnkeyCW(key);

            }
            else
                TransmitEnd();
        }

        private static int key = 0;
        private static bool isKeyed = false;
        private static async Task UnkeyCW(int wkey)
        {
            await Task.Delay(3000);
            if (key == wkey)
                TransmitEnd();
        }

        private static void TransmitStart()
        {
            if (!transmit)
            {
                transmit = true;
                transmitPending = true;
                backupRegisters.Clear();
                SendCommand(Packet.ReadRegisters,
                    (ushort)12,
                    (ushort)0x24,
                    (ushort)0x33,
                    (ushort)0x38,
                    (ushort)0x39,
                    (ushort)0x30,
                    (ushort)0x31,
                    (ushort)0x47,
                    (ushort)0x50,
                    (ushort)0x37,
                    (ushort)0x33,
                    (ushort)0x35,
                    (ushort)0x7e
                );
            }
        }

        private static uint DCS_GetCdcssCode(uint Code)
        {
            for (uint i = 0; i < 23; i++)
            {
                uint Shift;
                if (((Code >> 9) & 0x7U) == 4)
                {
                    int t = Array.IndexOf(Defines.Golays, Code);
                    if (t > -1)
                        return Beautifiers.DcsOptions[t];
                }
                Shift = Code >> 1;
                if ((Code & 1U) !=0)
                    Shift |= 0x400000U;
                Code = Shift;
            }
            return 0;
        }

        private static void SetPower(int freq, double pwrpct)
        {
            ushort pwr = (ushort)(((int)(pwrpct * 2.55).Clamp(0, 255)) << 8);
            SendCommand(Packet.WriteRegisters, (ushort)1, (ushort)0x36, (ushort)((freq < 2800000 ? 0x88 : 0xa2) | pwr));
        }

        private static async void GoTransmit()
        {
            if (!transmitPending) return;
            Mute();
            led.Value.Color = Colors.Red;
            status.Value = "TX";
            int freq = (int)Math.Round(txFreq.Value * 100000.0);
            reg7e = (ushort)((reg7e & ~(0b111 << 3)) | 0x28);
            backupRxFreq = rxFreq.Value;
            rxFreq.Value = txFreq.Value;
            vfoRssi.Value = power.Value;
            SendCommand(Packet.WriteRegisters, (ushort)16,
                (ushort)0x7d, (ushort)(0xE940 | (vfoMode.Value >= 100 ? 0 : micGain.Value)),
                (ushort)0x24, (ushort)0,
                (ushort)0x33, BK4819GPIO(0, false),
                (ushort)0x38, (ushort)(freq & 0xffff),
                (ushort)0x39, (ushort)((freq >> 16) & 0xffff),
                (ushort)0x47, (ushort)0x6040,
                (ushort)0x7e, reg7e,
                (ushort)0x50, (ushort)(TxMute ? 0xbb20 : 0x3b20),
                (ushort)0x37, (ushort)0x1D0F,
                (ushort)0x30, (ushort)0,
                (ushort)0x30, (ushort)0xc1fe,
                (ushort)0x33, BK4819GPIO(4, freq < 2800000),
                (ushort)0x33, BK4819GPIO(3, freq >= 2800000),
                (ushort)0x33, BK4819GPIO(1, true),
                (ushort)0x33, BK4819GPIO(5, true),
                (ushort)0x33, BK4819GPIO(6, false)
                );
            if (vfoMode.Value == 101)
            {
                await Task.Delay(50);
                SendCommand(Packet.WriteRegisters, (ushort)2,
                    (ushort)0x70, (ushort)0xbf00,
                    (ushort)0x71, ScaleFreq((long)sideTone.Value)
                );
            }
            if (vfoMode.Value == 102)
            {
                await Task.Delay(50);
                SendCommand(Packet.WriteRegisters, (ushort)3,
                    (ushort)0x70, (ushort)0xbfbf,
                    (ushort)0x71, dtmfA,
                    (ushort)0x72, dtmfB
                );
            }
            SetCompander();
            Thread.Sleep(5);
            SetPower(freq, vfoMode.Value == 100 ? 0.0 : power.Value);
            Thread.Sleep(10);
            switch (tonetype.Value)
            {
                case XTONETYPE.CTCSS:
                    if (ctcss.Value < 50)
                    {
                        if (Beautifiers.CtcssStrings[ctcss.Value].DoubleParse(out double d))
                        {
                            long l = (((long)Math.Round(d * 10) * 206488L) + 50000L) / 100000L;
                            SendCommand(Packet.WriteRegisters, (ushort)2, (ushort)0x51, (ushort)0x904a, (ushort)7, (ushort)l);
                        }
                    }
                    break;
                case XTONETYPE.DCS:
                case XTONETYPE.RDCS:
                    if (dcs.Value<104)
                    {
                        uint code = tonetype.Value == XTONETYPE.RDCS ? Defines.GolaysR[dcs.Value] : Defines.Golays[dcs.Value];
                        SendCommand(Packet.WriteRegisters, (ushort)4,
                            (ushort)0x51, (ushort)0x8033,
                            (ushort)7, (ushort)0xad7,
                            (ushort)8, (ushort)(code & 0xfff),
                            (ushort)8, (ushort)(((code >> 12) & 0xfff) | 0x8000)
                        );
                    }
                    break;
                case XTONETYPE.NONE:
                    SendCommand(Packet.WriteRegisters, (ushort)1, (ushort)0x51, (ushort)0);
                    break;
            }
            if(vfoMode.Value == 1)
                SendCommand(0x875);
            transmitPending = false;
        }

        public static async Task Send1750()
        {
            if (transmit)
            {
                Sound.StartTone(Radio.AudioOutID, 1750);
                Sound.SetToneVolume(true);
                SendCommand(Packet.WriteRegisters, (ushort)2,
                    (ushort)0x70, (ushort)0xbf00,
                    (ushort)0x71, ScaleFreq(1750)
                );
                await Task.Delay(1000);
                SendCommand(Packet.WriteRegisters, (ushort)1,
                    (ushort)0x70, (ushort)0
                );
                Sound.SetToneVolume(false);
                Sound.StopTone();
            }
        }

        private static int lastVfoMode = -1;
        private static bool dtmfOverlap = false;
        public static async Task SendDTMF(int toneIndex)
        {
            int to = 0;
            while (dtmfOverlap && to++ < 10)
            {
                await Task.Delay(100);
            }
            if (!dtmfOverlap)
            {
                int delay;
                dtmfOverlap = true;
                dtmfA = Defines.DTMF[toneIndex].A;
                dtmfB = Defines.DTMF[toneIndex].B;
                if (lastVfoMode == -1)
                {
                    lastVfoMode = vfoMode.Value;
                    vfoMode.Value = 102;
                    delay = 500;
                }
                else
                    delay = 250;
                await Transmit();
                await Task.Delay(delay);
                await TransmitOff();
                dtmfOverlap = false;
            }
        }

        public static ushort BK4819GPIO(int pin, bool set)
        {
            int val = 0x40 >> pin;
            if (set)
                val |= reg33;
            else
                val = ~val & reg33;
            reg33 = (ushort)val;
            return reg33;
        }

        public static void Interaction()
        {
            lock(watch)
            {
                if(watchMode)
                {
                    watchMode = false;
                    watchStart = 0;
                    SetFrequency(rxFreq.Value);
                }
            }
        }

        public static void RegisterValue(ushort register, ushort value)
        {
            if(transmit)
            {
                if (register != 0x67 && register != 0x65 && register != 0x64)
                {
                    backupRegisters[register] = value;
                    if (register == 0x7e)
                        GoTransmit();
                }
                return;
            }
            switch (register)
            {
                case 0x7e:
                    reg7e = value;
                    break;
                case 0x0c:
                    if ((value & 0x800) != 0)
                    {

                    }
                    break;
                case 0x0:
                    rxFreq.Value = currentFreq / 100000.0;
                    if (txisRX.Value || txFreq.Value == 0)
                        txFreq.Value = rxFreq.Value;
                    Ready = true;
                    (_ = Aquired)?.Invoke(null, EventArgs.Empty);
                    Mute();
                    SetCompander();
                    Radio.Invoke(VFOPreset.CurrentVFO.Recall);
                    SetRFGain(true);
                    _ = Loop();
                    break;
                case 0x02:
                    //Debug.WriteLine($"0x02: {value:X4}");
                    break;
                case 0x30:
                    if (reg30 == 0)
                        reg30 = value;
                    break;
                case 0x31:
                    reg31 = value;
                    break;
                case 0x33:
                    reg33 = value;
                    break;
                case 0x38:
                    currentFreq = value;
                    break;
                case 0x39:
                    currentFreq |= (uint)(value << 16);
                    break;
                case 0x67:
                    rssi = (ushort)(value & 0x1ff);
                    if (vfoMode.Value == 1)
                        FixAM.Tick(rssi, currentFreq);
                    break;
                case 0x65:
                    value &= 0x7f;
                    reg65 = value;
                    rssi = (ushort)(value < rssi ? rssi - value : 0);
                    rssipct = rssi / 3.2;                    
                    if (!scanActive)
                    {
                        vfoRssi.Value = rssipct;
                        if (!squelchOpen)
                        {
                            lock (watch)
                            {
                                if (watchMode ? WatchSquelchTest(value, watchVFO) : SquelchTestOpen(value))
                                {
                                    Unmute();
                                    SendCommand(Packet.WriteRegisters, (ushort)1, (ushort)0x33, BK4819GPIO(6, true));
                                    if (watchMode)
                                    {
                                        Radio.Invoke(watchVFO.Recall);
                                        watchStart = 0;
                                        watchMode = false;
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (SquelchTestClosed(value))
                            {
                                Mute();
                                SendCommand(Packet.WriteRegisters, (ushort)1, (ushort)0x33, BK4819GPIO(6, false));
                                if (dtmfLog.Value.Length > 0 && dtmfLog.Value[^1] != '.')
                                    dtmfLog.Value += ".";
                            }
                            else
                            {
                                if(monitoring)
                                    monitorLastRx = DateTime.Now.Ticks;
                            }
                        }
                    }
                    else
                        NextPreset();
                    break;
                case 0x69:
                    reg69 = value;
                    break;
                case 0x6a:
                    reg6a = value;
                    break;
                case 0x68:
                    if ((reg69 & 0x8000) == 0)
                    {
                        int a = ((reg69 & 0xfff) << 12) | (reg6a & 0xfff);
                        uint b = DCS_GetCdcssCode((uint)a);
                        if (b > 0)
                        {
                            skipDrop = false;
                            rxTone = (int)b;
                            detected.Value = $"D{Convert.ToString(b, 8).PadLeft(3, '0')}N";
                            break;
                        }
                    }
                    if ((value & 0x8000) == 0)
                    {
                        int newTone = ((value & 0x1FFF) * 4843) / 10000;
                        newTone = (int)Beautifiers.ClosestCtcss((uint)newTone);
                        if (rxTone != 0 && newTone != rxTone && skipChange)
                        {
                            skipChange = false;
                            break;
                        }
                        skipDrop = true;
                        skipChange = true;
                        rxTone = newTone;                        
                        detected.Value = $"{rxTone / 10f:F1}";
                        break;
                    }

                    if (rxTone != 0)
                    {
                        if(!skipDrop)
                            rxTone = 0;
                    }
                    skipDrop = false;
                    skipChange = false;
                    detected.Value = string.Empty;
                    break;
            }
        }

        private static bool ScanSquelchTest(int noise)
        {
            switch (current.Asql)
            {
                case 0:
                    return rssipct >= current.Sql;
                default:
                    int s = 85 - (current.Asql * 15);
                    return noise <= s;
            }
        }

        private static bool WatchSquelchTest(int noise, VFOPreset vfo)
        {
            if (vfo.RxToneType == XTONETYPE.CTCSS)
            {
                return rxTone == Beautifiers.CtcssOptions[vfo.RxCTCSS];
            }
            if (vfo.RxToneType == XTONETYPE.DCS)
            {
                return rxTone == Beautifiers.DcsOptions[vfo.RxDCS];
            }
            if (vfo.RxToneType == XTONETYPE.RDCS)
            {
                return rxTone == Beautifiers.DcsOptions[Defines.ItoN[vfo.RxDCS]];
            }
            switch (vfo.Asql)
            {
                case 0:
                    return rssipct >= vfo.Squelch;
                default:
                    int s = 85 - (vfo.Asql * 15);
                    return noise <= s;
            }
        }

        private static bool SquelchTestOpen(int noise)
        {
            if (openSquelch.Value || vfoMode.Value == 100) return true;
            if (rxToneType.Value == XTONETYPE.CTCSS)
            {
                return rxTone == Beautifiers.CtcssOptions[rxCtcss.Value];
            }
            if (rxToneType.Value == XTONETYPE.DCS)
            {
                return rxTone == Beautifiers.DcsOptions[rxDcs.Value];
            }
            if (rxToneType.Value == XTONETYPE.RDCS)
            {
                return rxTone == Beautifiers.DcsOptions[Defines.ItoN[rxDcs.Value]];
            }
            switch (autoSq.Value)
            {
                case 0:
                    return squelch.Value == 0 || rssipct >= squelch.Value;
                default:
                    int s = 85 - (autoSq.Value * 15);
                    return noise <= s;
            }
        }

        private static bool SquelchTestClosed(int noise)
        {
            if (openSquelch.Value || vfoMode.Value == 100) return false;
            if (rxToneType.Value == XTONETYPE.CTCSS)
            {
                return rxTone != Beautifiers.CtcssOptions[rxCtcss.Value];
            }
            if (rxToneType.Value == XTONETYPE.DCS)
            {
                return rxTone != Beautifiers.DcsOptions[rxDcs.Value];
            }
            if (rxToneType.Value == XTONETYPE.RDCS)
            {
                return rxTone != Beautifiers.DcsOptions[Defines.ItoN[rxDcs.Value]];
            }
            switch (autoSq.Value)
            {
                case 0:
                    return rssipct + hysteresis <= squelch.Value;
                default:
                    int s = 85 - (autoSq.Value * 15);
                    return noise - hysteresis >= s;
            }
        }

        private static void Mute()
        {
            squelchOpen = false;
            GPIO.EnableAudio(false);
            status.Value = string.Empty;
            led.Value.Color = Colors.Black;
        }

        private static void Unmute()
        {
            squelchOpen = true;
            GPIO.EnableAudio(true);
            status.Value = "RX";
            led.Value.Color = Colors.LimeGreen;
        }

        public static void StopScan()
        {
            scanning.Value = false;
            scanActive = false;
            monitoring = false;
            monLCD.Value = string.Empty;
            using(logFile)
                logFile = null;
        }

        private static bool paused = false;
        public static void PauseScan()
        {
            if (!paused)
            {
                scanActive = false;
                paused = true;
                Radio.Invoke(() => scanMonitoring.Value = "PAUSED");
            }
            else
            {
                paused = false;
                scanActive = true;
                Radio.Invoke(() => scanMonitoring.Value = string.Empty);
                _ = ResumeScan();
            }
        }

        private static bool block = false, startingScan = false;
        private static double rssipct;
        private static long monitorStart, monitorLastRx, watchdog;
        private static VFOPreset current = null!;
        public static bool IsScanning => scanning.Value;

        public static void StartScan()
        {
            if (!scanning.Value)
            {
                logLine = string.Empty;
                if (logger.Value)
                {
                    try { logFile = new(UserFolder.LogFile($"{Radio.NowFF}.csv"), FileMode.Create); }
                    catch { logFile?.Dispose(); logFile = null; }
                }
                Radio.Invoke(() => scanMonitoring.Value = string.Empty);
                paused = false;
                if (selected.Value == null || selected.Value.Count == 0) return;
                ignoreFreq.Clear();
                VFOPreset.ScanVFO.Recall();
                openSquelch.Value = false;
                scanning.Value = true;
                //scanImage.Value = new(selected.Value.Count * 8, 100, 96, 96, PixelFormats.Pbgra32);
                scanImage.Value = new(1008, 100, 96, 96, PixelFormats.Pbgra32);
                int cnt = 0;
                VFOPreset prev = selected.Value[^1];
                foreach (VFOPreset preset in selected.Value)
                {
                    prev.Next = preset;
                    preset.WasActive = false;
                    preset.WasRssi = 0;
                    preset.LastRssi = 0;
                    preset.Index = cnt++;
                    preset.IsScanning = false;
                    preset.IsActive = false;
                    preset.Blacklisted = false;
                    prev = preset;
                }
                int sidx = current != null ? selected.Value.IndexOf(current) : 0;
                if (sidx < 0) sidx = 0;
                current = selected.Value[sidx];
                current.Recall();
                _ = ResumeScan();
            }
        }

        public static async Task ResumeScan()
        {
            if (logLine.Length > 0 && logFile != null)
            {
                var span = DateTime.Now - logTime;
                logLine += $"{span.TotalSeconds:F1}s\r\n";
                try { logFile.Write( Encoding.ASCII.GetBytes(logLine)); } catch { }
                logLine = string.Empty;
            }
            monitoring = false;
            startingScan = true;
            await Task.Delay(300);
            Mute();
            watchdog = 0;
            scanActive = true;
            startingScan = false;
        }

        private static FileStream? logFile = null;
        private static readonly List<uint> ignoreFreq = new();
        private static string logLine = string.Empty;
        private static DateTime logTime = DateTime.Now;
        private static void NextPreset()
        {            
            if (!scanActive) return;
            Radio.Invoke(() => scanMonitoring.Value = string.Empty);
            bool sq;
            bool force;
            if (ForceMonitor != null)
            {
                force = true;
                current = ForceMonitor;
                ForceMonitor = null;
                ignoreFreq.Clear();
                current.Blacklisted = false;
                sq = true;
            }
            else
            {
                sq = ScanSquelchTest(reg65);
                force = false;
            }
            current.LastRssi = rssipct;
            if (current.IsActive = sq) current.WasActive = true;
            if (sq)
            {
                if(logFile != null)
                {
                    logLine = $"{Radio.Now},{(currentFreq / 100000.0):F5},{modeName.Value},{rssipct:F0}%,{current.SafeName},";
                    logTime = DateTime.Now;
                }
                current.WasRssi = rssipct;
                if ((force || monitor.Value) && !current.Blacklisted && !ignoreFreq.Contains(currentFreq))
                {
                    double crx = currentFreq / 100000.0;
                    Radio.Invoke(() => current.Recall(crx));
                    scanActive = false;
                    monitoring = true;
                    current.Blacklisted = true;
                    ignoreFreq.Add(currentFreq);
                    monitorLastRx = monitorStart = DateTime.Now.Ticks;
                    Radio.Invoke(() => scanMonitoring.Value = $"Monitoring {current.PName} {(currentFreq / 100000.0):F5}");
                    DrawBar(current);
                    Unmute();
                    monLCD.Value = "MON";
                    return;
                }
            }
            else
            {
                current.Blacklisted = false;
                ignoreFreq.Remove(currentFreq);
            }
            var previous = current;
            double rx = -1;
            if (current.IsRange)
            {
                current.RangeCount++;
                rx = currentFreq / 100000.0;
                rx += current.Step;
                if (Math.Round(rx * 100000.0) > Math.Round(current.TX * 100000.0))
                    rx = -1;
            }
            if (rx >= 0)
                SetFrequency(rx);
            else
            {
                current = current.Next;
                current.RangeCount = 0;
                SetFrequency(current.RX);
                if (current.IsRange)
                    current.RangeTotal = 1 + (int)Pinch(Math.Abs(current.TX - current.RX) / current.Step);
            }
            if (scanSpeed.Value > 1) Thread.Sleep((scanSpeed.Value - 1) * 10);
            SendCommand(Packet.ReadRegisters, (ushort)2, (ushort)0x67, (ushort)0x65);
            watchdog = DateTime.Now.Ticks;
            DrawBar(previous);
            var crnt = current;
            Radio.Invoke(() =>
            {
                block = true;
                previous.IsScanning = false;
                crnt.IsScanning = true;
                crnt.Recall(rx);
                block = false;
            });
        }

        private static double Pinch(double d)
        {
            return (d % 1) >= 0.999 ? Math.Ceiling(d) : Math.Floor(d);
        }

        private static void DrawBar(VFOPreset preset)
        {
            Radio.Invoke(() =>
            {
                double barWidth = 1000.0 / selected.Value.Count;
                DrawingVisual drawingVisual = new();
                using (DrawingContext context = drawingVisual.RenderOpen())
                {
                    Rect rect;
                    if (preset.IsRange)
                    {
                        double obw = barWidth;
                        barWidth /= preset.RangeTotal;
                        rect = new((preset.Index * obw) + (barWidth * preset.RangeCount), 0, barWidth + 0.1, 100);
                    }
                    else
                        rect = new(preset.Index * barWidth, 0, barWidth + 0.1, 100);
                    context.DrawRectangle(bgCol.Value.Brush, null, rect);
                    rect.Width = barWidth * 0.9;
                    if (!preset.IsRange && preset.WasRssi > 0)
                    {
                        rect.Y = 100 - preset.WasRssi;
                        context.DrawRectangle(Brushes.DarkBlue, null, rect);
                    }
                    rect.Y = 100 - preset.LastRssi;
                    context.DrawRectangle(monitoring ? Brushes.Purple : preset.IsActive ? Brushes.Red : fgCol.Value.Brush, null, rect);
                }
                scanImage.Value.Render(drawingVisual);
            });
        }

        private static async Task Loop()
        {
            while (Ready)
            {
                long now = DateTime.Now.Ticks;
                if (monitoring && !paused)
                {
                    double rxs = (now - monitorLastRx) / 10000000.0;
                    double tots = (now - monitorStart) / 10000000.0;
                    if (rxs >= rxTimeout.Value || tots >= totTimeout.Value)
                    {
                        monLCD.Value = string.Empty;
                        _ = ResumeScan();
                    }
                }
                if (scanActive)
                {
                    double wd = (now - watchdog) / 10000000.0;
                    if (wd > 3)
                    {
                        SendCommand(Packet.ReadRegisters, (ushort)2, (ushort)0x67, (ushort)0x65);
                        watchdog = DateTime.Now.Ticks;
                    }
                }
                else if (!transmit && !startingScan)
                {
                    lock (watch) { if (!squelchOpen && !scanning.Value && watch.Value) WatchTick(now); }
                    SendCommand(Packet.ReadRegisters, (ushort)5, (ushort)0x69, (ushort)0x6a, (ushort)0x68, (ushort)0x67, (ushort)0x65);
                }
                await Task.Delay(vfoMode.Value == 1 ? 100 : 250);
            }
        }

        private static long watchStart;
        private static bool watchMode;
        private static bool watchFlipFlop;
        private static int watchCnt;
        private static VFOPreset watchVFO = null!;

        private static void WatchTick(long now)
        {
            if (watchStart == 0)
            {
                watchStart = now + 30000000;
                watchMode = false;
            }
            else
            {
                if (!watchMode)
                {
                    if (now >= watchStart)
                    {
                        watchMode = true;
                        Radio.Invoke(VFOPreset.CurrentVFO.Set);
                        watchVFO = VFOPreset.CurrentVFO;
                    }
                }
                else
                {
                    if(watchFlipFlop = !watchFlipFlop)
                    {
                        if (++watchCnt >= 4) watchCnt = 0;
                        watchVFO = VFOPreset.VFOs[watchCnt];
                        SetFrequency(watchVFO.RX);
                    }
                }
            }
        }
    }
}
