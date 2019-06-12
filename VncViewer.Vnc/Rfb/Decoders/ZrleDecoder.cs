using System;
using System.Drawing;

namespace VncViewer.Vnc
{
    public class ZrleDecoder : ZlibDecoderBase
    {
        private RfbSerializer _DSerializer;
        private RawDecoder _RawDecoder;

        private const ushort TileWidth = 64;
        private const ushort TileHeight = 64;

        private int[] _Palette = new int[128];
        private int[] _TileBuffer = new int[TileWidth * TileHeight];

        public ZrleDecoder(Framebuffer framebuffer, RfbSerializer rfbSerializer, ZlibStream zStream) : base(framebuffer, rfbSerializer, zStream)
        {
            if (framebuffer == null) throw new ArgumentNullException(nameof(framebuffer));            

            _DSerializer = new RfbSerializer(zStream);

            var bpp = framebuffer.PixelFormat.BitsPerPixel;

            if (bpp == 32)
            {
                throw new NotImplementedException();
            }
            else if (bpp == 16)
            {
                PixelReader = new PixelReader16(framebuffer, _DSerializer);
            }
            else if (bpp == 8)
            {
                PixelReader = new PixelReader8(framebuffer, _DSerializer);
            }

            _RawDecoder = new RawDecoder(Framebuffer, _DSerializer);
        }

        private void ReadZrlePackedPixels(int tw, int th, int[] palette, int palSize, int[] tile)
        {
            var bppp = palSize > 16 ? 8 :
                (palSize > 4 ? 4 : (palSize > 2 ? 2 : 1));
            var ptr = 0;

            for (var i = 0; i < th; i++)
            {
                var eol = ptr + tw;
                var b = 0;
                var nbits = 0;

                while (ptr < eol)
                {
                    if (nbits == 0)
                    {
                        b = _DSerializer.ReadByte();
                        nbits = 8;
                    }
                    nbits -= bppp;
                    var index = (b >> nbits) & ((1 << bppp) - 1) & 127;
                    tile[ptr++] = palette[index];
                }
            }
        }

        private void ReadZrlePlainRLEPixels(int tw, int th, int[] tileBuffer)
        {
            var ptr = 0;
            var end = ptr + tw * th;
            while (ptr < end)
            {
                var pix = PixelReader.ReadPixel();
                var len = 1;
                int b;
                do
                {
                    b = _DSerializer.ReadByte();
                    len += b;
                } while (b == byte.MaxValue);

                while (len-- > 0) tileBuffer[ptr++] = pix;
            }
        }

        private void ReadZrlePackedRLEPixels(int tw, int th, int[] palette, int[] tile)
        {
            var ptr = 0;
            var end = ptr + tw * th;
            while (ptr < end)
            {
                int index = _DSerializer.ReadByte();
                var len = 1;
                if ((index & 128) != 0)
                {
                    int b;
                    do
                    {
                        b = _DSerializer.ReadByte();
                        len += b;
                    } while (b == byte.MaxValue);
                }

                index &= 127;

                while (len-- > 0) tile[ptr++] = palette[index];
            }
        }

        public override void Decode(Rectangle r)
        {
            FillBuffer();

            for (var ty = 0; ty < r.Height; ty += TileHeight)
            {
                var th = Math.Min(r.Height - ty, TileHeight);

                for (var tx = 0; tx < r.Width; tx += TileWidth)
                {
                    var tw = Math.Min(r.Width - tx, TileWidth);

                    var subencoding = _DSerializer.ReadByte();

                    if (subencoding >= 17 && subencoding <= 127 || subencoding == 129)
                        throw new VncException("Invalid subencoding value");

                    var isRLE = (subencoding & 128) != 0;
                    var paletteSize = subencoding & 127;

                    var tileRect = new Rectangle(tx, ty, tw, th);
                    var destRect = new Rectangle(tx + r.X, ty + r.Y, tw, th);

                    // Fill palette
                    for (var i = 0; i < paletteSize; i++)
                        _Palette[i] = PixelReader.ReadPixel();

                    if (paletteSize == 1)
                    {
                        FillRect(destRect, _Palette[0]);
                        continue;
                    }

                    if (!isRLE)
                    {
                        if (paletteSize == 0)
                            _RawDecoder.Decode(destRect); // Raw Pixel                        
                        else
                            ReadZrlePackedPixels(tw, th, _Palette, paletteSize, _TileBuffer);  // Packed palette

                    }
                    else
                    {
                        if (paletteSize == 0)
                            ReadZrlePlainRLEPixels(tw, th, _TileBuffer); // Plain RLE
                        else
                            ReadZrlePackedRLEPixels(tw, th, _Palette, _TileBuffer);  // Packed RLE palette                  

                    }

                    CopyTile(_TileBuffer, destRect);
                }
            }
        }
    }
}
