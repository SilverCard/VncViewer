using System;

#pragma warning disable CA1032 // Implement standard exception constructors
#pragma warning disable CA2237

namespace VncViewer.Vnc
{
    public class VncSecurityException : Exception
    {
        public String Reason { get; protected set; }
        public uint SecurityResult { get; protected set; }

        public VncSecurityException(String message, uint securityResult, String reason = null) : base(message)
        {
            Reason = reason;
            SecurityResult = securityResult;
        }
    }
}
