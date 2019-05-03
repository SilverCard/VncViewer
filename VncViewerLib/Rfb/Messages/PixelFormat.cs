using System;

#pragma warning disable CA1819 // Properties should not return arrays

namespace VncViewer.Vnc
{
    public class PixelFormat
    {
        [MessageMember(0)]
        public byte BitsPerPixel { get; set; }

        [MessageMember(1)]
        public byte Depth { get; set; }

        [MessageMember(2)]
        public bool IsBigEndian { get; set; }

        [MessageMember(3)]
        public bool IsTrueColour { get; set; }

        [MessageMember(4)]
        public ushort RedMax { get; set; }

        [MessageMember(5)]
        public ushort GreenMax { get; set; }

        [MessageMember(6)]
        public ushort BlueMax { get; set; }

        [MessageMember(7)]
        public byte RedShift { get; set; }

        [MessageMember(8)]
        public byte GreenShift { get; set; }

        [MessageMember(9)]
        public byte BlueShift { get; set; }

        [MessageMember(10, 3)]
        public byte[] Padding { get; private set; }

        public PixelFormat()
        {
            Padding = new byte[3];
        }

        public PixelFormat(byte bitsPerPixel, byte depth) : this()
        {
            BitsPerPixel = bitsPerPixel;
            Depth = depth;

            if ((bitsPerPixel == 16) && (depth == 16))
            {
                IsTrueColour = false;
                RedMax = 31;
                GreenMax = 63;
                BlueMax = 31;
                RedShift = 11;
                GreenShift = 5;
                BlueShift = 0;
            }
            else if ((bitsPerPixel) == 16 && (depth == 8))
            { 
                IsTrueColour = false;
                RedMax = 31;
                GreenMax = 63;
                BlueMax = 31;
                RedShift = 11;
                GreenShift = 5;
                BlueShift = 0;
            }
            else if ((bitsPerPixel) == 8 && (depth == 8))
            {
                IsTrueColour = false;
                RedMax = 7;
                GreenMax = 7;
                BlueMax = 3;
                RedShift = 0;
                GreenShift = 3;
                BlueShift = 6;
            }
            else if ((bitsPerPixel) == 8 && (depth == 6))
            {
                IsTrueColour = false;
                RedMax = 3;
                GreenMax = 3;
                BlueMax = 3;
                RedShift = 4;
                GreenShift = 2;
                BlueShift = 0;
            }
            else if ((bitsPerPixel == 8) && (depth == 3))
            {
                IsTrueColour = false;
                RedMax = 1;
                GreenMax = 1;
                BlueMax = 1;
                RedShift = 2;
                GreenShift = 1;
                BlueShift = 0;
            }
            else throw new NotImplementedException();
        }
    }
}