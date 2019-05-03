using System;
using System.Drawing;

namespace VncViewer.Vnc
{
    public abstract class BaseDecoder
    {
        public Framebuffer Framebuffer { get; private set; }
        public RfbSerializer Serializer { get; protected set; }
        public PixelReader PixelReader { get; protected set; }

        public BaseDecoder(Framebuffer framebuffer, RfbSerializer rfbSerializer)
        {
            Framebuffer = framebuffer ?? throw new ArgumentNullException(nameof(framebuffer));
            Serializer = rfbSerializer ?? throw new ArgumentNullException(nameof(rfbSerializer));

            var bpp = framebuffer.PixelFormat.BitsPerPixel;

            if (bpp == 32)
            {
                PixelReader = new PixelReader32(framebuffer, rfbSerializer);
            }
            else if (bpp == 16)
            {
                PixelReader = new PixelReader16(framebuffer, rfbSerializer);
            }
            else if (bpp == 8)
            {
                PixelReader = new PixelReader8(framebuffer, rfbSerializer);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public abstract void Decode(Rectangle r);

        protected void CopyTile(int[] tile, Rectangle destRect)
        {
            for (int y = 0; y < destRect.Height; y++)            
                Buffer.BlockCopy(tile, y * destRect.Width * 4, Framebuffer.PixelData, 4 * (destRect.X + (destRect.Y + y) * Framebuffer.Width), destRect.Width * 4);
            
        }

        protected void FillRect(Rectangle r, int pixel)
        {      
            for (int y = 0; y < r.Height; y++)
            {
                int z = r.X + (y + r.Y) * Framebuffer.Width;
                for (int x = 0; x < r.Width; x++)
                {
                    Framebuffer.PixelData[x + z] = pixel;
                }
            }
        }



    }
}
