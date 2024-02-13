using QuanshengDock.Channels;
using QuanshengDock.ExtendedVFO;
using QuanshengDock.General;
using QuanshengDock.User;
using QuanshengDock.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Serialization;

namespace QuanshengDock.UI
{
    /// <summary>
    /// Interaction logic for VFOPreset.xaml
    /// </summary>
    public partial class VFOPreset : UserControl, INotifyPropertyChanged
    {
        public static int Activator { get => 0; set { } }
        private static readonly ViewModel<double> rxFreq = VM.Get<double>("XVfoRxFreq");
        private static readonly ViewModel<double> txFreq = VM.Get<double>("XVfoTxFreq");
        private static readonly ViewModel<ushort> vfoMode = VM.Get<ushort>("XVfoMode");
        private static readonly ViewModel<XBANDWIDTH> bandwidth = VM.Get<XBANDWIDTH>("XVfoBandwidth");
        private static readonly ViewModel<int> vfoStep = VM.Get<int>("XVfoStep");
        private static readonly ViewModel<bool> txisRX = VM.Get<bool>("TXisRX");
        private static readonly ViewModel<XTONETYPE> toneType = VM.Get<XTONETYPE>("XVfoToneType");
        private static readonly ViewModel<int> vfoCtcss = VM.Get<int>("XVfoCTCSS");
        private static readonly ViewModel<int> vfoDcs = VM.Get<int>("XVfoDCS");
        private static readonly ViewModel<XTONETYPE> rxtoneType = VM.Get<XTONETYPE>("RXVfoToneType");
        private static readonly ViewModel<int> rxvfoCtcss = VM.Get<int>("RXVfoCTCSS");
        private static readonly ViewModel<int> rxvfoDcs = VM.Get<int>("RXVfoDCS");
        private static readonly ViewModel<double> power = VM.Get<double>("XVfoPower");
        private static readonly ViewModel<XCOMPANDER> compander = VM.Get<XCOMPANDER>("XVfoCompander");
        private static readonly ViewModel<double> squelch = VM.Get<double>("Squelch");
        private static readonly ViewModel<int> autoSquelch = VM.Get<int>("AutoSquelch");
        private static readonly ViewModel<ObservableCollection<VFOPreset>> presets = VM.Get<ObservableCollection<VFOPreset>>("VFOPresets");
        private static readonly ViewModel<string> input = VM.Get<string>("VFOPresetInput");
        private static readonly ViewModel<string> selected = VM.Get<string>("SelectedPreset");
        private static readonly ViewModel<string> xvfolcd = VM.Get<string>("XVFOLCD");
        private static readonly ViewModel<bool> lockPower = VM.Get<bool>("LockPower");
        public static Dictionary<ulong, VFOPreset> Store { get; } = new();
        public static VFOPreset? MouseWasOver { get; private set; } = null;
        public static VFOPreset? MenuSelected { get; private set; } = null;


        public static VFOPreset[] VFOs { get; } = new VFOPreset[4];
        private static readonly SavedList saved = new(UserFolder.File("vfopresets.conf"));
        private static readonly SavedList savedvfos = new(UserFolder.File("vfos.conf"));
        private static int lastPresetIndex = -1;
        public static VFOPreset CurrentVFO { get; set; }
        public static VFOPreset ScanVFO { get; private set; }


        private static readonly Brush scanBrush = new SolidColorBrush(Color.FromArgb(255, 0, 50, 0));
        static VFOPreset()
        {
            for (int i = 0; i < 4; i++)
            {
                if (savedvfos.Count > i)
                    VFOs[i] = FromString(savedvfos[i]);
                else
                    VFOs[i] = new VFOPreset();
                VFOs[i].MainVFO = true;
                VFOs[i].PName = $"{(char)(i + 65)}";
            }
            foreach (string s in saved)
            {
                var preset = FromString(s);
                Add(preset);
            }
            CurrentVFO = VFOs[0];
            ScanVFO = new() { PName = "S", MainVFO = true };
        }

        private static void Add(VFOPreset preset)
        {
            presets.Value.Add(preset);
        }

        private static void Remove(VFOPreset? preset)
        {
            if (preset != null)
            { 
                if(presets.Value.Contains(preset))
                    presets.Value.Remove(preset);
                if(Store.ContainsKey(preset.id))
                    Store.Remove(preset.id);
            }
        }

