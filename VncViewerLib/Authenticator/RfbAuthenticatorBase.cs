namespace VncViewer.Vnc
{
    public abstract class RfbAuthenticator
    {
        public byte SecurityType { get; private set; }

        protected RfbAuthenticator(byte securityType)
        {
            SecurityType = securityType;
        }

        public abstract void Authenticate(RfbSerializer serializer);
    }
}
