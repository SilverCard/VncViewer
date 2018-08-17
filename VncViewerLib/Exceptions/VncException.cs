using System;

namespace VncViewerLib
{
    public class VncException : Exception
    {
        public VncException(String msg) : base(msg)
        {
        }
    }
}
