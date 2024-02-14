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
        private static readonly string myDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        private static readonly string appFolder = Path.Combine(myDocuments, "QuanshengDock");
        public static string CustomConfig { get; set; } = string.Empty;

        static UserFolder()
        {
            if (!Directory.Exists(appFolder))
                Directory.CreateDirectory(appFolder);
        }

        public static string File(string file)
        {
            if(CustomConfig.Length > 0 && file.Equals("app.config"))
                return CustomConfig;
            return Path.Combine(appFolder, file);
        }

        public static string Dir => appFolder;
    }
}
