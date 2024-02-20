using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanshengDock.User
{
    // code by -nicsure- 2024
    // https://www.youtube.com/nicsure

    public static class UserFolder
    {
        private const string scanLogDir = "scanlogs";

        static UserFolder()
        {
            string logdir = Path.Combine(Instance.Name, scanLogDir);
            if (!Directory.Exists(Instance.Name))
                Directory.CreateDirectory(Instance.Name);
            if (!Directory.Exists(logdir))
                Directory.CreateDirectory(logdir);
        }

        public static string File(string file)
        {
            return Path.Combine(Instance.Name, file);
        }

        public static string LogFile(string file)
        {
            return Path.Combine(Instance.Name, scanLogDir, file);
        }

        public static string Dir => Instance.Name;
    }

    public static class Instance
    {
        private static string name = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "QuanshengDock");
        public static string Name
        {
            get => name;
            set => name = value;
        }
    }

}
