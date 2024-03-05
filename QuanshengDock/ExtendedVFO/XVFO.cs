using NAudio.Dmo;
using QuanshengDock.Audio;
using QuanshengDock.Channels;
using QuanshengDock.General;
using QuanshengDock.Serial;
using QuanshengDock.UI;
using QuanshengDock.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanshengDock.ExtendedVFO
{
    public enum PttMode
    {
        None = 0, Normal = 1, VOX = 2, External = 4
    }

    public static class XVFO
    {
        private static readonly ViewModel<double> rxFreq = VM.Get<double>("XVfoRxFreq");
        private static readonly ViewModel<double> txFreq = VM.Get<double>("XVfoTxFreq");
        private static readonly ViewModel<string> rxText = VM.Get<string>("XVfoRxText");
        private static readonly ViewModel<ushort> vfoMode = VM.Get<ushort>("XVfoMode");
        private static readonly ViewModel<XBANDWIDTH> bandwidth = VM.Get<XBANDWIDTH>("XVfoBandwidth");
        private static readonly ViewModel<int> vfoStep = VM.Get<int>("XVfoStep");
        private static readonly ViewModel<bool> enterMode = VM.Get<bool>("XVfoRxEnter");
        private static readonly ViewModel<bool> txisRX = VM.Get<bool>("TXisRX");
        private static readonly ViewModel<int> vox = VM.Get<int>("VOX");
        private static readonly ViewModel<XTONETYPE> toneType = VM.Get<XTONETYPE>("XVfoToneType");
        private static readonly ViewModel<int> vfoCtcss = VM.Get<int>("XVfoCTCSS");
        private static readonly ViewModel<int> vfoDcs = VM.Get<int>("XVfoDCS");
        private static readonly ViewModel<double> power = VM.Get<double>("XVfoPower");
        private static readonly ViewModel<XCOMPANDER> compander = VM.Get<XCOMPANDER>("XVfoCompander");
        private static readonly ViewModel<bool> txLock = VM.Get<bool>("TxLockButtonLocked");
        private static readonly ViewModel<XTONETYPE> rxToneType = VM.Get<XTONETYPE>("RXVfoToneType");
        private static readonly ViewModel<int> vfoRxCtcss = VM.Get<int>("RXVfoCTCSS");
        private static readonly ViewModel<int> vfoRxDcs = VM.Get<int>("RXVfoDCS");
        private static readonly ViewModel<int> autoSq = VM.Get<int>("AutoSquelch");
        private static readonly ViewModel<int> micGain = VM.Get<int>("XMicGain");
        private static readonly ViewModel<string> selected = VM.Get<string>("SelectedPreset");
        private static readonly ViewModel<bool> watch = VM.Get<bool>("XWatch");
        private static readonly ViewModel<bool> quantizing = VM.Get<bool>("Quantizing");
        private static readonly ViewModel<bool> dtmfSend = VM.Get<bool>("DTMFSend");
        private static readonly ViewModel<string> dtmfLog = VM.Get<string>("DTMFLog");



        static XVFO()
        {
            rxFreq.PropertyChanged += RxFreq_PropertyChanged;
        }

        private static void RxFreq_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            SetFrequency(rxFreq.Value);
        }

        public static void Jog(bool rx, bool up)
        {
            if (rx)
            {
                enterMode.Value = false;
                double step = Defines.StepValues[vfoStep.Value];
                double newFreq = rxFreq.Value + (up ? step : -step);
                rxFreq.Value = newFreq.Clamp(0.01, 1300.0);
            }
        }

        public static void Quantize()
        {
            enterMode.Value = false;
            int step = (int)Math.Round(Defines.StepValues[vfoStep.Value] * 100000);
            int freq = (int)Math.Round(rxFreq.Value * 100000);
            int qfreq = step * (freq / step);
            if (qfreq != freq)
                rxFreq.Value = qfreq / 100000.0;
        }

        //private static bool qskip = false;
        public static void SetFrequency(double d)
        {
            if (BK4819.Ready)
            {
                BK4819.SetFrequency(d);
                if (txisRX.Value)
                    txFreq.Value = d;
                if(!BK4819.Transmitting)
                    selected.Value = VFOPreset.CurrentVFO.LastPreset = string.Empty;
                //if (quantizing.Value && !qskip)
                //{
                //    qskip = true;
                //    Quantize();
                //    qskip = false;
                //}
            }
        }

        public static void ToggleMicGain(int dir)
        {
            int i;
            switch (dir)
            {
                case 0:
                    i = micGain.Value + 6;
                    break;
                case 1:
                case -1:
                    i = micGain.Value + dir;
                    break;
                default:
                    return;
            }
            if (i < 0) i = 31;
            if (i > 31) i = 0;
            micGain.Value = i;
        }

        public static void ToggleVOX(int newVal = -1)
        {
            if (newVal == -1)
                vox.Value = vox.Value != 0 ? 0 : 1;
            else
                vox.Value = newVal;
            VOX.Init();
        }

        private static PttMode pttMode = PttMode.None;

        public static void Ptt(PttMode mode)
        {
            bool clear;
            lock (vox)
            {
                clear = pttMode == PttMode.None;
                pttMode |= mode;
            }
            if (clear)
            {
                if (!txLock.Value)
                    _ = BK4819.Transmit();
            }
        }

        public static void PttUp(PttMode mode)
        {
            bool clear;
            lock (vox)
            {
                pttMode &= ~mode;
                clear = pttMode == PttMode.None;
            }
            if (clear)
                _ = BK4819.TransmitOff();
        }

        public static void AppendDTMF(string b)
        {
            dtmfLog.Value += b;
            if (dtmfLog.Value.Length > 14)
                dtmfLog.Value = dtmfLog.Value[1..];
        }

        public static void Button(string b)
        {
            if (dtmfSend.Value && !b.Equals("DTMF") && !txLock.Value)
            {
                var i = b switch
                {
                    "0" => 0,
                    "1" => 1,
                    "2" => 2,
                    "3" => 3,
                    "4" => 4,
                    "5" => 5,
                    "6" => 6,
                    "7" => 7,
                    "8" => 8,
                    "9" => 9,
                    "A" => 10,
                    "B" => 11,
                    "C" => 12,
                    "D" => 13,
                    "Dot" => 14,
                    "Del" => 15,
                    _ => -1,
                };
                if (i > -1)
                {
                    _ = BK4819.SendDTMF(i);
                    if (b.Equals("Dot")) b = "*";
                    if (b.Equals("Del")) b = "#";
                    AppendDTMF(b);
                }
                return;
            }
            bool oldMode = enterMode.Value;
            if(int.TryParse(b, out int _))
            {
                if (!oldMode)
                {
                    rxText.Value = b;
                    enterMode.Value = true;
                }
                else
                {
                    bool isDot = b.Equals(".") || b.Equals(",");
                    bool hasDot = rxText.Value.Contains('.') || rxText.Value.Contains(',');
                    if (hasDot || rxText.Value.Length < 4)
                    {
                        if(!hasDot || !isDot)
                            rxText.Value += b;
                    }
                }
            }
            else
            {
                switch (b)
                {
                    case "A":
                        VFOPreset.ToggleMainVFO(0);
                        break;
                    case "B":
                        VFOPreset.ToggleMainVFO(1);
                        break;
                    case "C":
                        VFOPreset.ToggleMainVFO(2);
                        break;
                    case "D":
                        VFOPreset.ToggleMainVFO(3);
                        break;
                    case "Abort":
                        enterMode.Value = false;
                        rxText.Value = string.Empty;
                        break;
                    case "Dot":
                        rxText.Value += ".";
                        break;
                    case "Ret":
                        if (enterMode.Value)
                        {
                            enterMode.Value = false;
                            if (rxText.Value.DoubleParse(out double d))
                                rxFreq.Value = d.Clamp(0.01, 1300.0);
                        }
                        rxText.Value = string.Empty;
                        break;
                    case "Del":
                        if (enterMode.Value)
                        {
                            rxText.Value = rxText.Value[..^1];
                            if (rxText.Value.Length == 0)
                                enterMode.Value = false;
                        }
                        break;
                    case "DTMF":
                        dtmfSend.Value = !dtmfSend.Value;
                        Comms.SendCommand(Packet.WriteRegisters, (ushort)2, (ushort)0x3f, (ushort)0, (ushort)0x3f, (ushort)(dtmfSend.Value ? 0x800 : 0));
                        break;
                }
            }

        }

        public static void ToggleCompander()
        {
            compander.Value = compander.Value switch
            {
                XCOMPANDER.OFF => XCOMPANDER.RX,
                XCOMPANDER.RX => XCOMPANDER.TX,
                XCOMPANDER.TX => XCOMPANDER.BOTH,
                _ => XCOMPANDER.OFF
            };
            BK4819.SetCompander();
        }

        public static void ToggleAutoSquelch()
        {
            int i = autoSq.Value;
            i++;
            if (i > 4) i = 0;
            autoSq.Value = i;
        }

        public static void ToggleTxPower()
        {
            double d = power.Value + 10;
            if (d > 100) d = 0;
            power.Value = d;
        }

        public static void ToggleTone(int dir, bool tx)
        {
            int i;
            var cts = tx ? vfoCtcss : vfoRxCtcss;
            var dcs = tx ? vfoDcs : vfoRxDcs;
            var tt = tx ? toneType : rxToneType;
            switch (tt.Value)
            {
                case XTONETYPE.CTCSS:
                    i = cts.Value + dir;
                    if (i >= Beautifiers.CtcssStrings.Length - 2)
                        i = 0;
                    if (i < 0) 
                        i = Beautifiers.CtcssStrings.Length - 2;
                    cts.Value = i;
                    break;
                case XTONETYPE.DCS:
                case XTONETYPE.RDCS:
                    i = dcs.Value + dir;
                    if (i >= Beautifiers.DcsStrings.Length - 2)
                        i = 0;
                    if (i < 0) 
                        i = Beautifiers.DcsStrings.Length - 2;
                    dcs.Value = i;
                    break;
            }
        }

        public static void ToggleToneType(bool tx)
        {
            var tt = tx ? toneType : rxToneType;
            tt.Value = tt.Value switch
            {
                XTONETYPE.NONE => XTONETYPE.CTCSS,
                XTONETYPE.CTCSS => XTONETYPE.DCS,
                XTONETYPE.DCS => XTONETYPE.RDCS,
                _ => XTONETYPE.NONE
            };
        }

        public static void SetCtcss(int index, bool tx)
        {
            var tt = tx ? toneType : rxToneType;
            var cts = tx ? vfoCtcss : vfoRxCtcss;
            if (tt.Value == XTONETYPE.CTCSS)
            {
                cts.Value = index;
            }
        }

        public static void SetDcs(int index, bool tx)
        {
            var tt = tx ? toneType : rxToneType;
            var dcs = tx ? vfoDcs : vfoRxDcs;
            if (tt.Value == XTONETYPE.DCS || tt.Value == XTONETYPE.RDCS)
            {
                dcs.Value = index;
            }
        }

        public static void ToggleWatch()
        {
            watch.Value = !watch.Value;
        }

        public static void ToggleTXisRX()
        {
            txisRX.Value = !txisRX.Value;
            if(txisRX.Value)
                txFreq.Value = rxFreq.Value;
        }

        public static void ToggleMode(int mode = -1)
        {
            if (mode == -1)
            {
                vfoMode.Value = vfoMode.Value switch
                {
                    0 => 1,
                    1 => 2,
                    2 => 3,
                    3 => 4,
                    4 => 100,
                    100 => 101,
                    _ => 0,
                };
            }
            else
                vfoMode.Value = (ushort)mode;
            BK4819.Modulation();
        }

        public static void ToggleStep(int dir)
        {
            int i = vfoStep.Value + dir;
            if (i >= Defines.StepValues.Length) i = 0;
            if (i < 0) i = Defines.StepValues.Length - 1;
            SetStep(i);
        }

        public static void SetStep(int index)
        {
            vfoStep.Value = index.Clamp(0, Defines.StepValues.Length - 1);
            if (quantizing.Value)
                Quantize();
        }

        public static void ToggleBandwidth()
        {
            bandwidth.Value = bandwidth.Value switch
            {
                XBANDWIDTH.WIDE => XBANDWIDTH.NRRW,
                XBANDWIDTH.NRRW => XBANDWIDTH.THIN,
                XBANDWIDTH.THIN => XBANDWIDTH.UWIDE,
                XBANDWIDTH.UWIDE => XBANDWIDTH.ULOW,
                _ => XBANDWIDTH.WIDE
            };
            BK4819.Bandwidth();
        }

        public static void ToggleBandwidthWN()
        {
            bandwidth.Value = bandwidth.Value switch
            {
                XBANDWIDTH.WIDE => XBANDWIDTH.NRRW,
                _ => XBANDWIDTH.WIDE
            };
            BK4819.Bandwidth();
        }

        public static void Start()
        {
            if(!BK4819.Ready)
            {
                BK4819.Aquire();
            }
        }

        public static void Stop()
        {
            BK4819.Release();
        }
    }
}
