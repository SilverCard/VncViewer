using System;

namespace VncViewer.Vnc
{
    public class ServerInitMessage
    {
        public ushort FramebufferWidth { get; set; }
        public ushort FramebufferHeight { get; set; }
        public PixelFormat PixelFormat { get; set; }
        public String Name { get; set; }

        public override string ToString()
        {
            return $"{FramebufferWidth}x{FramebufferHeight} {Name}";
        }
    }
}
