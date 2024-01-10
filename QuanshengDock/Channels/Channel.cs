using QuanshengDock.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace QuanshengDock.Channels
{
    // code by -nicsure- 2024
    // https://www.youtube.com/nicsure

    public partial class Channel
    {
        public void Clear()
        {
            Array.Clear(DataBytes, Offset, 16);
            Array.Clear(NameBytes, Offset, 16);
            AttrBytes[number] = 15;
        }

        public int Number => number;
        private int number;

        private int Offset => number * 16;

        public uint RxFrequency
        {
            get => BitConverter.ToUInt32(DataBytes, Offset);
            set
            {
                uint v = value <= 130000000 ? value : 130000000;
                Array.Copy(BitConverter.GetBytes(v), 0, DataBytes, Offset, 4);
                Band = v switch
                {
                    0 => 15,
                    uint when v < 10800000 => 0,
                    uint when v < 13700000 => 1,
                    uint when v < 17400000 => 2,
                    uint when v < 35000000 => 3,
                    uint when v < 40000000 => 4,
                    uint when v < 47000000 => 5,
                    _ => 6,
                };
            }
        }

        public uint TxOffset
        {
            get => BitConverter.ToUInt32(DataBytes, Offset + 4);
            set => Array.Copy(BitConverter.GetBytes(value), 0, DataBytes, Offset + 4, 4);
        }

        public int RxCode
        {
            get => DataBytes[Offset + 8];
            set => DataBytes[Offset + 8] = (byte)value;
        }

        public int TxCode
        {
            get => DataBytes[Offset + 9];
            set => DataBytes[Offset + 9] = (byte)value;
        }

        public int TxCodeType
        {
            get => DataBytes[Offset + 10] >> 4;
            set => DataBytes[Offset + 10] = (byte)((DataBytes[Offset + 10] & 0b00001111) | (value << 4));
        }

        public int RxCodeType
        {
            get => DataBytes[Offset + 10] & 0b00001111;
            set => DataBytes[Offset + 10] = (byte)((DataBytes[Offset + 10] & 0b11110000) | value);
        }

        public int Modulation
        {
            get => DataBytes[Offset + 11] >> 4;
            set => DataBytes[Offset + 11] = (byte)((DataBytes[Offset + 11] & 0b00001111) | (value << 4));
        }

        public int OffsetDir
        {
            get => DataBytes[Offset + 11] & 0b00001111;
            set => DataBytes[Offset + 11] = (byte)((DataBytes[Offset + 11] & 0b11110000) | value);
        }

        public int BusyLock
        {
            get => DataBytes[Offset + 12] >> 4;
            set => DataBytes[Offset + 12] = (byte)((DataBytes[Offset + 12] & 0b00001111) | (value << 4));
        }

        public int OutputPower
        {
            get => (DataBytes[Offset + 12] & 0b00001100) >> 2;
            set => DataBytes[Offset + 12] = (byte)((DataBytes[Offset + 12] & 0b11110011) | (value << 2));
        }

        public int Bandwidth
        {
            get => (DataBytes[Offset + 12] & 0b00000010) >> 1;
            set => DataBytes[Offset + 12] = (byte)((DataBytes[Offset + 12] & 0b11111101) | (value << 1));
        }

        public int Reverse
        {
            get => DataBytes[Offset + 12] & 0b00000001;
            set => DataBytes[Offset + 12] |= (byte)((DataBytes[Offset + 12] & 0b11111110) | value);
        }

        public int PttId
        {
            get => (DataBytes[Offset + 13] & 0b00001110) >> 1;
            set => DataBytes[Offset + 13] = (byte)((DataBytes[Offset + 13] & 0b11110001) | (value << 1));
        }

        public int Dtmf
        {
            get => DataBytes[Offset + 13] & 0b00000001;
            set => DataBytes[Offset + 13] |= (byte)((DataBytes[Offset + 13] & 0b11111110) | value);
        }

        public int Step
        {
            get => DataBytes[Offset + 14];
            set => DataBytes[Offset + 14] = (byte)value;
        }
        
        public int Scramble
        {
            get => DataBytes[Offset + 15];
            set => DataBytes[Offset + 15] = (byte)value;
        }

        public string Name
        {
            get => Encoding.ASCII.GetString(NameBytes, Offset, 10).Trim('\0');
            set
            {
                Array.Clear(NameBytes, Offset, 16);
                Array.Copy(Encoding.ASCII.GetBytes(value), 0, NameBytes, Offset, value.Length > 10 ? 10 : value.Length);
            }
        }

        public bool Scanlist1
        {
            get => AttrBytes[number] >= 128;
            set => AttrBytes[number] = (byte)((AttrBytes[number] & 0b01111111) | (value ? 128 : 0));
        }

        public bool Scanlist2
        {
            get => (AttrBytes[number] & 0b01000000) != 0;
            set => AttrBytes[number] = (byte)((AttrBytes[number] & 0b10111111) | (value ? 64 : 0));
        }

        public int Compander
        {
            get => (AttrBytes[number] & 0b00110000) >> 4;
            set => AttrBytes[number] = (byte)((AttrBytes[number] & 0b11001111) | (value << 4));
        }

        public int Band
        {
            get => AttrBytes[number] & 0b00001111;
            set => AttrBytes[number] = (byte)((AttrBytes[number] & 0b11110000) | value);
        }

        public double Rx
        {
            get => RxFrequency / 100000.0;
            set
            {
                RxFrequency = (uint)Math.Round(value * 100000.0);
                CheckTx();
            }
        }

        private void CheckTx()
        {
            if (Tx > 1300.0 || Tx <= 0.0)
            {
                OffsetDir = 0;
                TxOffset = 0;
            }
        }

        public double Tx
        {
            get => 
                (RxFrequency +
                (OffsetDir == 1 ? TxOffset :
                (OffsetDir == 2 ? -(int)TxOffset : 0))) / 100000.0;
            set
            {
                if (value <= 0.0)
                {
                    OffsetDir = 0;
                    TxOffset = 0;
                }
                else
                {
                    uint off = (uint)Math.Round(value * 100000.0);
                    long diff = off - RxFrequency;
                    OffsetDir = diff > 0 ? 1 : diff < 0 ? 2 : 0;
                    TxOffset = (uint)Math.Abs(diff);
                    CheckTx();
                }
            }
        }

        public int Scanlist
        {
            get => (Scanlist1 ? 1 : 0) | (Scanlist2 ? 2 : 0);
            set
            {
                Scanlist1 = (value & 1) != 0;
                Scanlist2 = (value & 2) != 0;
            }
        }
    }

    public class GridChannel : INotifyPropertyChanged, IComparable<GridChannel>
    {
        public string G => grouped ? "➤" : string.Empty;
        private bool grouped = false;
        private static readonly List<GridChannel> groupedChannels = new();
        private static readonly List<GridChannel> selectedChannels = new();
        public int Number => number + 1;
        private readonly int number;
        public event PropertyChangedEventHandler? PropertyChanged;
        private readonly Channel Base;
        private static bool groupChange = false;
        private DataGridRow? row = null;
        private static readonly Brush groupedBrush = new SolidColorBrush(Colors.DarkBlue);
        private static readonly Brush transBrush = new SolidColorBrush(Colors.Transparent);
        private static readonly Brush whiteBrush = new SolidColorBrush(Colors.White);
        private static readonly Brush greyBrush = new SolidColorBrush(Colors.DarkGray);

        public static List<GridChannel> GroupedChannels => groupedChannels;
        public static List<GridChannel> SelectedChannels => selectedChannels;

        public int CompareTo(GridChannel? other)
        {
            return number.CompareTo(other?.number);
        }

        public void SetRow(DataGridRow? row) => this.row = row;

        public void Group(bool set)
        {
            grouped = set;
            if (set)
            {
                if(!groupedChannels.Contains(this))
                    groupedChannels.Add(this);
            }
            else
                groupedChannels.Remove(this);
            DataGridRow? temp = row;
            if (temp != null)
                temp.Background = set ? groupedBrush : transBrush;
            groupedChannels.Sort();
            OnPropertyChanged(nameof(G));
        }
        public bool IsGrouped() => grouped;

        public virtual void OnPropertyChanged(string name)
        {
            (_ = PropertyChanged)?.Invoke(this, new(name));
            if(!groupChange && groupedChannels.Contains(this))
            {
                groupChange = true;
                bool first = groupedChannels[0] == this;
                int cnt = 1;
                double step = double.Parse(Step.ToString()[1..].Replace("kHz", "").Replace('_', '.')) / 1000.0;
                foreach (GridChannel channel in groupedChannels)
                {
                    if (channel == this) continue;
                    switch (name)
                    {
                        case "Name":
                            if (first && Name.EndsWith("1"))
                                channel.Name = $"{Name[..^1]}{cnt + 1}";
                            else
                                channel.Name = Name;
                            break;
                        case "RX": channel.RX = RX + (first ? step * cnt : 0.0); break;
                        case "TX": channel.Base.OffsetDir = Base.OffsetDir; channel.Base.TxOffset = Base.TxOffset; channel.OnPropertyChanged(nameof(TX)); break;
                        case "Bandwidth": channel.Bandwidth = Bandwidth; break;
                        case "Reverse": channel.Reverse = Reverse; break;
                        case "Power": channel.Power = Power; break;
                        case "RxTone": channel.RxTone = RxTone; break;
                        case "RxCTCSS": channel.RxCTCSS = RxCTCSS; break;
                        case "RxDCS": channel.RxDCS = RxDCS; break;
                        case "TxTone": channel.TxTone = TxTone; break;
                        case "TxCTCSS": channel.TxCTCSS = TxCTCSS; break;
                        case "TxDCS": channel.TxDCS = TxDCS; break;
                        case "Step": channel.Step = Step; break;
                        case "BusyLock": channel.BusyLock = BusyLock; break;
                        case "PttID": channel.PttID = PttID; break;
                        case "DTMF": channel.DTMF = DTMF; break;
                        case "Modulation": channel.Modulation = Modulation; break;
                        case "Scramble": channel.Scramble = Scramble; break;
                        case "Scanlists": channel.Scanlists = Scanlists; break;
                        case "Compand": channel.Compand = Compand; break;
                    }
                    cnt++;
                }
                groupChange = false;
            }
        }

        public GridChannel(int num)
        {
            number = num;
            Base = Channel.Get(num);
        }

        public string Name
        {
            get => Base.Name.Length == 0 || Base.Name.IndexOf('?') > -1 ? $"CH {Number:D3}" : Base.Name;
            set
            {
                Base.Name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public double RX
        {
            get => Base.Rx;
            set
            {
                Base.Rx = value;
                DataGridRow? temp = row;
                if (temp != null)
                    temp.Foreground = value == 0 ? greyBrush : whiteBrush;
                SetRowOpacity();
                OnPropertyChanged(nameof(RX));
                OnPropertyChanged(nameof(TX));
            }
        }

        public double TX
        {
            get => Base.Tx;
            set
            {
                Base.Tx = value;
                OnPropertyChanged(nameof(TX));
            }
        }

        public BANDWIDTH Bandwidth
        {
            get => (BANDWIDTH)Base.Bandwidth.Clamp(0, 1);
            set
            {
                Base.Bandwidth = (int)value;
                OnPropertyChanged(nameof(Bandwidth));
            }
        }

        public bool Reverse
        {
            get => Base.Reverse != 0;
            set
            {
                Base.Reverse = value ? 1 : 0;
                OnPropertyChanged(nameof(Reverse));
            }
        }

        public TX_POWER Power
        {
            get => (TX_POWER)Base.OutputPower.Clamp(0, 2);
            set
            {
                Base.OutputPower = (int)value;
                OnPropertyChanged(nameof(Power));
            }
        }

        public TONE_TYPE RxTone
        {
            get => (TONE_TYPE)Base.RxCodeType.Clamp(0, 3);
            set
            {
                Base.RxCodeType = (int)value;
                if (value == TONE_TYPE.CTCSS && Base.RxCode > (int)CTCSS_CODE._None)
                    Base.RxCode = (int)CTCSS_CODE._None;
                OnPropertyChanged(nameof(RxTone));
                OnPropertyChanged(nameof(RxCTCSS));
                OnPropertyChanged(nameof(RxDCS));
            }
        }

        public CTCSS_CODE RxCTCSS
        {
            get => RxTone == TONE_TYPE.CTCSS ? (CTCSS_CODE)Base.RxCode.Clamp(0, 50) : CTCSS_CODE._None;
            set
            {
                if(RxTone == TONE_TYPE.CTCSS)
                    Base.RxCode = (int)value;
                OnPropertyChanged(nameof(RxCTCSS));
            }
        }

        public DCS_CODE RxDCS
        {
            get => RxTone == TONE_TYPE.DCS || RxTone == TONE_TYPE.ReverseDCS ? (DCS_CODE)Base.RxCode.Clamp(0, 104) : DCS_CODE._None;
            set
            {
                if (RxTone == TONE_TYPE.DCS || RxTone == TONE_TYPE.ReverseDCS)
                    Base.RxCode = (int)value;
                OnPropertyChanged(nameof(RxDCS));
            }
        }

        public TONE_TYPE TxTone
        {
            get => (TONE_TYPE)Base.TxCodeType.Clamp(0, 3);
            set
            {
                Base.TxCodeType = (int)value;
                if (value == TONE_TYPE.CTCSS && Base.TxCode > (int)CTCSS_CODE._None)
                    Base.TxCode = (int)CTCSS_CODE._None;
                OnPropertyChanged(nameof(TxTone));
                OnPropertyChanged(nameof(TxCTCSS));
                OnPropertyChanged(nameof(TxDCS));
            }
        }

        public CTCSS_CODE TxCTCSS
        {
            get => TxTone == TONE_TYPE.CTCSS ? (CTCSS_CODE)Base.TxCode.Clamp(0, 50) : CTCSS_CODE._None;
            set
            {
                if (TxTone == TONE_TYPE.CTCSS)
                    Base.TxCode = (int)value;
                OnPropertyChanged(nameof(TxCTCSS));
            }
        }

        public DCS_CODE TxDCS
        {
            get => TxTone == TONE_TYPE.DCS || TxTone == TONE_TYPE.ReverseDCS ? (DCS_CODE)Base.TxCode.Clamp(0, 104) : DCS_CODE._None;
            set
            {
                if (TxTone == TONE_TYPE.DCS || TxTone == TONE_TYPE.ReverseDCS)
                    Base.TxCode = (int)value;
                OnPropertyChanged(nameof(TxDCS));
            }
        }

        public FQ_STEP Step
        {
            get => (FQ_STEP)Base.Step.Clamp(0, 20);
            set
            {
                Base.Step = (int)value;
                OnPropertyChanged(nameof(Step));
            }
        }

        public bool BusyLock
        {
            get => Base.BusyLock != 0;
            set
            {
                Base.BusyLock = value ? 1 : 0;
                OnPropertyChanged(nameof(BusyLock));
            }
        }

        public PTT_ID PttID
        {
            get => (PTT_ID)Base.PttId.Clamp(0, 4);
            set
            {
                Base.PttId = (int)value;
                OnPropertyChanged(nameof(PttID));
            }
        }

        public bool DTMF
        {
            get => Base.Dtmf != 0;
            set
            {
                Base.Dtmf = value ? 1 : 0;
                OnPropertyChanged(nameof(DTMF));
            }
        }

        public MODULATION Modulation
        {
            get => (MODULATION)Base.Modulation.Clamp(0, 2);
            set
            {
                Base.Modulation = (int)value;
                OnPropertyChanged(nameof(Modulation));
            }
        }

        public SCRAMBLER Scramble
        {
            get => (SCRAMBLER)Base.Scramble.Clamp(0, 10);
            set
            {
                Base.Scramble = (int)value;
                OnPropertyChanged(nameof(Scramble));
            }
        }

        public SCANLISTS Scanlists
        {
            get => (SCANLISTS)Base.Scanlist.Clamp(0, 3);
            set
            {
                Base.Scanlist = (int)value;
                OnPropertyChanged(nameof(Scanlists));
            }
        }

        public COMPANDER Compand
        {
            get => (COMPANDER)Base.Compander.Clamp(0, 3);
            set
            {
                Base.Compander = (int)value;
                OnPropertyChanged(nameof(Compand));
            }
        }

        public bool IsInUse() => Base.Band <= 6;

        public void SetRowOpacity()
        {
            if (row != null)
                row.Opacity = IsInUse() ? 1.0 : 0.6;
        }

    }


}
