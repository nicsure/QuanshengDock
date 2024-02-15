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
        static UserFolder()
        {
            if (!Directory.Exists(Instance.Name))
                Directory.CreateDirectory(Instance.Name);
        }

        public static string File(string file)
        {
            return Path.Combine(Instance.Name, file);
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
