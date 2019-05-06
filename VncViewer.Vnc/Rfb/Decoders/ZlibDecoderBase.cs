namespace VncViewer.Vnc
{
    public abstract class ZlibDecoderBase : BaseDecoder
    {
        private ZlibStream _ZlibStream;

        public ZlibDecoderBase(Framebuffer framebuffer, RfbSerializer rfbSerializer, ZlibStream zStream) : base(framebuffer, rfbSerializer)
        {
            _ZlibStream = zStream;
        }

        public virtual void FillBuffer()
        {
            var len = Serializer.ReadInt32();
            var bytes = Serializer.ReadBytes(len);
            _ZlibStream.SetBuffer(bytes);
        }
    }
}
