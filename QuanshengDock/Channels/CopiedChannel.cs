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

        public void Paste(int num)
        {
            Array.Copy(data, 0, Channel.DataBytes, num * 16, 16);
            Array.Copy(name, 0, Channel.NameBytes, num * 16, 16);
            Channel.AttrBytes[num] = attr;
        }



    }
}
