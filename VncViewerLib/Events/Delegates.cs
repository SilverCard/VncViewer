namespace VncViewerLib
{
    public delegate void FrameupdateUpdateHandler(VncClient sender, FramebufferUpdateEventArgs e);
    public delegate void DisconnectHandler(VncClient sender, DisconnectEventArgs e);
    public delegate void VncStateChangedHandler(VncClient sender, VncStateChangedEventArgs e);
}
