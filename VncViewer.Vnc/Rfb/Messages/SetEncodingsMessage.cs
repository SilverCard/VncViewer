using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#pragma warning disable CA1819 // Properties should not return arrays

namespace VncViewer.Vnc
{
    public class SetEncodingsMessage : MessageBase
    {
        [MessageMember(1)]
        public byte Padding { get; private set; }

        [MessageMember(2)]
        public short NumberOfEncodings { get; private set; }

        [MessageMember(3)]

        public int[] Encodings { get; private set; }

        public SetEncodingsMessage(RfbEncodingType[] encodings) : base(2)
        {
            if (encodings == null) throw new ArgumentNullException(nameof(encodings));
            if (encodings.Length > short.MaxValue) throw new ArgumentOutOfRangeException(nameof(encodings), "Too many encodings.");

            NumberOfEncodings = (short)encodings.Length;
            Encodings = encodings.Select(e => (int)e).ToArray();
        }
    }
}
