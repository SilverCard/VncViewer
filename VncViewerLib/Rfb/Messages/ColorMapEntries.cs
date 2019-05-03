#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional
#pragma warning disable CA1819 // Properties should not return arrays

namespace VncViewer.Vnc
{
    public class ColorMapEntries
    {
        public ushort FirstColour { get; set; }
        public ushort[,] Map { get; set; }
    }
}
