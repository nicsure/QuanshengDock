using QuanshengDock.Serial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanshengDock.ExtendedVFO
{
    public static class FixAM
    {
        private const int desiredRssi = (-89 + 160) * 2;
        private static uint lastFreq;
        private static int lastRssi = 0, holdCounter = 0, gainTableIndex = 0;

        public static (int reg, int gain)[] GainTable => gainTable;

        private static readonly (int reg, int gain)[] gainTable = new (int reg, int gain)[]
        {
            (0x03BE, -7),   //  0 .. 3 5 3 6 ..   0dB  -4dB  0dB  -3dB ..  -7dB original
	        (0x0000,-93),   //  1 .. 0 0 0 0 .. -28dB -24dB -8dB -33dB .. -93dB
	        (0x0008,-91),   //  2 .. 0 0 1 0 .. -28dB -24dB -6dB -33dB .. -91dB
	        (0x0010,-88),   //  3 .. 0 0 2 0 .. -28dB -24dB -3dB -33dB .. -88dB
	        (0x0001,-87),   //  4 .. 0 0 0 1 .. -28dB -24dB -8dB -27dB .. -87dB
	        (0x0009,-85),   //  5 .. 0 0 1 1 .. -28dB -24dB -6dB -27dB .. -85dB
	        (0x0011,-82),   //  6 .. 0 0 2 1 .. -28dB -24dB -3dB -27dB .. -82dB
	        (0x0002,-81),   //  7 .. 0 0 0 2 .. -28dB -24dB -8dB -21dB .. -81dB
	        (0x000A,-79),   //  8 .. 0 0 1 2 .. -28dB -24dB -6dB -21dB .. -79dB
	        (0x0012,-76),   //  9 .. 0 0 2 2 .. -28dB -24dB -3dB -21dB .. -76dB
	        (0x0003,-75),   // 10 .. 0 0 0 3 .. -28dB -24dB -8dB -15dB .. -75dB
	        (0x000B,-73),   // 11 .. 0 0 1 3 .. -28dB -24dB -6dB -15dB .. -73dB
	        (0x0013,-70),   // 12 .. 0 0 2 3 .. -28dB -24dB -3dB -15dB .. -70dB
	        (0x0004,-69),   // 13 .. 0 0 0 4 .. -28dB -24dB -8dB  -9dB .. -69dB
	        (0x000C,-67),   // 14 .. 0 0 1 4 .. -28dB -24dB -6dB  -9dB .. -67dB
	        (0x000D,-64),   // 15 .. 0 0 1 5 .. -28dB -24dB -6dB  -6dB .. -64dB
	        (0x001C,-61),   // 16 .. 0 0 3 4 .. -28dB -24dB  0dB - 9dB .. -61dB
	        (0x001D,-58),   // 17 .. 0 0 3 5 .. -28dB -24dB  0dB  -6dB .. -58dB
	        (0x001E,-55),   // 18 .. 0 0 3 6 .. -28dB -24dB  0dB  -3dB .. -55dB
	        (0x001F,-52),   // 19 .. 0 0 3 7 .. -28dB -24dB  0dB   0dB .. -52dB
	        (0x003E,-50),   // 20 .. 0 1 3 6 .. -28dB -19dB  0dB  -3dB .. -50dB
	        (0x003F,-47),   // 21 .. 0 1 3 7 .. -28dB -19dB  0dB   0dB .. -47dB
	        (0x005E,-45),   // 22 .. 0 2 3 6 .. -28dB -14dB  0dB  -3dB .. -45dB
	        (0x005F,-42),   // 23 .. 0 2 3 7 .. -28dB -14dB  0dB   0dB .. -42dB
	        (0x007E,-40),   // 24 .. 0 3 3 6 .. -28dB  -9dB  0dB  -3dB .. -40dB
	        (0x007F,-37),   // 25 .. 0 3 3 7 .. -28dB  -9dB  0dB   0dB .. -37dB
	        (0x009F,-34),   // 26 .. 0 4 3 7 .. -28dB  -6dB  0dB   0dB .. -34dB
	        (0x00BF,-32),   // 27 .. 0 5 3 7 .. -28dB  -4dB  0dB   0dB .. -32dB
	        (0x00DF,-30),   // 28 .. 0 6 3 7 .. -28dB  -2dB  0dB   0dB .. -30dB
	        (0x00FF,-28),   // 29 .. 0 7 3 7 .. -28dB   0dB  0dB   0dB .. -28dB
	        (0x01DF,-26),   // 30 .. 1 6 3 7 .. -24dB  -2dB  0dB   0dB .. -26dB
	        (0x01FF,-24),   // 31 .. 1 7 3 7 .. -24dB   0dB  0dB   0dB .. -24dB
	        (0x02BF,-23),   // 32 .. 2 5 3 7 .. -19dB  -4dB  0dB   0dB .. -23dB
	        (0x02DF,-21),   // 33 .. 2 6 3 7 .. -19dB  -2dB  0dB  -0dB .. -21dB
	        (0x02FF,-19),   // 34 .. 2 7 3 7 .. -19dB   0dB  0dB   0dB .. -19dB
	        (0x035E,-17),   // 35 .. 3 2 3 6 ..   0dB -14dB  0dB  -3dB .. -17dB
	        (0x035F,-14),   // 36 .. 3 2 3 7 ..   0dB -14dB  0dB   0dB .. -14dB
	        (0x037E,-12),   // 37 .. 3 3 3 6 ..   0dB  -9dB  0dB  -3dB .. -12dB
	        (0x037F,-9),    // 38 .. 3 3 3 7 ..   0dB  -9dB  0dB   0dB ..  -9dB
	        (0x038F,-6),    // 39 .. 3 4 3 7 ..   0dB - 6dB  0dB   0dB ..  -6dB
	        (0x03BF,-4),    // 40 .. 3 5 3 7 ..   0dB  -4dB  0dB   0dB ..  -4dB
	        (0x03DF,-2),    // 41 .. 3 6 3 7 ..   0dB - 2dB  0dB   0dB ..  -2dB
	        (0x03FF,0)      // 42 .. 3 7 3 7 ..   0dB   0dB  0dB   0dB ..   0dB
        };

        public static void Tick(int newRssi, uint freq)
        {
            if (lastFreq != freq)
                Reset();
            lastFreq = freq;
            int rssi = (newRssi + lastRssi) / 2;
            lastRssi = newRssi;
            if (holdCounter > 0) holdCounter--;
            int diff = (rssi - desiredRssi) / 2;
            if (diff > 0)
            {
                int index = gainTableIndex;
                if (diff >= 10)
                {
                    int desiredGain = gainTable[index].gain - diff + 8;
                    while (index > 1)
                        if (gainTable[--index].gain <= desiredGain)
                            break;
                }
                else
                {
                    if (index > 1)
                        index--;
                }
                index = index >= 1 ? index : 1;
                if (gainTableIndex != index)
                {
                    gainTableIndex = index;
                    holdCounter = 5;
                }
            }
            if (diff >= -6)
                holdCounter = 5;
            if (holdCounter == 0)
            {
                if (++gainTableIndex >= gainTable.Length)
                    gainTableIndex = gainTable.Length - 1;                
            }
            Comms.SendCommand(Packet.WriteRegisters, (ushort)1, (ushort)0x13, (ushort)gainTable[gainTableIndex].reg);
        }

        public static void Reset()
        {
            lastRssi = 0;
            holdCounter = 0;
        }
    }

}
