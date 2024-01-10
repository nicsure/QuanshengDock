using QuanshengDock.Channels;
using QuanshengDock.General;
using QuanshengDock.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace QuanshengDock.RepeaterBook
{
    public class Repeater
    {
        private const string apiUrl = "https://www.repeaterbook.com/api/exportROW.php?";
        public static int Activator { get => 0; set { } }
        private static readonly VM Actions = VM.Get("BookActions");
        private static readonly ViewModel<string> callsign = VM.Get<string>("BookCallsign");
        private static readonly ViewModel<string> city = VM.Get<string>("BookCity");
        private static readonly ViewModel<string> country = VM.Get<string>("BookCountry");
        private static readonly ViewModel<string> region = VM.Get<string>("BookRegion");
        private static readonly ViewModel<string> freq = VM.Get<string>("BookFrequency");
        private static readonly ViewModel<string> mode = VM.Get<string>("BookMode");
        private static readonly ViewModel<bool> idle = VM.Get<bool>("BookIdle");
        private static readonly ViewModel<string> message = VM.Get<string>("BookMessage");
        private static readonly ViewModel<ObservableCollection<Repeater>> results = VM.Get<ObservableCollection<Repeater>>("BookResults");
        private static readonly ViewModel<ObservableCollection<Repeater>> selected = VM.Get<ObservableCollection<Repeater>>("BookSelected");

        static Repeater()
        {
            Actions.CommandReceived += Actions_CommandReceived;
        }

        private static void Actions_CommandReceived(object sender, CommandReceievedEventArgs e)
        {
            if (e.Parameter is string cmd)
            {
                switch (cmd)
                {
                    case "Browse":
                        _ = Search();
                        break;
                    case "Copy":
                        Copy();
                        break;
                }
            }
        }

        public static void Open()
        {
            new RepeaterBookBrowser().ShowDialog();
        }

        private static void Copy()
        {
            if (selected.Value.Count > 0)
            {
                List<CopiedChannel> channels = new();
                foreach(var repeater in selected.Value)
                {
                    CopiedChannel cc = new(repeater.Callsign, repeater.InputFreq, repeater.OutputFreq, repeater.Code);
                    channels.Add(cc);
                }
                CopiedChannel.Clipboard = channels.ToArray();
                ShowMessage($"{selected.Value.Count} items copied");
            }
            else
                ShowMessage("Nothing Selected");
        }

        private static async Task Search()
        {
            idle.Value = false;
            string cls = callsign.Value.Trim();
            string cty = city.Value.Trim();
            string cnt = country.Value.Trim();
            string reg = region.Value.Trim();
            string frq = freq.Value.Trim();
            string mod = mode.Value.Trim();
            string get = string.Empty;
            if (cls.Length > 0) get += $"callsign={WebUtility.UrlEncode(cls)}&";
            if (cty.Length > 0) get += $"city={WebUtility.UrlEncode(cty)}&";
            if (cnt.Length > 0) get += $"country={WebUtility.UrlEncode(cnt)}&";
            if (reg.Length > 0) get += $"region={WebUtility.UrlEncode(reg)}&";
            if (frq.Length > 0) get += $"frequency={WebUtility.UrlEncode(frq)}&";
            if (mod.Length > 0) get += $"mode={WebUtility.UrlEncode(mod)}&";
            if(get.Length > 0)
            {
                using HttpClient httpClient = new();
                ShowMessage("Searching RepeaterBook");
                try
                {
                    httpClient.DefaultRequestHeaders.UserAgent.ParseAdd($"(QuanshengDock {Radio.Version})");
                    HttpResponseMessage response = await httpClient.GetAsync(apiUrl + get);
                    if (response.IsSuccessStatusCode)
                    {
                        string json = await response.Content.ReadAsStringAsync();
                        JsonDocument jsonDocument = JsonDocument.Parse(json);
                        JsonElement root = jsonDocument.RootElement;
                        int count = root.GetProperty("count").GetInt32();
                        ObservableCollection<Repeater> list = new();
                        if (count > 0)
                        {
                            ShowMessage($"{count} items found");
                            results.Value.Clear();
                            var resultsArray = root.GetProperty("results");
                            foreach (var repeaterElement in resultsArray.EnumerateArray())
                            {
                                Repeater repeater = new(
                                    repeaterElement.GetProperty("Operational Status").GetString() ?? string.Empty,
                                    repeaterElement.GetProperty(nameof(Callsign)).GetString() ?? string.Empty,
                                    repeaterElement.GetProperty("Input Freq").GetString() ?? string.Empty,
                                    repeaterElement.GetProperty("Frequency").GetString() ?? string.Empty,
                                    repeaterElement.GetProperty("PL").GetString() ?? string.Empty,
                                    repeaterElement.GetProperty("FM Analog").GetString() ?? string.Empty,
                                    repeaterElement.GetProperty(nameof(Region)).GetString() ?? string.Empty,
                                    repeaterElement.GetProperty("Nearest City").GetString() ?? string.Empty,
                                    repeaterElement.GetProperty(nameof(State)).GetString() ?? string.Empty,
                                    repeaterElement.GetProperty(nameof(Country)).GetString() ?? string.Empty
                                );
                                list.Add(repeater);
                            }
                            results.Value = list;
                        }
                        else
                            ShowMessage("No items found");
                    }
                }
                catch { }
            }
            idle.Value = true;
        }

        private static void ShowMessage(string msg)
        {
            message.Value = msg;
            _ = ClearMessage(msg);
        }

        private static async Task ClearMessage(string msg)
        {
            await Task.Delay(5000);
            if (msg.Equals(message.Value))
                message.Value = string.Empty;
        }

        public string Status { get; private set; }
        public string Callsign { get; private set; }
        public string OutputFreq { get; private set; }
        public string InputFreq { get; private set; }
        public string Code { get; private set; }
        public string FM { get; private set; }
        public string Region { get; private set; }
        public string City { get; private set; }
        public string State { get; private set; }
        public string Country { get; private set; }

        public Repeater(string status, string callsign, string inputFreq, string outputFreq, string code, string fM, string region, string city, string state, string country)
        {
            Status = status;
            Callsign = callsign;
            InputFreq = inputFreq;
            OutputFreq = outputFreq;
            Code = code;
            FM = fM;
            Region = region;
            City = city;
            State = state;
            Country = country;
        }
    }
}
