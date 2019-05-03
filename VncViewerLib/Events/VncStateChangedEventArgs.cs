using System;

namespace VncViewer.Vnc
{
    public class VncStateChangedEventArgs : EventArgs
    {
        public VncState OldState { get; private set; }
        public VncState NewState { get; private set; }

        public VncStateChangedEventArgs(VncState oldState, VncState newState)
        {
            OldState = oldState;
            NewState = newState;              
        }
    }
}