        public static void Save()
        {
            saved.Clear();
            foreach(var preset in presets.Value)
                saved.Add(Serialize(preset));
            saved.Save();
            savedvfos.Clear();
            if(Radio.UsedXVFO)
                CurrentVFO.Set();
            for (int i = 0; i < 4; i++)
                savedvfos.Add(Serialize(VFOs[i]));
            savedvfos.Save();
        }

        public static void MenuOpened()
        {
            MenuSelected = MouseWasOver;
            MenuSelected?.Highlight(false);
        }

        public static void MenuClosed()
        {
            Unhighlight();
        }

        public static void ToggleMainVFO(int i = -1)
        {
            if (i == -1)
                i = Array.IndexOf(VFOs, CurrentVFO) + 1;
            i &= 3;
            VFOs[i].Recall();
        }

        public static void StepPreset(int dir)
        {
            int i = lastPresetIndex + dir;
            if (i < 0) i = presets.Value.Count - 1;
            if (i >= presets.Value.Count) i = 0;
            lastPresetIndex = i;
            if(i>=0 && i<presets.Value.Count)
                presets.Value[i].Recall();
        }

        public static void MoveUp()
        {
            if (MenuSelected != null && presets.Value.Contains(MenuSelected) && presets.Value.Count > 1)
            {
                int i = presets.Value.IndexOf(MenuSelected);
                if (i > 0)
                {
                    Delete();
                    presets.Value.Insert(i - 1, MenuSelected);
                }
            }
        }

        public static void MoveDown()
        {
            if (MenuSelected != null && presets.Value.Contains(MenuSelected) && presets.Value.Count > 1)
            {
                int i = presets.Value.IndexOf(MenuSelected);
                if (i < presets.Value.Count - 2)
                {
                    Delete();
                    presets.Value.Insert(i + 1, MenuSelected);
                }
            }
        }

        public static void Delete()
        {
            Remove(MenuSelected);
        }

        public static void Rename()
        {
            if (MenuSelected != null && input.Value.Length > 0)
            {
                MenuSelected.PName = input.Value;
                input.Value = string.Empty;
            }
        }

        public static void CreateNew()
        {
            VFOPreset preset = new() { PName = input.Value };
            ID(preset);
            if (input.Value.Length > 0)
            {
                Add(preset);
                input.Value = string.Empty;
            }
        }

        private static void ID(VFOPreset preset)
        {
            if (preset.id == 0)
            {
                ulong id;
                do
                {
                    id = (ulong)((RandomNumberGenerator.GetInt32(int.MaxValue) | (RandomNumberGenerator.GetInt32(int.MaxValue)<<32)));
                }
                while (id == 0 || Store.ContainsKey(id));
                preset.id = id;
                Store[id] = preset;
            }
            else
            {
                if(!Store.ContainsKey(preset.id))
                    Store[preset.id] = preset;
            }
        }

        private static string Serialize(VFOPreset p)
        {
            return
                $"{p.tx2rx},{p.rx},{p.tx},{p.pwr},{p.sql},{p.mode},{p.step},{p.ctcss}," +
                $"{p.dcs},{p.bw},{p.tone},{p.comp},{p.PName.Replace(',','.')},{p.asql},{p.rxctcss}," +
                $"{p.rxdcs},{p.rxtone},{p.id},{p.LastPreset}";
        }

        private static VFOPreset FromString(string s)
        {
            VFOPreset v = new();
            string[] p = s.Split(',');
            if (p.Length < 19)
            {
                string[] t = new string[19];
                Array.Fill(t, string.Empty);
                p = p.Concat(t).ToArray();
            }
            _ = bool.TryParse(p[0], out v.tx2rx);
            _ = p[1].DoubleParse(out v.rx);
            _ = p[2].DoubleParse(out v.tx);
            _ = p[3].DoubleParse(out v.pwr);
            _ = p[4].DoubleParse(out v.sql);
            _ = ushort.TryParse(p[5], out v.mode);
            _ = int.TryParse(p[6], out v.step);
            _ = int.TryParse(p[7], out v.ctcss);
            _ = int.TryParse(p[8], out v.dcs);
            _ = int.TryParse(p[9], out v.bw);
            _ = int.TryParse(p[10], out v.tone);
            _ = int.TryParse(p[11], out v.comp);
            v.PName = p[12];
            _ = int.TryParse(p[13], out v.asql);
            _ = int.TryParse(p[14], out v.rxctcss);
            _ = int.TryParse(p[15], out v.rxdcs);
            _ = int.TryParse(p[16], out v.rxtone);
            _ = ulong.TryParse(p[17], out v.id);
            v.LastPreset = p[18];
            v.RXFreq = v.rx.ToString("F5");
            v.TXFreq = v.tx.ToString("F5");
            ID(v);
            return v;
        }

