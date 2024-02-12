using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;

namespace QuanshengDock.ExtendedVFO
{
    public static class Defines 
    {
        public static (int A, int B)[] DTMF { get; } = {
            (941, 1336), // 0
            (697, 1209), // 1
            (697, 1336), // 2
            (697, 1477), // 3
            (770, 1209), // 4
            (770, 1336), // 5
            (770, 1477), // 6
            (852, 1209), // 7
            (852, 1336), // 8
            (852, 1477), // 9
            (697, 1633), // A
            (770, 1633), // B
            (852, 1633), // C
            (941, 1633), // D
            (941, 1209), // *
            (941, 1477) // #
        };


        public static int[] ItoN { get; } = new int[] 
        {
            7,39,76,90,8,31,70,0,
            4,72,66,49,51,40,80,32,
            97,26,103,36,62,61,85,35,
            65,50,17,99,47,79,42,5,
            15,46,60,23,19,64,59,1,
            13,82,30,75,71,100,33,28,
            73,11,25,12,95,67,83,77,
            74,84,88,38,34,21,20,101,
            37,24,10,53,98,81,6,44,
            9,48,56,43,2,55,94,29,
            14,69,41,54,57,22,96,91,
            58,92,3,87,89,102,78,52,
            86,16,68,27,45,63,93,18,
        };

        public static uint[] Golays { get; } = new uint[]
        {
            0x763813, 0x6B7815, 0x65D816, 0x51F819, 0x5F581A, 0x0BE81E, 0x5B6823, 0x0FD827,
            0x7CA829, 0x35582B, 0x6F482C, 0x5D1835, 0x679839, 0x69383A, 0x2E683B, 0x74783C,
            0x35E84C, 0x72B84D, 0x7C184E, 0x5DA852, 0x07B855, 0x3D3859, 0x33985A, 0x2ED85C,
            0x37A863, 0x2AE865, 0x1EC86A, 0x44D86D, 0x4A786E, 0x6BC872, 0x31D875, 0x05F87A, 
            0x18B87C, 0x6E9885, 0x5AB88A, 0x68E893, 0x75A895, 0x7B0896, 0x45B8A3, 0x1FA8A4,
            0x58F8A5, 0x5658A6, 0x6278A9, 0x6CD8AA, 0x36C8AD, 0x1778B1, 0x5E88B3, 0x43C8B5, 
            0x4D68B6, 0x7948B9, 0x6AA8BC, 0x0CF8C6, 0x38D8C9, 0x6C68CD, 0x1968D5, 0x23E8D9,
            0x2D48DA, 0x2978E3, 0x3A98E6, 0x0EB8E9, 0x54A8EE, 0x6858F4, 0x2F08F5, 0x1588F9,
            0x776909, 0x79C90A, 0x3E990B, 0x4B9913, 0x6C5919, 0x62F91A, 0x7B8925, 0x752926, 
            0x4FA92A, 0x52E92C, 0x15B92D, 0x3AA932, 0x27E934, 0x60B935, 0x6E1936, 0x3C6943, 
            0x2F8946, 0x41B94E, 0x275953, 0x34B956, 0x0E395A, 0x19E966, 0x0C7975, 0x5D9986,
            0x67198A, 0x0F5994, 0x01F997, 0x728999, 0x7C299A, 0x4C39AC, 0x2479B2, 0x3939B4, 
            0x22B9C3, 0x0BD9CA, 0x3989D3, 0x1E49D9, 0x10E9DA, 0x0DA9DC, 0x14D9E3, 0x20F9EC,
        };

        public static uint[] GolaysR { get; } = new uint[]
        {
            0x09C7EC, 0x1487EA, 0x1A27E9, 0x2E07E6, 0x20A7E5, 0x7417E1, 0x2497DC, 0x7027D8,
            0x0357D6, 0x4AA7D4, 0x10B7D3, 0x22E7CA, 0x1867C6, 0x16C7C5, 0x5197C4, 0x0B87C3,
            0x4A17B3, 0x0D47B2, 0x03E7B1, 0x2257AD, 0x7847AA, 0x42C7A6, 0x4C67A5, 0x5127A3,
            0x48579C, 0x55179A, 0x613795, 0x3B2792, 0x358791, 0x14378D, 0x4E278A, 0x7A0785,
            0x674783, 0x11677A, 0x254775, 0x17176C, 0x0A576A, 0x04F769, 0x3A475C, 0x60575B,
            0x27075A, 0x29A759, 0x1D8756, 0x132755, 0x493752, 0x68874E, 0x21774C, 0x3C374A,
            0x329749, 0x06B746, 0x155743, 0x730739, 0x472736, 0x139732, 0x66972A, 0x5C1726,
            0x52B725, 0x56871C, 0x456719, 0x714716, 0x2B5711, 0x17A70B, 0x50F70A, 0x6A7706, 
            0x0896F6, 0x0636F5, 0x4166F4, 0x3466EC, 0x13A6E6, 0x1D06E5, 0x0476DA, 0x0AD6D9,
            0x3056D5, 0x2D16D3, 0x6A46D2, 0x4556CD, 0x5816CB, 0x1F46CA, 0x11E6C9, 0x4396BC, 
            0x5076B9, 0x3E46B1, 0x58A6AC, 0x4B46A9, 0x71C6A5, 0x661699, 0x73868A, 0x226679,
            0x18E675, 0x70A66B, 0x7E0668, 0x0D7666, 0x03D665, 0x33C653, 0x5B864D, 0x46C64B,
            0x5D463C, 0x742635, 0x46762C, 0x61B626, 0x6F1625, 0x725623, 0x6B261C, 0x5F0613,
        };


