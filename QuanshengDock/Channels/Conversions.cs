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
        public static string[] CtcssStrings { get; } = new string[]
        {
             "67.0",  "69.3",  "71.9",  "74.4",  "77.0",  "79.7",  "82.5",  "85.4",  "88.5",  "91.5",
             "94.8",  "97.4", "100.0", "103.5", "107.2", "110.9", "114.8", "118.8", "123.0", "127.3",
            "131.8", "136.5", "141.3", "146.2", "151.4", "156.7", "159.8", "162.2", "165.5", "167.9",
            "171.3", "173.8", "177.3", "179.9", "183.5", "186.2", "189.9", "192.8", "196.6", "199.5",
            "203.5", "206.5", "210.7", "218.1", "225.7", "229.1", "233.6", "241.8", "250.3", "254.1", "None"
        };
        public static string[] DcsStrings { get; } = new string[]
        {
            "0019", "0021", "0022", "0025", "0026", "0030", "0035", "0039",
            "0041", "0043", "0044", "0053", "0057", "0058", "0059", "0060",
            "0076", "0077", "0078", "0082", "0085", "0089", "0090", "0092",
            "0099", "0101", "0106", "0109", "0110", "0114", "0117", "0122",
            "0124", "0133", "0138", "0147", "0149", "0150", "0163", "0164",
            "0165", "0166", "0169", "0170", "0173", "0177", "0179", "0181",
            "0182", "0185", "0188", "0198", "0201", "0205", "0213", "0217",
            "0218", "0227", "0230", "0233", "0238", "0244", "0245", "0249",
            "0265", "0266", "0267", "0275", "0281", "0282", "0293", "0294",
            "0298", "0300", "0301", "0306", "0308", "0309", "0310", "0323",
            "0326", "0334", "0339", "0342", "0346", "0358", "0373", "0390",
            "0394", "0407", "0409", "0411", "0412", "0428", "0434", "0436",
            "0451", "0458", "0467", "0473", "0474", "0476", "0483", "0492", "None"
        };
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
