using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanshengDock
{
    public static class ClassExtensions
    {
        public static byte Byte(this ushort s, int byteIndex) => (byte)((s >> (byteIndex * 8)) & 0xff);
        public static byte Byte(this short s, int byteIndex) => (byte)((s >> (byteIndex * 8)) & 0xff);
        public static byte Byte(this int s, int byteIndex) => (byte)((s >> (byteIndex * 8)) & 0xff);
        public static byte Byte(this uint s, int byteIndex) => (byte)((s >> (byteIndex * 8)) & 0xff);
        public static double ToDouble(this string s) => double.TryParse(s, out double d) ? d : 0;
        public static float Clamp(this float value, float min, float max) => Math.Max(min, Math.Min(value, max));
        public static double Clamp(this double value, double min, double max) => Math.Max(min, Math.Min(value, max));
        public static int Clamp(this int value, int min, int max) => value < min ? min : value > max ? max : value;


        public static bool DoubleParse(this string s, out double d)
        {
            if(!double.TryParse(s, out d))
            {
                if(!double.TryParse(s.Replace(",", "."), out d))
                {
                    if (!double.TryParse(s.Replace(".", ","), out d))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
