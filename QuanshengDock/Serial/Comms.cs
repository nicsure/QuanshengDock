
using QuanshengDock.Analyzer;
using QuanshengDock.Channels;
using QuanshengDock.General;
using QuanshengDock.UI;
using QuanshengDock.View;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace QuanshengDock.Serial
{
    public static class Comms
    {
        public static int Activator { get => 0; set { } }
        public static uint TimeStamp => timeStamp;

        private static readonly ViewModel<ColorBrushPen> ledColor = VM.Get<ColorBrushPen>("LEDColor");
        private static readonly ViewModel<string> comPort = VM.Get<string>("ComPort");
        private static readonly byte[] xor_array = { 0x16, 0x6c, 0x14, 0xe6, 0x2e, 0x91, 0x0d, 0x40, 0x21, 0x35, 0xd5, 0x40, 0x13, 0x03, 0xe9, 0x80 };
        private static uint timeStamp = 0;
        private static SerialPort? port = null;

        static Comms()
        {
            comPort.PropertyChanged += (object? sender, PropertyChangedEventArgs e) => Close();
            if (!Radio.DesignMode)
                _ = OpenPortLoop();
        }

        public static void Close()
        {
            try { port?.Close(); } catch { }
        }

        private static byte Crypt(int byt, int xori) => (byte)(byt ^ xor_array[xori & 15]);
        private static int Crc16(int byt, int crc)
        {
            crc ^= byt << 8;
            for (int i = 0; i < 8; i++)
            {
                crc <<= 1;
                if (crc > 0xffff)
                {
                    crc ^= 0x1021;
                    crc &= 0xffff;
                }
            }
            return crc;
        }

        private static async Task OpenPortLoop()
        {
            SerialPort sp;
            while (true)
            {
                try
                {
                    sp = new SerialPort(comPort.Value, 38400, Parity.None, 8, StopBits.One);
                    sp.Open();
                    port = sp;
                    SendHello();
                    await Task.Delay(50);
                    SendCommand(Packet.KeyPress, (ushort)13);
                    await Task.Delay(50);
                    SendCommand(Packet.KeyPress, (ushort)19);
                }
                catch
                {
                    await Task.Delay(1000);
                    continue;
                }
                await Task.Run(ListenLoop);
                try { sp.Close(); } catch { }
            }
        }

        private static void ListenLoop()
        {
            while (true)
            {
                int b = NextByte();
                if (b == -1) return;
                ByteIn((byte)b);
            }
        }

        private static int NextByte()
        {
            int b;
            try { b = port?.ReadByte() ?? -1; } catch { b = -1; }
            return b;
        }

        private enum Stage { Idle, CD, LenLSB, LenMSB, Data, CrcLSB, CrcMSB, DC, BA, UiType }
        private static Stage stage = Stage.Idle;
        private static int pLen, pCnt;
        private static byte[] data=Array.Empty<byte>();
        private static void ByteIn(byte b)
        {
            switch (stage)
            {
                case Stage.Idle:
                    if (b == 0xAB)
                        stage = Stage.CD;
                    else
                    if (b == 0xB5)
                        stage = Stage.UiType;
                    break;
                case Stage.CD:
                    stage = (b == 0xcd ? Stage.LenLSB : Stage.Idle);
                    break;
                case Stage.LenLSB:
                    pLen = b;
                    stage = Stage.LenMSB;
                    break;
                case Stage.LenMSB:
                    pCnt = 0;
                    pLen |= b << 8;
                    data = new byte[pLen];
                    stage = Stage.Data;
                    break;
                case Stage.Data:
                    data[pCnt] = Crypt(b, pCnt++);
                    if (pCnt >= pLen)
                        stage = Stage.CrcLSB;
                    break;
                case Stage.CrcLSB:
                    stage = Stage.CrcMSB;
                    break;
                case Stage.CrcMSB:
                    stage = Stage.DC;
                    break;
                case Stage.DC:
                    stage = (b == 0xdc ? Stage.BA : Stage.Idle);
                    break;
                case Stage.BA:
                    stage = Stage.Idle;
                    if (b == 0xba)
                        ParsePacket(data);
                    break;
                case Stage.UiType:
                    int uiType = b, uiVal1, uiVal2, uiVal3, uiDataLen;
                    for (bool once = true; once; once = !once)
                    {
                        if ((uiVal1 = NextByte()) == -1) break;
                        if ((uiVal2 = NextByte()) == -1) break;
                        if ((uiVal3 = NextByte()) == -1) break;
                        if ((uiDataLen = NextByte()) == -1) break;
                        byte[] uiData = new byte[uiDataLen];
                        for (int i = 0; i < uiDataLen; i++)
                        {
                            int t = NextByte();
                            if (t == -1)
                            {
                                uiType = -1;
                                break;
                            }
                            uiData[i] = (byte)t;
                        }
                        if (uiType > -1)
                            UiPacket(uiType, uiVal1, uiVal2, uiVal3, uiDataLen, uiData);
                    }
                    stage = Stage.Idle;
                    break;
            }
        }

        private static void ParsePacket(byte[] packet)
        {
            ushort cmd = BitConverter.ToUInt16(packet, 0);
            ushort offset;
            switch (cmd)
            {
                case Packet.ScanReply:
                    SpectrumAnalyzer.Data(packet);
                    break;
                case Packet.ReadEepromReply:
                    offset = BitConverter.ToUInt16(packet, 4);
                    byte size = data[6];
                    byte[] b = new byte[size];
                    Array.Copy(packet, 8, b, 0, size);
                    Channel.EepromDataRead(offset, b, size);
                    break;
                case Packet.WriteEepromReply:
                    offset = BitConverter.ToUInt16(packet, 4);
                    Channel.EepromDataWritten(offset);
                    break;
            }
        }

        private static void UiPacket(int type, int val1, int val2, int val3, int dataLen, byte[] data)
        {
            switch (type)
            {
                case 0:
                    while (val1 > 128) { val2++; val1 -= 128; }
                    LCD.DrawText(val1, val2 + 1, 1.5, Encoding.ASCII.GetString(data));
                    break;
                case 1:
                    while (val1 > 128) { val2++; val1 -= 128; }
                    LCD.DrawText(val1, val2 + 1, val3 / 6.0, Encoding.ASCII.GetString(data), false, false);
                    break;
                case 2:
                    while (val1 > 128) { val2++; val1 -= 128; }
                    LCD.DrawText(val1, val2 + 1, val3 / 6.0, Encoding.ASCII.GetString(data), true, true);
                    break;
                case 3:
                    while (val1 > 128) { val2++; val1 -= 128; }
                    LCD.DrawText(val1, val2 + 1, 2, Encoding.ASCII.GetString(data), false, true);
                    break;
                case 5:
                    LCD.ClearLines(val1, val2);
                    break;
                case 6:
                    string ps = string.Empty;
                    switch (val1 & 7)
                    {
                        case 1:
                            ps = "T";
                            ledColor.Value.Color = Colors.Red;
                            Radio.State = RState.TX;
                            Radio.AnalyzerMode = false;
                            Radio.Monitoring = false;
                            break;
                        case 2:
                            ps = "R";
                            ledColor.Value.Color = Radio.Monitoring ? Colors.Cyan : Colors.LimeGreen;
                            Radio.State = RState.RX;
                            break;
                        case 4:
                            ps = "PS";
                            ledColor.Value.Color = Colors.Black;
                            Radio.State = RState.None;
                            break;
                        default:
                            ledColor.Value.Color = Colors.DarkBlue;
                            Radio.State = RState.None;
                            break;
                    }
                    LCD.DrawText(0, 0, 0.5, ps);
                    if ((val1 & 8) != 0)
                        LCD.DrawText(8, 0, 0.5, "NOA");
                    if ((val1 & 16) != 0)
                        LCD.DrawText(19, 0, 0.5, "DTMF");
                    if ((val1 & 32) != 0)
                        LCD.DrawText(33, 0, 0.5, "FM");
                    if (val3 != 0)
                        LCD.DrawText(42, 0, 0.5, ((char)val3).ToString());
                    if ((val1 & 64) != 0)
                        LCD.DrawText(48, 0, 0.5, "🢀");
                    if ((val1 & 128) != 0)
                        LCD.DrawText(56, 0, 0.5, "UWR");
                    if ((val2 & 1) != 0)
                        LCD.DrawText(56, 0, 0.5, "><");
                    if ((val2 & 2) != 0)
                        LCD.DrawText(56, 0, 0.5, "XB");
                    if ((val2 & 4) != 0)
                        LCD.DrawText(68, 0, 0.5, "VOX");
                    if ((val2 & 8) != 0)
                        LCD.DrawText(80, 0, 0.5, "🔒");
                    if ((val2 & 16) != 0)
                        LCD.DrawText(80, 0, 0.6, "🄵");
                    if ((val2 & 32) != 0)
                        LCD.DrawText(85, 0, 0.5, "⚡");
                    if (Radio.Monitoring)
                        LCD.DrawText(93, 0, 0.75, "MONITOR");
                    else
                    {
                        LCD.DrawText(93, 0, 0.5, "🔋");
                        float bat = dataLen * 0.04f;
                        LCD.DrawText(99, 0, 0.5, $"{bat:F2}V {(dataLen / 2.1f):F0}%");
                    }
                    break;
                case 7:
                    LCD.DrawText(0, val1, 1, val2 == 0 ? "▻" : "➤", false, true);
                    break;
                case 8:
                    LCD.DrawSignal(val1, val2);
                    break;
            }
        }

        public static void SendHello() 
        {
            timeStamp = (uint)DateTimeOffset.Now.ToUnixTimeSeconds();
            SendCommand(Packet.Hello, timeStamp); 
        }

        public static void SendCommand(ushort cmd, params object[] args)
        {
            if (!Radio.AnalyzerMode || Radio.Monitoring || cmd == Packet.ScanAdjust)
                SendCommand2(cmd, args);
        }

        private static void SendCommand2(ushort cmd, params object[] args)
        {
            var data = new byte[256];
            data[0] = 0xAB;
            data[1] = 0xCD;
            data[4] = cmd.Byte(0);
            data[5] = cmd.Byte(1);
            int ind = 8;
            foreach (object val in args)
            {
                if (val is byte[] ba)
                {
                    foreach (byte byt in ba)
                        data[ind++] = byt;
                }
                else
                if (val is byte b)
                    data[ind++] = b;
                else if (val is ushort s1)
                {
                    data[ind++] = s1.Byte(0);
                    data[ind++] = s1.Byte(1);
                }
                else if (val is short s2)
                {
                    data[ind++] = s2.Byte(0);
                    data[ind++] = s2.Byte(1);
                }
                else if (val is uint i1)
                {
                    data[ind++] = i1.Byte(0);
                    data[ind++] = i1.Byte(1);
                    data[ind++] = i1.Byte(2);
                    data[ind++] = i1.Byte(3);
                }
                else if (val is int i2)
                {
                    data[ind++] = i2.Byte(0);
                    data[ind++] = i2.Byte(1);
                    data[ind++] = i2.Byte(2);
                    data[ind++] = i2.Byte(3);
                }
            }
            int prmLen = ind - 8;
            data[6] = prmLen.Byte(0);
            data[7] = prmLen.Byte(1);
            int crc = 0, xor = 0;
            for (int i = 4; i < ind; i++)
            {
                crc = Crc16(data[i], crc);
                data[i] = Crypt(data[i], xor++);
            }
            data[ind++] = Crypt(crc.Byte(0), xor++);
            data[ind++] = Crypt(crc.Byte(1), xor);
            data[ind++] = 0xDC;
            data[ind++] = 0xBA;
            ind -= 8;
            data[2] = ind.Byte(0);
            data[3] = ind.Byte(1);
            SerialPort? sp = port;
            if (sp != null)
            {
                lock (sp)
                {
                    try { sp.Write(data, 0, ind + 8); } catch { }
                }
            }
        }
    }
}
