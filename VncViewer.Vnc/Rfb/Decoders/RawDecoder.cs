using System.Drawing;

namespace VncViewer.Vnc
{
    public class RawDecoder : BaseDecoder
    {
        public RawDecoder(Framebuffer framebuffer, RfbSerializer rfbSerializer) : base(framebuffer, rfbSerializer)
        {
        }

        public override void Decode(Rectangle r)
        {
            for (int y = 0; y < r.Height; y++)
            {
                int z = r.X + (y + r.Y) * Framebuffer.Width;
                for (int x = 0; x < r.Width; x++)
                {
                    Framebuffer.PixelData[x + z] = PixelReader.ReadPixel();
                }
            }
        }
    }
}
