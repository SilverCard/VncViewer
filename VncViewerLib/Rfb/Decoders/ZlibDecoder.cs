using System.Drawing;

namespace VncViewer.Vnc
{
    public class ZlibDecoder : ZlibDecoderBase
    {
        private RawDecoder _RawDecoder;

        public ZlibDecoder(Framebuffer framebuffer, RfbSerializer rfbSerializer, ZlibStream zStream) : base(framebuffer, rfbSerializer, zStream)
        {
            _RawDecoder = new RawDecoder(Framebuffer, new RfbSerializer(zStream));
        }

        public override void Decode(Rectangle r)
        {
            FillBuffer();
            _RawDecoder.Decode(r);
        } 
    }
}