        public static void Import(bool all)
        {
            var names = presets.Value.Select(c => c.PName).ToList();
            foreach(var channel in Channel.Channels)
            {
                if (channel.Rx < 1 || channel.Name.Length == 0) continue;
                VFOPreset? preset = null;
                if(all)
                {
                    if(!names.Contains(channel.Name))
                    {
                        preset = new() { PName = channel.Name };
                        Add(preset);
                    }
                }
                else
                {
                    if (names.Contains(channel.Name))
                    {
                        var named = presets.Value.Where(c => c.PName == channel.Name).ToList();
                        if (named.Count == 1)
                        {
                            preset = named[0];
                        }
                    }
                }
                if(preset != null) 
                {
                    preset.tx2rx = channel.Tx.ToString("F5") == channel.Rx.ToString("F5");
                    preset.rx = channel.Rx;
                    preset.RXFreq = preset.rx.ToString("F5");
                    preset.tx = channel.Tx;
                    preset.TXFreq = preset.tx.ToString("F5");
                    preset.pwr = channel.OutputPower == 0 ? 10 : channel.OutputPower == 1 ? 25 : 99;
                    preset.sql = 25;
                    preset.asql = 1;
                    string step = Beautifiers.StepStrings[channel.Step].Replace("kHz", "").Trim() + "k";
                    preset.step = Array.IndexOf(Defines.StepNames, step);
                    if (preset.step == -1) preset.step = 0;
                    preset.mode = (ushort)channel.Modulation;
                    preset.ctcss = channel.TxCodeType == 1 ? channel.TxCode : 0;
                    preset.dcs = channel.TxCodeType == 2 || channel.TxCodeType == 3 ? channel.TxCode : 0;
                    preset.bw = channel.Bandwidth == 0 ? 18856 : 18440;
                    preset.tone = channel.TxCodeType;
                    preset.comp = channel.Compander;
                    preset.rxtone = channel.RxCodeType;
                    preset.rxctcss = channel.RxCodeType == 1 ? channel.RxCode : 0;
                    preset.rxdcs = channel.RxCodeType == 2 || channel.RxCodeType == 3 ? channel.RxCode : 0;
                    ID(preset);
                }
            }
        }
        private static readonly PropertyChangedEventArgs args = new(nameof(BG));

        private double rx, tx, pwr, sql;
        private ushort mode;
        private int step, ctcss, dcs, bw, tone, comp, asql, rxctcss, rxdcs, rxtone;
        private bool tx2rx, isRange;
        private ulong id = 0;

        private Brush bg = Brushes.Black;
        private bool isScanning = false, isActive = false, wasActive = false;

        public XTONETYPE RxToneType => (XTONETYPE)rxtone;
        public int RxCTCSS => rxctcss;
        public int RxDCS => rxdcs;
        public double Squelch => sql;
        public double Step => Defines.StepValues[step];
        public double Sql => sql;
        public int Asql => asql;
        public ulong Id => id;
        public double RX => rx;
        public double TX => tx;
        public bool IsRange => isRange;
        public double RangeFreq { get; set; }
        public bool MainVFO { get; private set; } = false;
        public bool Blacklisted { get; set; } = false;
        public bool IsSelected { get; set; } = false;
        public bool IsSelectedInList { get; set; } = false;
        public string LastPreset { get; set; } = string.Empty;
        public double LastRssi { get; set; } = 0;
        public double WasRssi { get; set; } = 0;
        public int Index { get; set; } = 0;
        public VFOPreset Next { get; set; } = null!;
        public bool IsScanning 
        {
            get => isScanning;
            set
            {
                isScanning = value;
                OnPropertyChanged();
            }
        }
        public bool IsActive
        { 
            get => isActive;
            set
            {
                isActive = value;
                OnPropertyChanged();
            }
        }
        public bool WasActive
        {
            get => wasActive;
            set
            {
                wasActive = value;
                OnPropertyChanged();
            }
        }

        public Brush BG => bg;

        protected virtual void OnPropertyChanged()
        {
            bg = isScanning ? scanBrush : isActive ? Brushes.Maroon : wasActive ? Brushes.DarkBlue : Brushes.Black;
            (_ = PropertyChanged)?.Invoke(this, args);
        }

