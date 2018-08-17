using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media.Imaging;

namespace VncViewerLib.WPF
{
    public class WritableBitmapWriter
    {
        public WriteableBitmap WriteableBitmap { get; private set; }

        public WritableBitmapWriter(WriteableBitmap writeableBitmap)
        {
            WriteableBitmap = writeableBitmap ?? throw new ArgumentNullException(nameof(writeableBitmap));
        }

        public static WriteableBitmap BuildWriteableBitmap(int width, int height)
        {
            var colors = new System.Windows.Media.Color[] {
            System.Windows.Media.Colors.Red,
            System.Windows.Media.Colors.Blue,
            System.Windows.Media.Colors.Green};

            BitmapPalette myPalette = new BitmapPalette(colors);
            return new WriteableBitmap(width, height, 96, 96, System.Windows.Media.PixelFormats.Pbgra32, myPalette);
        }

        /// <summary>
        /// Copy a rectangle from framebuffer into the bitmap's buffer.
        /// </summary>
        private void CopyRectangle(IntPtr backBuffer, Framebuffer framebuffer, Rectangle r)
        {            
            for (int y = 0; y < r.Height; y++)
            {
                int idx = r.X + (r.Y + y) * framebuffer.Width;
                Marshal.Copy(framebuffer.PixelData, idx, backBuffer + 4 * idx, r.Width);
            }
        }

        /// <summary>
        /// Copy multiple rectangles from framebuffer into the bitmap.
        /// </summary>
        public void UpdateFromFramebuffer(Framebuffer f, Rectangle[] rectangles)
        {
            IntPtr backBuffer = IntPtr.Zero;

            WriteableBitmap.Dispatcher.Invoke(() =>
            {
                WriteableBitmap.Lock();
                backBuffer = WriteableBitmap.BackBuffer;
            });

            foreach (var r in rectangles)
            {
                CopyRectangle(backBuffer, f, r);
            }

            WriteableBitmap.Dispatcher.Invoke(() =>
            {
                foreach (var r in rectangles)
                {
                    WriteableBitmap.AddDirtyRect(new Int32Rect(r.X, r.Y, r.Width, r.Height));
                }

                WriteableBitmap.Unlock();
            });
        }
    }
}
