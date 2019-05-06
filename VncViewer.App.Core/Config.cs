using Newtonsoft.Json;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

#pragma warning disable CA1819 // Properties should not return arrays

namespace VncViewer.App.Core
{
    public class Config
    {
        public String Host { get; set; }
        public int Port { get; set; } = 5900;
        public byte[] ProtectedPassword { get; set; }
        public byte BitsPerPixel { get; set; } = 8;
        public byte Depth { get; set; } = 8;

        public void SetPassword(String p)
        {
            if (p != null)
            {
                ProtectedPassword = ProtectedData.Protect(Encoding.UTF8.GetBytes(p), null, DataProtectionScope.CurrentUser);
            }
        }

        public String GetPassword()
        {
            var p = ProtectedData.Unprotect(ProtectedPassword, null, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(p);
        }
    }
}
