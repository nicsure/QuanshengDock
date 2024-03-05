using Microsoft.Win32;
using QuanshengDock.ExtendedVFO;
using QuanshengDock.General;
using QuanshengDock.UI;
using QuanshengDock.View;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QuanshengDock.Serial
{
    public static class PTT
    {
        public static int Activator { get => 0; set { } }
        private static readonly ViewModel<bool> tncMode = VM.Get<bool>("TNCMode");
        private static readonly ViewModel<bool> txLock = VM.Get<bool>("TxLockButtonLocked");
        private static readonly ViewModel<string> pttCom = VM.Get<string>("PttComPort");
        private static readonly ViewModel<string> catCom = VM.Get<string>("CatComPort");
        private static readonly ViewModel<bool> txisRX = VM.Get<bool>("TXisRX");
        private static readonly ViewModel<double> txFreq = VM.Get<double>("XVfoTxFreq");
        private static readonly ViewModel<double> rxFreq = VM.Get<double>("XVfoRxFreq");
        private static readonly ViewModel<int> ctcss = VM.Get<int>("XVfoCTCSS");
        private static readonly ViewModel<int> dcs = VM.Get<int>("XVfoDCS");
        private static readonly ViewModel<XTONETYPE> toneType = VM.Get<XTONETYPE>("XVfoToneType");
        private static readonly ViewModel<int> rxCtcss = VM.Get<int>("RXVfoCTCSS");
        private static readonly ViewModel<int> rxDcs = VM.Get<int>("RXVfoDCS");
        private static readonly ViewModel<XTONETYPE> rxToneType = VM.Get<XTONETYPE>("RXVfoToneType");
        //private static readonly ViewModel<XTONETYPE> tonetype = VM.Get<XTONETYPE>("XVfoToneType");
        //private static readonly ViewModel<int> ctcss = VM.Get<int>("XVfoCTCSS");
        //private static readonly ViewModel<int> dcs = VM.Get<int>("XVfoDCS");

        private static readonly SerialPort? pttport = null, catport = null;
        private static bool ptt = false;
        private static Task? old = null;

        static PTT()
        {
            try
            {
                pttport = new SerialPort(pttCom.Value, 38400, Parity.None, 8, StopBits.One);
                pttport.Open();
                pttport.PinChanged += DCD;
            }
            catch { } // I don't give a crap if I'm catching all exceptions, OKAY???? Get a life Sheldon.
            try
            {
                if (pttCom.Value.Equals(catCom.Value))
                    catport = pttport;
                else
                {
                    catport = new SerialPort(catCom.Value, 38400, Parity.None, 8, StopBits.One);
                    catport.Open();
                }
                Task.Run(CAT);
            }
            catch { }
        }

        private static void Send(string s)
        {
            byte[] b = Encoding.ASCII.GetBytes(s);
            try { pttport?.Write(b, 0, b.Length); } catch { }
        }

        private static void CAT()
        {
            string s = string.Empty;
            int b;
            while (true)
            {
                try { b = catport?.ReadByte() ?? -1; } catch { break; }
                if (b == -1) break;
                if (b == ';')
                {
                    if (s.Length >= 2 && Radio.IsXVFO)
                    {
                        bool trans = true;
                        Radio.Invoke(() => trans = BK4819.Transmitting);
                        if (trans)
                        {
                            while (trans)
                            {
                                while (BK4819.Transmitting)
                                {
                                    Thread.Sleep(10);
                                }
                                Thread.Sleep(250);
                                Radio.Invoke(() => trans = BK4819.Transmitting);
                            }
                        }
                        string cmd = s[..2];
                        string prm = s[2..];
                        int i;
                        double d;
                        uint fi;
                        switch (cmd.ToUpper())
                        {
                            case "AB":
                                txisRX.Value = false;
                                break;
                            case "BA":
                                rxFreq.Value = txFreq.Value;
                                txisRX.Value = false;
                                break;
                            case "BD":
                                d = rxFreq.Value;
                                d /= 100;
                                d -= 1;
                                d *= 1000;
                                rxFreq.Value = d.Clamp(0.1, 1300);
                                break;
                            case "BS":
                                if (uint.TryParse(prm, out  fi))
                                    rxFreq.Value = (fi * 100.0).Clamp(0.1, 1300);
                                break;
                            case "BU":
                                d = rxFreq.Value;
                                d /= 100;
                                d += 1;
                                d *= 1000;
                                rxFreq.Value = d.Clamp(0.1, 1300);
                                break;
                            case "BY":
                                if (prm.Length == 0)
                                    Send($"{cmd}{(BK4819.RxBusy ? 1 : 0)}0;");
                                break;
                            case "FB":                                
                            case "FA":
                                {
                                    var vm = cmd.Equals("FA") ? rxFreq : txFreq;
                                    if (prm.Length > 0)
                                    {
                                        if (uint.TryParse(prm, out fi))
                                        {
                                            if (vm == txFreq)
                                                txisRX.Value = false;
                                            d = fi / 1000000.0;
                                            vm.Value = d;
                                        }
                                    }
                                    else
                                    {
                                        uint u = (uint)Math.Round(vm.Value * 1000000.0);
                                        if (u > 999999999) u = 999999999;
                                        Send($"{cmd}{u:D9};");
                                    }
                                }
                                break;
                            case "CH":
                                if (uint.TryParse(prm, out fi))
                                {
                                    VFOPreset.StepPreset(fi == 0 ? -1 : 1);
                                }
                                break;
                            case "CN":
                                if (prm.Length == 2)
                                {
                                    switch (prm[1])
                                    {
                                        case '0':
                                            Send($"{cmd}00{ctcss.Value:D3};");
                                            break;
                                        case '1':
                                            Send($"{cmd}01{dcs.Value:D3};");
                                            break;
                                    }
                                }
                                else
                                if (prm.Length == 5)
                                {
                                    if (int.TryParse(prm[2..], out i))
                                    {
                                        switch (prm[1])
                                        {
                                            case '0':
                                                ctcss.Value = rxCtcss.Value = i;
                                                break;
                                            case '1':
                                                dcs.Value = rxDcs.Value = i;
                                                break;
                                        }
                                    }
                                }
                                break;
                            case "CT":
                                if (prm.Length == 1)
                                {
                                    if (toneType.Value == XTONETYPE.CTCSS && rxToneType.Value == XTONETYPE.CTCSS)
                                        i = 1;
                                    else if (toneType.Value == XTONETYPE.CTCSS)
                                        i = 2;
                                    else if (toneType.Value == XTONETYPE.DCS && rxToneType.Value == XTONETYPE.DCS)
                                        i = 3;
                                    else if (toneType.Value == XTONETYPE.DCS)
                                        i = 4;
                                    else i = 0;
                                    Send($"{cmd}0{i};");
                                }
                                else if (prm.Length == 2)
                                {
                                    if (int.TryParse(prm[1..], out i))
                                    {
                                        switch (i)
                                        {
                                            default:
                                                toneType.Value = XTONETYPE.NONE;
                                                rxToneType.Value = XTONETYPE.NONE;
                                                break;
                                            case 1:
                                                toneType.Value = XTONETYPE.CTCSS;
                                                rxToneType.Value = XTONETYPE.CTCSS;
                                                break;
                                            case 2:
                                                toneType.Value = XTONETYPE.CTCSS;
                                                rxToneType.Value = XTONETYPE.NONE;
                                                break;
                                            case 3:
                                                toneType.Value = XTONETYPE.DCS;
                                                rxToneType.Value = XTONETYPE.DCS;
                                                break;
                                            case 4:
                                                toneType.Value = XTONETYPE.DCS;
                                                rxToneType.Value = XTONETYPE.NONE;
                                                break;
                                        }
                                    }
                                }
                                break;
                        }
                    }
                    s = string.Empty;
                    continue;
                }
                s += (char)b;
            }
        }

        private static void DCD(object sender, SerialPinChangedEventArgs e)
        {
            if(txLock.Value) txLock.Value = false;
            bool p = pttport!.CDHolding;
            if (p != ptt)
            {
                ptt = p;
                tncMode.Value = ptt;
                if (Radio.IsXVFO)
                {
                    if (ptt)
                        Radio.Invoke(() => XVFO.Ptt(PttMode.External));
                    else
                        Radio.Invoke(() => XVFO.PttUp(PttMode.External));
                }
                else
                {
                    if (ptt)
                    {
                        using (old)
                        {
                            old = Task.Run(PttLoop);
                        }
                    }
                    else
                        Comms.SendCommand(Packet.KeyPress, (ushort)19);
                }
            }
        }

        private static void PttLoop()
        {
            while(pttport!.CDHolding)
            {
                Comms.SendCommand(Packet.KeyPress, (ushort)16);
                Thread.Sleep(50);
            }
        }
    }
}
