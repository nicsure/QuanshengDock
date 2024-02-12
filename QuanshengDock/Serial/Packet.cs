using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanshengDock.Serial
{
    // code by -nicsure- 2024
    // https://www.youtube.com/nicsure

    public static class Packet
    {
        public const ushort Hello = 0x514;
        public const ushort GetRssi = 0x527;
        public const ushort KeyPress = 0x801;
        public const ushort GetScreen = 0x803;
        public const ushort Scan = 0x808;
        public const ushort ScanAdjust = 0x809;
        public const ushort ScanReply = 0x908;
        public const ushort WriteRegisters = 0x850;
        public const ushort ReadRegisters = 0x851;
        public const ushort RegisterInfo = 0x951;
        public const ushort WriteGPIO = 0x860;
        public const ushort ReadGPIO = 0x861;
        public const ushort GPIOInfo = 0x961;
        public const ushort GPIOPulse = 0x862;
        public const ushort EnterHardwareMode = 0x0870;
        public const ushort ExitHardwareMode = 0x0871;
        public const ushort SetReportReg = 0x0872;
        public const ushort ImHere = 0x515;
        public const ushort RssiInfo = 0x528;
        public const ushort WriteEeprom = 0x51D;
        public const ushort WriteEepromReply = 0x51E;
        public const ushort ReadEeprom = 0x51B;
        public const ushort ReadEepromReply = 0x51C;
    }
}
