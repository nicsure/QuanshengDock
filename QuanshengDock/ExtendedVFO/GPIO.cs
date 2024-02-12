using QuanshengDock.Serial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace QuanshengDock.ExtendedVFO
{
    public static class GPIO
    {
        public static void ReadA(GPIOA_PINS bit)
        {
            Comms.SendCommand(Packet.ReadGPIO, (ushort)1, (byte)0, (byte)bit);
        }
        public static void ReadB(GPIOA_PINS bit)
        {
            Comms.SendCommand(Packet.ReadGPIO, (ushort)1, (byte)1, (byte)bit);
        }
        public static void ReadC(GPIOA_PINS bit)
        {
            Comms.SendCommand(Packet.ReadGPIO, (ushort)1, (byte)2, (byte)bit);
        }
        public static void SetA(GPIOA_PINS bit)
        {
            Comms.SendCommand(Packet.WriteGPIO, (ushort)1, (byte)0, (byte)bit);
        }
        public static void ClearA(GPIOA_PINS bit)
        {
            Comms.SendCommand(Packet.WriteGPIO, (ushort)1, (byte)3, (byte)bit);
        }
        public static void SetB(GPIOB_PINS bit)
        {
            Comms.SendCommand(Packet.WriteGPIO, (ushort)1, (byte)1, (byte)bit);
        }
        public static void ClearB(GPIOB_PINS bit)
        {
            Comms.SendCommand(Packet.WriteGPIO, (ushort)1, (byte)4, (byte)bit);
        }
        public static void SetC(GPIOC_PINS bit)
        {
            Comms.SendCommand(Packet.WriteGPIO, (ushort)1, (byte)2, (byte)bit);
        }
        public static void ClearC(GPIOC_PINS bit)
        {
            Comms.SendCommand(Packet.WriteGPIO, (ushort)1, (byte)5, (byte)bit);
        }

        public static void EnableAudio(bool enabled)
        {
            if (enabled)
                SetC(GPIOC_PINS.GPIOC_PIN_AUDIO_PATH);
            else
                ClearC(GPIOC_PINS.GPIOC_PIN_AUDIO_PATH);
        }


    }
}
