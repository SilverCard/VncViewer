using System;
using System.Collections.Generic;
using System.Text;

namespace VncViewerLib
{
    public class ColorMapEntries
    {
        public ushort FirstColour { get; set; }
        public ushort[,] Map { get; set; }
    }
}
