using System;

#pragma warning disable CA1819 // Properties should not return arrays
#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace VncViewer.Vnc
{
    public class Framebuffer
    {
        public String Name { get; set; }
        public int[] PixelData { get; private set; }

        public ushort Width { get; private set; }
        public ushort Height { get; private set; }
        public PixelFormat PixelFormat { get; private set; }

        public ushort[,] ColorMap { get; private set; } = new ushort[256, 3];

        public Framebuffer(ushort width, ushort height, PixelFormat pixelFormat)
        {
            Width = width;
            Height = height;

            PixelFormat = pixelFormat ?? throw new ArgumentNullException(nameof(pixelFormat));
            PixelData = new int[width * height];
        }

        public void UpdateColorMap(ColorMapEntries map)
        {
            if (map == null) throw new ArgumentNullException(nameof(map));

            for (int i = 0; i < map.Map.GetLength(0); i++)
            {
                ColorMap[map.FirstColour + i, 0] = map.Map[i, 0];
                ColorMap[map.FirstColour + i, 1] = map.Map[i, 1];
                ColorMap[map.FirstColour + i, 2] = map.Map[i, 2];
            }
        }
    }
}
