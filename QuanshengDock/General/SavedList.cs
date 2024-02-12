using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace QuanshengDock.General
{
    public class SavedList : List<string>
    {
        public string BackingFile { get; set; }

        public SavedList(string backingFile) 
        {
            BackingFile = backingFile;
            try
            {
                string[] lines = File.ReadAllLines(backingFile);
                foreach(string line in lines) 
                    Add(Regex.Unescape(line));
            }
            catch { }
        }

        public void Save()
        {
            try
            {
                File.WriteAllLines(BackingFile, this.Select(l => $"{Regex.Escape(l)}").ToArray());
            }
            catch { }
        }

    }
}
