namespace VncViewer.Vnc
{
    public enum RfbEncodingType : int
    {
        Raw = 0,
        CopyRect = 1,
        RRE = 2,
        CoRRE = 4,
        Hextile = 5,
        Zlib = 6,
        Tight = 7,
        ZlibHex = 8,
        Ultra = 9,
        Ultra2 = 10,
        TRLE = 15,
        ZRLE = 16,
        ZYWRLE = 17,
        H264 = 20,
        Jpeg = 21,
        Jrle = 22,
        TightPng = -260,
    }
}
