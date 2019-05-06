using System;
using System.Collections.Generic;
using System.Text;

namespace VncViewer.Vnc
{
    public abstract class PixelReader
    {
        public RfbSerializer Serializer { get; internal set; }
        public Framebuffer Framebuffer { get; internal set; }

        private const int _Alpha = 0xFF << 24;

        protected PixelReader(Framebuffer framebuffer, RfbSerializer rfbSerializer)
        {
            Serializer = rfbSerializer  ?? throw new ArgumentNullException(nameof(rfbSerializer));
            Framebuffer = framebuffer ?? throw new ArgumentNullException(nameof(framebuffer));
        }

        public abstract int ReadPixel();

        protected static int PackPixel(byte red, byte green, byte blue)
        {
            // Put colour values into proper order for GDI+ (i.e., BGRA, where Alpha is always 0xFF)
            return blue & 0xFF | green << 8 | red << 16 | _Alpha;
        }
    }
}
