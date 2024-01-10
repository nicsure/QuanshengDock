using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanshengDock.Channels
{
    public class CopiedChannel
    {
        public static CopiedChannel[] Clipboard { get; set; } = Array.Empty<CopiedChannel>();

        private readonly byte[] data = new byte[16];
        private readonly byte[] name = new byte[16];
        private readonly byte attr;
        public CopiedChannel(int num) 
        {
            Array.Copy(Channel.DataBytes, num * 16, data, 0, 16);
            Array.Copy(Channel.NameBytes, num * 16, name, 0, 16);
            attr = Channel.AttrBytes[num];
        }

        public CopiedChannel(string bookname, string tx, string rx, string code)
        {
            byte[] b = Encoding.ASCII.GetBytes(bookname);
            Array.Copy(b, 0, name, 0, b.Length > 10 ? 10 : b.Length);
            double rxf = double.TryParse(rx, out double f) ? f : 0;
            double txf = double.TryParse(tx, out f) ? f : 0;
            uint rxi = (uint)Math.Round(rxf * 100000.0);
            uint txi = (uint)Math.Round(txf * 100000.0);
            int txd = txi > rxi ? 1 : txi < rxi ? 2 : 0;
            txi = txi > rxi ? txi - rxi : rxi - txi;
            int codeindex = Array.IndexOf(Beautifiers.CtcssStrings, code);
            int codetype = 0;
            if (codeindex == -1)
            {
                codeindex = Array.IndexOf(Beautifiers.DcsStrings, code);
                if (codeindex > -1)
                    codetype = 2;
            }
            else
                codetype = 1;
            if (codeindex == -1) codeindex = 0;
            Array.Copy(BitConverter.GetBytes(rxi), 0, data, 0, 4);
            Array.Copy(BitConverter.GetBytes(txi), 0, data, 4, 4);
            data[9] = (byte)codeindex;
            data[10] = (byte)(codetype << 4);
            data[11] = (byte)txd;
        }

        public void Paste(int num)
        {
            Array.Copy(data, 0, Channel.DataBytes, num * 16, 16);
            Array.Copy(name, 0, Channel.NameBytes, num * 16, 16);
            Channel.AttrBytes[num] = attr;
        }



    }
}
