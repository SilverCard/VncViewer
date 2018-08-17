using Newtonsoft.Json;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace VncViewer
{
    public class Config
    {
        public String Host { get; set; }
        public int Port { get; set; }
        public String Password { get; set; }
        public byte[] ProtectedPassword { get; set; }
        public byte BitsPerPixel { get; set; }
        public byte Depth { get; set; }

        public static Config ReadFromFile(String fileName)
        {
            return JsonConvert.DeserializeObject<Config>(File.ReadAllText(fileName));
        }

        public void Save(String fileName)
        {
            File.WriteAllText(fileName, JsonConvert.SerializeObject(this, Formatting.Indented));
        }

        public void ProtectPassword()
        {
            if (Password != null)
            {
                ProtectedPassword = ProtectedData.Protect(Encoding.UTF8.GetBytes(Password), null, DataProtectionScope.CurrentUser);
                Password = null;
            }
        }

        public String GetProtectedPassword()
        {
            var p = ProtectedData.Unprotect(ProtectedPassword, null, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(p);
        }
    }
}
