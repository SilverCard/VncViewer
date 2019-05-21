using Newtonsoft.Json;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using VncViewer.App.Core.WindowPlacement;

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

        public WindowPlacementStruct? WindowPlacement { get; set; }   

        public void SetPassword(String p)
        {
            if (p != null)
            {
                ProtectedPassword = ProtectedData.Protect(Encoding.UTF8.GetBytes(p), Globals.Entropy, DataProtectionScope.CurrentUser);
            }
        }

        public String GetPassword()
        {
            if (ProtectedPassword == null) return null;

            var p = ProtectedData.Unprotect(ProtectedPassword, Globals.Entropy, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(p);
        }
    }
}
