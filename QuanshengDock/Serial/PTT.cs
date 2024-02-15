using Microsoft.Win32;
using QuanshengDock.ExtendedVFO;
using QuanshengDock.General;
using QuanshengDock.View;
using System;
using System.Collections.Generic;
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

        private static readonly SerialPort? port = null;
        private static bool ptt = false;
        private static Task? old = null;

        static PTT()
        {
            try
            {
                port = new SerialPort(pttCom.Value, 38400, Parity.None, 8, StopBits.One);
                port.Open();
                port.PinChanged += DCD;
            }
            catch { } // I don't give a crap if I'm catching all exceptions, OKAY???? Get a life Sheldon.
        }

        private static void DCD(object sender, SerialPinChangedEventArgs e)
        {
            if(txLock.Value) txLock.Value = false;
            bool p = port!.CDHolding;
            if (p != ptt)
            {
                ptt = p;
                tncMode.Value = ptt;
                if (Radio.IsXVFO)
                {
                    if (ptt)
                        Radio.Invoke(XVFO.Ptt);
                    else
                        Radio.Invoke(XVFO.PttUp);
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
            while(port!.CDHolding)
            {
                Comms.SendCommand(Packet.KeyPress, (ushort)16);
                Thread.Sleep(50);
            }
        }
    }
}
