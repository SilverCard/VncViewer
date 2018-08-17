using System;

namespace VncViewer
{
    public class Config
    {
        public String Host { get; set; }
        public int Port { get; set; }
        public String Password { get; set; }
        public byte BitsPerPixel { get; set; }
        public byte Depth { get; set; }
    }
}
