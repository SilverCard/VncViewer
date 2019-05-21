using System;
using System.Runtime.InteropServices;

namespace VncViewer.App.Core.WindowPlacement
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct WindowPlacementStruct
    {
        public int length;
        public int flags;
        public int showCmd;
        public Point minPosition;
        public Point maxPosition;
        public Rect normalPosition;
    }
}
