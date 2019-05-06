using System;
using System.Collections.Generic;
using System.Text;

#pragma warning disable CA1707 // Identifiers should not contain underscores

namespace VncViewer.Vnc
{
    public static class RfbVersions
    {
        public static readonly RfbVersion v3_3 = new RfbVersion(3, 3);
        public static readonly RfbVersion v3_7 = new RfbVersion(3, 7);
        public static readonly RfbVersion v3_8 = new RfbVersion(3, 8);
    }
}
