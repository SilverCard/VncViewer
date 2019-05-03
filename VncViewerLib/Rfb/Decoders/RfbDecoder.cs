using System;
using System.Drawing;
using System.Threading;

namespace VncViewer.Vnc
{
    public class RfbDecoder : IDisposable
    {
        public Framebuffer Framebuffer { get; private set; }
        public RfbSerializer Serializer { get; private set; }

        public RawDecoder RawDecoder { get; private set; }
        public ZlibDecoder ZlibDecoder { get; private set; }
        public CopyRectDecoder CopyRectDecoder { get; private set; }
        public ZrleDecoder ZrleDecoder { get; private set; }

        private ZlibStream _ZlibStream;

        public readonly static RfbEncodingType[] SupportedDecoders = new RfbEncodingType[] {
                RfbEncodingType.CopyRect,
                RfbEncodingType.ZRLE,
                RfbEncodingType.Zlib,             
                RfbEncodingType.Raw,                
            };

        public RfbDecoder(Framebuffer framebuffer, RfbSerializer rfbSerializer)
        {
            Framebuffer = framebuffer ?? throw new ArgumentNullException(nameof(framebuffer));
            Serializer = rfbSerializer ?? throw new ArgumentNullException(nameof(rfbSerializer));

            _ZlibStream = new ZlibStream();
            RawDecoder = new RawDecoder(framebuffer, rfbSerializer);
            ZlibDecoder = new ZlibDecoder(framebuffer, rfbSerializer, _ZlibStream);
            CopyRectDecoder = new CopyRectDecoder(framebuffer, rfbSerializer);
            ZrleDecoder = new ZrleDecoder(framebuffer, rfbSerializer, _ZlibStream);
        }

        private BaseDecoder GetDecoder(RfbEncodingType enc)
        {
            switch (enc)
            {
                case RfbEncodingType.Raw:
                    return RawDecoder;

                case RfbEncodingType.CopyRect:
                    return CopyRectDecoder;           
     
                case RfbEncodingType.Zlib:
                    return ZlibDecoder;

                case RfbEncodingType.ZRLE:
                    return ZrleDecoder;

                default:
                    throw new NotImplementedException();
            }
        }

        public Rectangle[] UpdateFramebuffer(CancellationToken token)
        {
            Serializer.ReadByte();

            var n = Serializer.ReadUInt16();
            var rects = new Rectangle[n];

            for (int i = 0; i < n && !token.IsCancellationRequested; i++)
            {
                rects[i] = Serializer.ReadRectangle();
                var encoding = (RfbEncodingType)Serializer.ReadInt32();
                var dec = GetDecoder(encoding);
                dec.Decode(rects[i]);
            }

            return rects;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _ZlibStream?.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
