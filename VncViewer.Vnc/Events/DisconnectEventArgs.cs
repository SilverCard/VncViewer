using System;

namespace VncViewer.Vnc
{
    public class DisconnectEventArgs : EventArgs
    {
        public Exception Exception { get; private set; }

        public DisconnectEventArgs(Exception exception)
        {
            Exception = exception;
        }

        public DisconnectEventArgs()
        { 
        }
    }
}
