namespace VncViewer.Vnc
{
    public sealed class PixelReader32 : PixelReader
    {
        public PixelReader32(Framebuffer framebuffer, RfbSerializer rfbSerializer) : base(framebuffer, rfbSerializer)
        {
        }

        public override int ReadPixel()
        {
            // Read the pixel value
            var b = Serializer.ReadBytes(4);

            var pixel = (uint)b[0] & 0xFF |
                         (uint)b[1] << 8 |
                         (uint)b[2] << 16 |
                         (uint)b[3] << 24;

            var pf = Framebuffer.PixelFormat;

            // Extract RGB intensities from pixel
            var red = (byte)((pixel >> pf.RedShift) & pf.RedMax);
            var green = (byte)((pixel >> pf.GreenShift) & pf.GreenMax);
            var blue = (byte)((pixel >> pf.BlueShift) & pf.BlueMax);

            return PackPixel(red, green, blue);
        }
    }
}
