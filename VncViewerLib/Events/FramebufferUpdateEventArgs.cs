using System;
using System.Collections.Generic;
using System.Drawing;

namespace VncViewer.Vnc
{
    public class FramebufferUpdateEventArgs : EventArgs
    {
        public Framebuffer Framebuffer { get; private set; }
        public IEnumerable<Rectangle> Rectangles { get; private set; }

        public FramebufferUpdateEventArgs(Framebuffer framebuffer, IEnumerable<Rectangle> rectangles)
        {      
            Framebuffer = framebuffer ?? throw new ArgumentNullException(nameof(framebuffer));
            Rectangles = rectangles ?? throw new ArgumentNullException(nameof(rectangles));
        }
    }
}
