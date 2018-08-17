using System;
using System.Drawing;

namespace VncViewerLib
{
    public class FramebufferUpdateEventArgs : EventArgs
    {
        public Framebuffer Framebuffer { get; private set; }
        public Rectangle[] Rectangles { get; private set; }

        public FramebufferUpdateEventArgs(Framebuffer framebuffer, Rectangle[] rectangles)
        {
            Framebuffer = framebuffer ?? throw new ArgumentNullException(nameof(framebuffer));
            Rectangles = rectangles ?? throw new ArgumentNullException(nameof(rectangles));
        }
    }
}
