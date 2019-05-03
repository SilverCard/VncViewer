namespace VncViewer.Vnc
{
    public sealed class PixelReader16 : PixelReader
    {
        public PixelReader16(Framebuffer framebuffer, RfbSerializer rfbSerializer) : base(framebuffer, rfbSerializer)
        {
        }

        public override int ReadPixel()
        {
            var b = Serializer.ReadBytes(2);

            var pixel = (ushort)((uint)b[0] & 0xFF | (uint)b[1] << 8);
            var pf = Framebuffer.PixelFormat;

            var red = (byte)(((pixel >> pf.RedShift) & pf.RedMax) * 255 / pf.RedMax);
            var green = (byte)(((pixel >> pf.GreenShift) & pf.GreenMax) * 255 / pf.GreenMax);
            var blue = (byte)(((pixel >> pf.BlueShift) & pf.BlueMax) * 255 / pf.BlueMax);

            return PackPixel(red, green, blue);
        }
    }
}
