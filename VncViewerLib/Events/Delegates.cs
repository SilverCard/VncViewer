namespace VncViewerLib
{
    public delegate void FrameupdateUpdateEventHandler(VncClient sender, FramebufferUpdateEventArgs e);
    public delegate void DisconnectEventHandler(VncClient sender, DisconnectEventArgs e);
    public delegate void VncStateChangedEventHandler(VncClient sender, VncStateChangedEventArgs e);
}