        public static double[] StepValues { get; } = new double[]
        {
            0.00001 ,
            0.00005 ,
            0.00010 ,
            0.00025 ,
            0.00050 ,
            0.00100 ,
            0.00125 ,
            0.00250 ,
            0.00500 ,
            0.00625 ,
            0.00833 ,
            0.00900 ,
            0.01000 ,
            0.01250 ,
            0.01500 ,
            0.02000 ,
            0.02500 ,
            0.03000 ,
            0.05000 ,
            0.10000 ,
            0.12500 ,
            0.20000 ,
            0.25000 ,
            0.50000 ,
        };



        public static string[] StepNames { get; } = new string[]
        {
            "0.01k",
            "0.05k",
            "0.10k",
            "0.25k",
            "0.5k",
            "1k",
            "1.25k",
            "2.5k",
            "5k",
            "6.25k",
            "8.33k",
            "9k",
            "10k",
            "12.5k",
            "15k",
            "20k",
            "25k",
            "30k",
            "50k",
            "100k",
            "125k",
            "200k",
            "250k",
            "500k",
        };
    }

    public enum XCOMPANDER
    {
        OFF,
        RX,
        TX,
        BOTH,
    }

    public enum XTONETYPE
    {
        NONE,
        CTCSS,
        DCS,
        RDCS
    }

    public enum XTXPOWER
    {
        LOW,
        MID,
        HIGH
    }


    public enum XBANDWIDTH
    {
        WIDE = 18856,
        NRRW = 18440,
        THIN = 13912,
        UWIDE = 32620,
        ULOW = 88
    }

    public enum GPIOA_PINS
    {
        GPIOA_PIN_0 = 0,
        GPIOA_PIN_1 = 1,
        GPIOA_PIN_2 = 2,
        GPIOA_PIN_3 = 3,
        GPIOA_PIN_4 = 4,
        GPIOA_PIN_5 = 5,
        GPIOA_PIN_6 = 6,
        GPIOA_PIN_7 = 7,
        GPIOA_PIN_8 = 8,
        GPIOA_PIN_9 = 9,
        #pragma warning disable CA1069
        GPIOA_PIN_I2C_SCL = 10, // Shared with keyboard!
        GPIOA_PIN_I2C_SDA = 11, // Shared with keyboard!
        GPIOA_PIN_VOICE_0 = 12, // Shared with keyboard!
        GPIOA_PIN_VOICE_1 = 13, // Shared with keyboard!
        GPIOA_PIN_KEYBOARD_4 = 10, // Shared with keyboard!
        GPIOA_PIN_KEYBOARD_5 = 11, // Shared with keyboard!
        GPIOA_PIN_KEYBOARD_6 = 12, // Shared with keyboard!
        GPIOA_PIN_KEYBOARD_7 = 13, // Shared with keyboard!
        #pragma warning restore CA1069
        GPIOA_PIN_14 = 14,
        GPIOA_PIN_15 = 15,
    };

    public enum GPIOB_PINS
    {
        GPIOB_PIN_BACKLIGHT = 6,

        GPIOB_PIN_ST7565_A0 = 9,
        GPIOB_PIN_ST7565_RES = 11, // Shared with SWD!

        #pragma warning disable CA1069 // Enums values should not be duplicated - YEAH? SUE ME!
        GPIOB_PIN_SWD_IO = 11, // Shared with ST7565!
        #pragma warning restore CA1069 // Enums values should not be duplicated
        GPIOB_PIN_SWD_CLK = 14,

        GPIOB_PIN_BK1080 = 15
    };

    public enum GPIOC_PINS
    {
        GPIOC_PIN_BK4819_SCN = 0,
        GPIOC_PIN_BK4819_SCL = 1,
        GPIOC_PIN_BK4819_SDA = 2,

        GPIOC_PIN_FLASHLIGHT = 3,
        GPIOC_PIN_AUDIO_PATH = 4,
        GPIOC_PIN_PTT = 5
    };

}
