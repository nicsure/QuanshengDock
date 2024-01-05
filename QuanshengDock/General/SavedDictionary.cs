using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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
            List<string> lines = new();
            foreach (string key in Keys)
            {
                string deKey = Regex.Escape(key);
                string deVal = Regex.Escape(this[key]);
                lines.Add(deKey);
                lines.Add(deVal);
            }
            try
            {
                File.WriteAllLines(BackingFile, lines);
            }
            catch { }
        }

        public string GetValue(string key, string? def = null) => ContainsKey(key) ? this[key] : (def ?? string.Empty);

    }
}
