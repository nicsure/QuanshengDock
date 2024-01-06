using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanshengDock.User
{
    public static class UserFolder
    {
        private static readonly string myDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        private static readonly string appFolder = Path.Combine(myDocuments, "QuanshengDock");

        static UserFolder()
        {
            if (!Directory.Exists(appFolder))
                Directory.CreateDirectory(appFolder);
        }

        public static string File(string file) => Path.Combine(appFolder, file);

        public static string Dir => appFolder;
    }
}
