using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;

namespace VncViewer.Vnc
{
    public class ZlibStream : Stream
    {        
        private MemoryStream _Buffer;
        private DeflateStream _DeflateStream;
        private Boolean _SkippedHeader;

        public ZlibStream()
        {
            _Buffer = new MemoryStream();
            _DeflateStream = new DeflateStream(_Buffer, CompressionMode.Decompress);
            _SkippedHeader = false;
        }

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;

        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override void Flush() => throw new NotSupportedException();

        public void SetBuffer(byte[] b)
        {
            if (b == null) throw new ArgumentNullException(nameof(b));           

            _Buffer.SetLength(b.Length);
            _Buffer.Position = 0;
            _Buffer.Write(b, 0, b.Length);

            // Skip the zlib header.
            if (!_SkippedHeader)
            {
                _Buffer.Position = 2;
                _SkippedHeader = true;
            }
            else
            {
                _Buffer.Position = 0;
            }
        }

        public override int Read(byte[] buffer, int offset, int count) => _DeflateStream.Read(buffer, offset, count);
        public override int ReadByte() => _DeflateStream.ReadByte();
        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state) => _DeflateStream.BeginRead(buffer, offset, count, callback, state);
        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => _DeflateStream.ReadAsync(buffer, offset, count, cancellationToken);
        public override int EndRead(IAsyncResult asyncResult) => _DeflateStream.EndRead(asyncResult);

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        protected override void Dispose(bool disposing)
        {
            _Buffer?.Dispose();
            _DeflateStream?.Dispose();
            base.Dispose(disposing);
        }

     

    }
}
