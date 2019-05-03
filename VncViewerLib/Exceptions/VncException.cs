using System;
using System.Runtime.Serialization;

namespace VncViewer.Vnc
{
    [Serializable]
    public class VncException : Exception
    {
        public VncException()
        {
        }

        public VncException(string message) : base(message)
        {
        }

        public VncException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected VncException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
