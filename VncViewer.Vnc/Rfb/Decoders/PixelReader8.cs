namespace VncViewer.Vnc
{
    public sealed class PixelReader8 : PixelReader
    {
        public PixelReader8(Framebuffer framebuffer, RfbSerializer rfbSerializer) : base(framebuffer, rfbSerializer)
        {
        }

        /// <summary>
        /// Reads an 8-bit pixel.
        /// </summary>
        /// <returns>Returns an Integer value representing the pixel in GDI+ format.</returns>
        public override int ReadPixel()
        {
            var idx = Serializer.ReadByte();
            return PackPixel((byte)Framebuffer.ColorMap[idx, 0], (byte)Framebuffer.ColorMap[idx, 1], (byte)Framebuffer.ColorMap[idx, 2]);
        }
    }
}
