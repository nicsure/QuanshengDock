using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;

namespace QuanshengDock.General
{
    public class SavedDictionary : Dictionary<string, string>
    {
        public string BackingFile { get; set; }
        public SavedDictionary(string backingFile)
        {
            BackingFile = backingFile;
            try
            {
                string[] lines = File.ReadAllLines(backingFile);
                for (int i = 0; i < (lines.Length & 0x7ffffffe); i += 2)
                {
                    string key = Regex.Unescape(lines[i]);
                    string value = Regex.Unescape(lines[i + 1]);
                    this[key] = value;
                }
            }
            catch { }
        }

        public void Save()
        {
            try
            {
                File.WriteAllLines(BackingFile, this.Select(kp => $"{Regex.Escape(kp.Key)}\r\n{Regex.Escape(kp.Value)}").ToArray());
            }
            catch { }
        }

        public string GetValue(string key, string? def = null) => ContainsKey(key) ? this[key] : (def ?? string.Empty);

    }
}
