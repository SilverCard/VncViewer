using System;
using System.Collections.Generic;
using System.Text;

#pragma warning disable CA1028 // Enum Storage should be Int32

namespace VncViewer.Vnc
{
    public enum ServerClientMessageType : byte
    {
        // Must support
        FramebufferUpdate = 0,
        SetColourMapEntries = 1,
        Bell = 2,
        ServerCutText = 3,

        // Optional
        ResizeFrameBuffer = 4,
        KeyFrameUpdate = 5,
        FileTransfer = 7,
        TextChat = 11,
        KeepAlive = 13
    }
}
