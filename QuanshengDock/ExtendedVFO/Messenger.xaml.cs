using QuanshengDock.Data;
using QuanshengDock.General;
using QuanshengDock.Serial;
using QuanshengDock.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace QuanshengDock.ExtendedVFO
{
    public partial class Messenger : Window
    {
        private static readonly ViewModel<ObservableCollection<XMessage>> messages = VM.Get<ObservableCollection<XMessage>>("Messages");
        private static readonly ViewModel<ContextMenu> seenMenu = VM.Get<ContextMenu>("SeenMenu");
        public static ViewModel<string> Callsign { get; } = VM.Get<string>("Callsign");
        public static Messenger? Instance { get; private set; } = null;
        public static readonly List<string> seen = new();

        static Messenger()
        {
            AddSeen("ALL");
        }

        public static void Open()
        {
            Instance?.Close();           
            Instance ??= new();
            Instance.Show();
            if (messages.Value.Count > 0)
                messages.Value[^1].BringIntoView();
            BK4819.PrepareFSK();
        }

        public static void AddSeen(string from)
        {
            string fromUC = from.ToUpper();
            if (!seen.Contains(fromUC) && !fromUC.Equals(Callsign.Value.ToUpper()))
            {
                seen.Add(fromUC);
                MenuItem mi = new();
                string fromnc = from;
                mi.Header = fromnc;
                mi.Click += (object sender, RoutedEventArgs e) =>
                {
                    if (Instance != null)
                        Instance.Target.Text = fromnc;
                };
                seenMenu.Value.Items.Add(mi);
            }
        }

        public static XMessage Add(string from, string to, string message)
        {
            AddSeen(from);
            ObservableCollection<XMessage> temp = new();
            foreach(var xx in messages.Value)
                temp.Add(xx);
            XMessage x = new(from, to, message);
            temp.Add(x);
            while (temp.Count >= 50)
                temp.RemoveAt(0);
            messages.Value.Clear();
            messages.Value = temp;
            x.BringIntoView();
            return x;
        }


        public static void SetTarget(string target)
        {
            if(Instance != null)
                Instance.Target.Text = target;
        }

        public static void Data(byte[] data)
        {
            DateTime now = DateTime.Now;
            if ((now - lastData).TotalMilliseconds > 100)
            {
                stage = 0xcd;
                cnt = 0;
            }
            lastData = now;
            ProcessByte(data[6]);
            ProcessByte(data[7]);
            ProcessByte(data[4]);
            ProcessByte(data[5]);
            ProcessByte(data[2]);
            ProcessByte(data[3]);
            ProcessByte(data[0]);
            ProcessByte(data[1]);
        }

        protected override void OnActivated(EventArgs e)
        {
            MainWindow.Instance?.Focus();
            base.OnActivated(e);
        }

        private static int stage = 0xcd, cnt = 0, inCrc = 0, crc = 0;
        private static string inFrom = string.Empty, inTo = string.Empty, inMess = string.Empty;
        private static DateTime lastData = DateTime.Now;
        private static void ProcessByte(byte b)
        {
            cnt++;
            if (stage <= 2)
                crc = Comms.Crc16(b, crc);
            switch (stage)
            {
                case 0xcd:
                    if (b == stage) stage = 0xab;
                    break;
                case 0xab:
                    if (b == stage)
                    {
                        inFrom = string.Empty;
                        stage = 0;
                        crc = 0;
                    }
                    break;
                case 0xba:
                    if (b == stage) stage = 0xdc;
                    break;
                case 0xdc:
                    if (b == stage)
                    {
                        if (inCrc == crc)
                            Radio.Invoke(() => Add(inFrom, inTo, inMess));
                        stage = 0xcd;
                        cnt = 0;
                    }
                    break;
                case 0:
                    if (b == 0)
                    {
                        stage = 1;
                        inTo = string.Empty;
                    }
                    else
                        inFrom += (char)b;
                    break;
                case 1:
                    if (b == 0)
                    {
                        stage = 2;
                        inMess = string.Empty;
                    }
                    else
                        inTo += (char)b;
                    break;
                case 2:
                    if (b != 0)
                        inMess += (char)b;
                    if (cnt == 68)
                    {
                        inCrc = 0;
                        stage = 3;
                    }
                    break;
                case 3:
                    inCrc = b;
                    stage = 4;
                    break;
                case 4:
                    inCrc |= b << 8;
                    stage = 0xba;
                    break;
            }
        }

        private Messenger()
        {
            DataContext = Context.Instance;           
            InitializeComponent();
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void Frame_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Instance = null;
            Close();
        }

        private void Input_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                var x = Add(Callsign.Value, Target.Text, Input.Text);
                Input.Text = string.Empty;
                x.Transmit();
            }
        }
    }

    public class XMessage : StackPanel
    {
        private static byte IDs = 0;

        public string From { get => from; set { from = value; SetText(); } }
        private string from = string.Empty;
        public string To { get => to; set { to = value; SetText(); } }
        private string to = string.Empty;
        public string Message { get => message; set { message = value; SetText(); } }
        private string message = string.Empty;
        public byte ID { get; } = IDs++;

        private readonly TextBlock headerBlock;
        private readonly TextBlock messageBlock;

        public XMessage(string from, string to, string message)
        {
            Margin = new(0, 5, 0, 5);
            headerBlock = new();
            messageBlock = new();
            headerBlock.TextWrapping = TextWrapping.Wrap;
            messageBlock.TextWrapping = TextWrapping.Wrap;
            headerBlock.Opacity = 0.5;
            this.Children.Add(headerBlock);
            this.Children.Add(messageBlock);
            this.from = from;
            this.to = to;
            Message = message;
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                Messenger.SetTarget(Messenger.Callsign.Value.Equals(from) ? to : from);
            }
            base.OnMouseDown(e);
        }

        private void SetText()
        {
            headerBlock.Text = $"[ {from} > {to} ]";
            messageBlock.Text = message;
        }

        public void Transmit()
        {
            BK4819.Chat(from, to, message);
        }

    }

}
