namespace VncViewer.Vnc
{
    public class FramebufferUpdateRequestMessage : MessageBase
    {
        [MessageMember(1)]
        public bool Incremental { get; set; }

        [MessageMember(2)]
        public ushort X { get; set; }

        [MessageMember(3)]
        public ushort Y { get; set; }

        [MessageMember(4)]
        public ushort Width { get; set; }

        [MessageMember(5)]
        public ushort Height { get; set; }

        public FramebufferUpdateRequestMessage() : base(3)
        {
        }
    }
}
