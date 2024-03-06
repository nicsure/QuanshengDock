using QuanshengDock.General;
using QuanshengDock.UI;
using QuanshengDock.User;
using QuanshengDock.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace QuanshengDock.ExtendedVFO
{
    public class ScanList
    {
        public static int Activator { get => 0; set { } }
        private static readonly SavedList scanListsSave = new(UserFolder.File("scanlists.xvfo"));
        private static readonly ViewModel<ObservableCollection<ScanList>> scanlists = VM.Get<ObservableCollection<ScanList>>("ScanLists");
        private static readonly ViewModel<ObservableCollection<VFOPreset>> selected = VM.Get<ObservableCollection<VFOPreset>>("SelectedScanList");
        private static readonly ViewModel<ObservableCollection<VFOPreset>> presets = VM.Get<ObservableCollection<VFOPreset>>("VFOPresets");
        private static readonly ViewModel<double> rxTimeout = VM.Get<double>("ScanRxTimeout");
        private static readonly ViewModel<double> totTimeout = VM.Get<double>("ScanTotTimeout");
        private static readonly ViewModel<bool> scanMonitor = VM.Get<bool>("ScanMonitor");
        private static readonly ViewModel<int> scanSpeed = VM.Get<int>("ScanSpeed");

        public string Name { get; } = "report error";
        public override string ToString() => Name;
        public ObservableCollection<VFOPreset> Members { get; } = new();
        public bool IsSelected { get; set; } = false;

        static ScanList()
        {
            scanlists.CommandReceived += Scanlists_CommandReceived;
            VFOPreset.Activator++;
            foreach (string s in scanListsSave)
            {
                string[] p = s.Split(',');
                if (p.Length > 1)
                {
                    ScanList sl = new(p[0]);
                    for (int i = 1; i < p.Length; i++)
                    {
                        if (ulong.TryParse(p[i], out ulong u))
                        {
                            if (VFOPreset.Store.TryGetValue(u, out var preset))
                            {
                                sl.Members.Add(preset);
                            }
                        }
                    }
                }
            }
        }

        public static void Save()
        {
            scanListsSave.Clear();
            foreach (ScanList sl in scanlists.Value)
            {
                string s = sl.Name;
                foreach (VFOPreset preset in sl.Members)
                    s += "," + preset.Id.ToString();
                scanListsSave.Add(s);
            }
            scanListsSave.Save();
        }

        public static void Open()
        {
            if(!Scanner.IsOpened)
            {
                var window = new Scanner();
                window.Show();
            }
            else
            {
                Scanner.Instance?.Focus();
            }
        }

        private static void Scanlists_CommandReceived(object sender, CommandReceievedEventArgs e)
        {
            if(e.Parameter is ListBox lb && lb.Name.Equals("Lists"))
            {
                if(lb.SelectedItem is ScanList sl)
                {
                    selected.Value = sl.Members;
                }
                return;
            }

            if(e.Parameter is string s)
            {
                if(s.Equals("LeftDown"))
                {
                    if (Mouse.DirectlyOver is ButtonBorder button && button.Tag is string func)
                        s = func;
                }
                string[] cmd = s.Split(',');
                switch (cmd[0])
                {
                    case "ScanSpeed":
                        if (cmd.Length > 1)
                        {
                            if (int.TryParse(cmd[1], out var i))
                                scanSpeed.Value = i;
                        }
                        break;
                    case "Monitor":
                        scanMonitor.Value = !scanMonitor.Value;
                        break;
                    case "RxTime":
                        if(cmd.Length>1)
                        {
                            if (double.TryParse(cmd[1], out var d))
                                rxTimeout.Value = d;
                        }
                        break;
                    case "TotTime":
                        if (cmd.Length > 1)
                        {
                            if (double.TryParse(cmd[1], out var d))
                                totTimeout.Value = d;
                        }
                        break;
                    case "Scan":                        
                        BK4819.StartScan();
                        break;
                    case "Stop":
                        BK4819.StopScan();
                        break;
                    case "Delete":
                        foreach (var slist in scanlists.Value.ToArray())
                        {
                            if (slist.IsSelected)
                            {
                                scanlists.Value.Remove(slist);
                            }
                        }
                        selected.Value = null!;
                        break;
                    case "MoveListUp":
                        foreach (var slist in scanlists.Value.ToArray())
                        {
                            if (slist.IsSelected)
                            {
                                int i = scanlists.Value.IndexOf(slist);
                                if (i <= 0)
                                    break;
                                else
                                {
                                    (scanlists.Value[i - 1], scanlists.Value[i]) = (scanlists.Value[i], scanlists.Value[i - 1]);
                                }
                            }
                        }
                        break;
                    case "MoveListDown":
                        foreach (var slist in scanlists.Value.Reverse().ToArray())
                        {
                            if (slist.IsSelected)
                            {
                                int i = scanlists.Value.IndexOf(slist);
                                if (i >= scanlists.Value.Count - 1 || i < 0)
                                    break;
                                else
                                {
                                    (scanlists.Value[i + 1], scanlists.Value[i]) = (scanlists.Value[i], scanlists.Value[i + 1]);
                                }
                            }
                        }
                        break;
                    case "New":
                        string? name = Radio.Prompt("Enter name of new scan list.", false);
                        if (name != null)
                            _ = new ScanList(name);
                        break;
                    case "Add":
                        if (selected.Value != null)
                        {
                            foreach (var preset in presets.Value)
                            {
                                if (preset.IsSelected && !selected.Value.Contains(preset))
                                    selected.Value.Add(preset);
                            }
                        }
                        break;
                    case "Remove":
                        if (selected.Value != null)
                        {
                            foreach (var preset in selected.Value.ToArray())
                            {
                                if (preset.IsSelectedInList)
                                {
                                    selected.Value.Remove(preset);
                                }
                            }
                        }
                        break;
                    case "MoveUp":
                        foreach (var preset in selected.Value.ToArray())
                        {
                            if (preset.IsSelectedInList)
                            {
                                int i = selected.Value.IndexOf(preset);
                                if (i <= 0) 
                                    break;
                                else
                                {
                                    (selected.Value[i - 1], selected.Value[i]) = (selected.Value[i], selected.Value[i - 1]);
                                }
                            }
                        }
                        break;
                    case "MoveDown":
                        foreach (var preset in selected.Value.Reverse().ToArray())
                        {
                            if (preset.IsSelectedInList)
                            {
                                int i = selected.Value.IndexOf(preset);
                                if (i >= selected.Value.Count - 1 || i < 0)
                                    break;
                                else
                                {
                                    (selected.Value[i + 1], selected.Value[i]) = (selected.Value[i], selected.Value[i + 1]);
                                }
                            }
                        }
                        break;

                    default:
                        break;
                }
                return;
            }
        }

        public ScanList(string name)
        {
            Name = name;
            scanlists.Value.Add(this);
        }
    }
}
