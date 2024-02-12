using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace QuanshengDock.Channels
{
    // code by -nicsure- 2024
    // https://www.youtube.com/nicsure

    internal class Conversions { }

    public enum CTCSS_CODE
    {
        _67_0, _69_3, _71_9, _74_4, _77_0, _79_7, _82_5, _85_4, _88_5, _91_5,
        _94_8, _97_4, _100_0, _103_5, _107_2, _110_9, _114_8, _118_8, _123_0, _127_3,
        _131_8, _136_5, _141_3, _146_2, _151_4, _156_7, _159_8, _162_2, _165_5, _167_9,
        _171_3, _173_8, _177_3, _179_9, _183_5, _186_2, _189_9, _192_8, _196_6, _199_5,
        _203_5, _206_5, _210_7, _218_1, _225_7, _229_1, _233_6, _241_8, _250_3, _254_1, _None
    }

    public enum DCS_CODE
    {
        _0013, _0015, _0016, _0019, _001A, _001E, _0023, _0027,
        _0029, _002B, _002C, _0035, _0039, _003A, _003B, _003C,
        _004C, _004D, _004E, _0052, _0055, _0059, _005A, _005C,
        _0063, _0065, _006A, _006D, _006E, _0072, _0075, _007A,
        _007C, _0085, _008A, _0093, _0095, _0096, _00A3, _00A4,
        _00A5, _00A6, _00A9, _00AA, _00AD, _00B1, _00B3, _00B5,
        _00B6, _00B9, _00BC, _00C6, _00C9, _00CD, _00D5, _00D9,
        _00DA, _00E3, _00E6, _00E9, _00EE, _00F4, _00F5, _00F9,
        _0109, _010A, _010B, _0113, _0119, _011A, _0125, _0126,
        _012A, _012C, _012D, _0132, _0134, _0135, _0136, _0143,
        _0146, _014E, _0153, _0156, _015A, _0166, _0175, _0186,
        _018A, _0194, _0197, _0199, _019A, _01AC, _01B2, _01B4,
        _01C3, _01CA, _01D3, _01D9, _01DA, _01DC, _01E3, _01EC, _None
    }

    public enum FQ_STEP
    {
        _2_5kHz,
        _5kHz,
        _6_25kHz,
        _10kHz,
        _12_5kHz,
        _25kHz,
        _8_33kHz,
        _0_01kHz,
        _0_05kHz,
        _0_1kHz,
        _0_25kHz,
        _0_5kHz,
        _1kHz,
        _1_25kHz,
        _15kHz,
        _30kHz,
        _50kHz,
        _100kHz,
        _125kHz,
        _250kHz,
        _500kHz
    }

    public enum SCRAMBLER
    {
        _Off,
        _2600Hz,
        _2700Hz,
        _2800Hz,
        _2900Hz,
        _3000Hz,
        _3100Hz,
        _3200Hz,
        _3300Hz,
        _3400Hz,
        _3500Hz
    }
    public enum BANDWIDTH { Wide, Narrow }
    public enum TX_POWER { Low, Mid, High }
    public enum TONE_TYPE { None, CTCSS, DCS, ReverseDCS }
    public enum PTT_ID { Off, Begin, End, Both, Apollo }
    public enum MODULATION { FM, AM, USB }
    public enum SCANLISTS { None, One, Two, Both }
    public enum COMPANDER { Off, Tx, Rx, Both }

    public static class Beautifiers
    {
        static Beautifiers()
        {
            int i;
            for (i = 0; i < DcsOptions.Length; i++)
                DcsStrings[i] = $"D{Convert.ToString(DcsOptions[i], 8).PadLeft(3, '0')}";
            DcsStrings[i] = "None";
            for (i = 0; i < CtcssOptions.Length; i++)
                CtcssStrings[i] = $"{(CtcssOptions[i]/10.0):F1}";
            CtcssStrings[i] = "None";            
        }
        public static uint[] CtcssOptions { get; } = new uint[]
        {
             670,  693,  719,  744,  770,  797,  825,  854,  885,  915,
             948,  974, 1000, 1035, 1072, 1109, 1148, 1188, 1230, 1273,
            1318, 1365, 1413, 1462, 1514, 1567, 1598, 1622, 1655, 1679,
            1713, 1738, 1773, 1799, 1835, 1862, 1899, 1928, 1966, 1995,
            2035, 2065, 2107, 2181, 2257, 2291, 2336, 2418, 2503, 2541
        };
        public static string[] CtcssStrings { get; } = new string[51];
        public static string[] DcsStrings { get; private set; } = new string[105];
        public static uint[] DcsOptions { get; } = new uint[]
        {
            0x0013, 0x0015, 0x0016, 0x0019, 0x001A, 0x001E, 0x0023, 0x0027,
            0x0029, 0x002B, 0x002C, 0x0035, 0x0039, 0x003A, 0x003B, 0x003C,
            0x004C, 0x004D, 0x004E, 0x0052, 0x0055, 0x0059, 0x005A, 0x005C,
            0x0063, 0x0065, 0x006A, 0x006D, 0x006E, 0x0072, 0x0075, 0x007A,
            0x007C, 0x0085, 0x008A, 0x0093, 0x0095, 0x0096, 0x00A3, 0x00A4,
            0x00A5, 0x00A6, 0x00A9, 0x00AA, 0x00AD, 0x00B1, 0x00B3, 0x00B5,
            0x00B6, 0x00B9, 0x00BC, 0x00C6, 0x00C9, 0x00CD, 0x00D5, 0x00D9,
            0x00DA, 0x00E3, 0x00E6, 0x00E9, 0x00EE, 0x00F4, 0x00F5, 0x00F9,
            0x0109, 0x010A, 0x010B, 0x0113, 0x0119, 0x011A, 0x0125, 0x0126,
            0x012A, 0x012C, 0x012D, 0x0132, 0x0134, 0x0135, 0x0136, 0x0143,
            0x0146, 0x014E, 0x0153, 0x0156, 0x015A, 0x0166, 0x0175, 0x0186,
            0x018A, 0x0194, 0x0197, 0x0199, 0x019A, 0x01AC, 0x01B2, 0x01B4,
            0x01C3, 0x01CA, 0x01D3, 0x01D9, 0x01DA, 0x01DC, 0x01E3, 0x01EC,
        };

        public static uint ClosestCtcss(uint u)
        {
            int i = Array.BinarySearch(CtcssOptions, u);
            if (i >= 0) return CtcssOptions[i];
            i = ~i;
            if (i <= 0) return CtcssOptions[0];
            if (i >= 50) return CtcssOptions[49];
            if (CtcssOptions[i] - u > u - CtcssOptions[i - 1]) i--;
            return CtcssOptions[i];
        }

        private static readonly string[] StepStrings = new string[]
        {
            "2.5 kHz", "5 kHz", "6.25 kHz", "10 kHz", "12.5 kHz", "25 kHz", "8.33 kHz", "0.01 kHz", "0.05 kHz", "0.1 kHz",
            "0.25 kHz", "0.5 kHz", "1 kHz", "1.25 kHz", "15 kHz", "30 kHz", "50 kHz", "100 kHz", "125 kHz", "250 kHz", "500 kHz"
        };
        private static readonly string[] ScrambleStrings = new string[]
        {
            "Off", "2600 Hz", "2700 Hz", "2800 Hz","2900 Hz", "3000 Hz", "3100 Hz",
            "3200 Hz", "3300 Hz", "3400 Hz", "3500 Hz"
        };
        public static EnumBeautifier Ctcss { get; } = new(CtcssStrings, typeof(CTCSS_CODE));
        public static EnumBeautifier Dcs { get; } = new(DcsStrings, typeof(DCS_CODE));
        public static EnumBeautifier Step { get; } = new(StepStrings, typeof(FQ_STEP));
        public static EnumBeautifier Scramble { get; } = new(ScrambleStrings, typeof(SCRAMBLER));
    }

    public class EnumBeautifier : IValueConverter
    {
        public string[] Strings { get; private set; }
        private readonly Type enumType;
        public EnumBeautifier(string[] strings, Type enumType)
        {
            Strings = strings;
            this.enumType = enumType;
        }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Strings[(int)value];
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Enum.ToObject(enumType, Array.IndexOf(Strings, value));
        }
    }

}
