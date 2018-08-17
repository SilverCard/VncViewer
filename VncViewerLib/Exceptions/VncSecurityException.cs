using System;

namespace VncViewerLib
{
    public class VncSecurityException : Exception
    {
        public String Reason { get; protected set; }
        public uint SecurityResult { get; protected set; }

        public VncSecurityException(String message, uint securityResult, String reason = null) : base(message)
        {
            Reason = null;
            SecurityResult = securityResult;
        }
    }
}
