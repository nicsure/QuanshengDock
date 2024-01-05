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
    }
}
