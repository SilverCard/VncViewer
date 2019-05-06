using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace VncViewer.Vnc
{
    public class CopyRectDecoder : BaseDecoder
    {    
        public CopyRectDecoder(Framebuffer framebuffer, RfbSerializer rfbSerializer) : base(framebuffer, rfbSerializer)
        {
        }

        public override void Decode(Rectangle r)
        {
            var p = new Point()
            {
                X = Serializer.ReadUInt16(),
                Y = Serializer.ReadUInt16()
            };

            int bufferStride = r.Width * 4;
            byte[] buffer = new byte[r.Width * r.Height * 4];

            for (int y = 0; y < r.Height; y++)
            {
                int s = 4 * (p.X + (p.Y + y) * Framebuffer.Width);
                int d = 4 * (y) * r.Width;
                Buffer.BlockCopy(Framebuffer.PixelData, s, buffer, d, bufferStride);
            }

            for (int y = 0; y < r.Height; y++)
            {
                int s = 4 * (y) * r.Width;
                int d = 4 * (r.X + (r.Y + y) * Framebuffer.Width);
                Buffer.BlockCopy(buffer, s, Framebuffer.PixelData, d, bufferStride);
            }
        }
    }
}