        public VFOPreset()
        {
            InitializeComponent();
            Opacity = 0.6;
            Set();
        }

        public void Set()
        {
            asql = autoSquelch.Value;
            sql = squelch.Value;
            rx = rxFreq.Value;
            RXFreq = rx.ToString("F5");
            tx = txFreq.Value;
            TXFreq = tx.ToString("F5");
            if (!lockPower.Value)
                pwr = power.Value;
            mode = vfoMode.Value;
            step = vfoStep.Value;
            ctcss = vfoCtcss.Value;
            rxctcss = rxvfoCtcss.Value;
            dcs = vfoDcs.Value;
            rxdcs = rxvfoDcs.Value;
            bw = (int)bandwidth.Value;
            tone = (int)toneType.Value;
            rxtone = (int)rxtoneType.Value;
            comp = (int)compander.Value;
            tx2rx = txisRX.Value;
        }

        public void Recall(double freqOverride)
        {
            if (MainVFO && this != CurrentVFO)
                CurrentVFO.Set();
            autoSquelch.Value = asql;
            squelch.Value = sql;
            txisRX.Value = tx2rx;
            string lp = CurrentVFO.LastPreset;
            if(freqOverride >= 0)
                rxFreq.Value = freqOverride;
            else
                rxFreq.Value = rx;
            CurrentVFO.LastPreset = lp;
            txFreq.Value = tx;
            if (!lockPower.Value)
                power.Value = pwr;
            vfoMode.Value = mode;
            vfoStep.Value = step;
            vfoCtcss.Value = ctcss;
            vfoDcs.Value = dcs;
            rxvfoCtcss.Value = rxctcss;
            rxvfoDcs.Value = rxdcs;
            bandwidth.Value = (XBANDWIDTH)bw;
            toneType.Value = (XTONETYPE)tone;
            rxtoneType.Value = (XTONETYPE)rxtone;
            compander.Value = (XCOMPANDER)comp;
            if (!MainVFO)
            {
                selected.Value = PName;
                CurrentVFO.LastPreset = PName;
                lastPresetIndex = presets.Value.IndexOf(this);
            }
            else
            {
                xvfolcd.Value = PName;
                selected.Value = LastPreset;
                CurrentVFO = this;
            }
            BK4819.Bandwidth();
            BK4819.Modulation();
        }

        public void Recall()
        {
            Recall(-1);
        }

        public override string ToString() => PName;

        public string PName
        {
            get { return (string)GetValue(PNameProperty); }
            set { SetValue(PNameProperty, value.Replace(',', '_')); }
        }
        public static readonly DependencyProperty PNameProperty =
            DependencyProperty.Register("PName", typeof(string), typeof(VFOPreset), new PropertyMetadata(string.Empty, PNameChanged));

        private static void PNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is string s && d is VFOPreset preset)
                preset.isRange = s.StartsWith("RANGE");
        }

        public string RXFreq
        {
            get { return (string)GetValue(RXFreqProperty); }
            set { SetValue(RXFreqProperty, value); }
        }
        public static readonly DependencyProperty RXFreqProperty =
            DependencyProperty.Register("RXFreq", typeof(string), typeof(VFOPreset), new PropertyMetadata(string.Empty));

        public string TXFreq
        {
            get { return (string)GetValue(TXFreqProperty); }
            set { SetValue(TXFreqProperty, value); }
        }
        public static readonly DependencyProperty TXFreqProperty =
            DependencyProperty.Register("TXFreq", typeof(string), typeof(VFOPreset), new PropertyMetadata(string.Empty));

        public event PropertyChangedEventHandler? PropertyChanged;

        private void Grid_MouseEnter(object sender, MouseEventArgs e)
        {
            Opacity = 1;
            MouseWasOver = this;
        }

        private void Grid_MouseLeave(object sender, MouseEventArgs e)
        {
            Opacity = 0.6;
        }

        private void Grid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                Highlight(true);
            }
            else
            {
                Unhighlight();
                Recall();
            }
        }

        private void Highlight(bool toggle)
        {
            if (!toggle)
                Background = new SolidColorBrush(Colors.Maroon);
            else
                Background =  Background == null ? new SolidColorBrush(Colors.Maroon) : null;
        }

        private static void Unhighlight()
        {
            foreach (var preset in presets.Value)
                preset.Background = null;
        }

    }

}
