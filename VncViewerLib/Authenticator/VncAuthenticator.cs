using System;
using System.Security.Cryptography;
using System.Text;

namespace VncViewerLib
{
    public class VncAuthenticator : RfbAuthenticator
    {
        private byte[] _PasswordBytes;

        public VncAuthenticator(String password) : base(2)
        {
            if (password == null) throw new ArgumentNullException(nameof(password));
            _PasswordBytes = MakeKey(password);
        }

        private byte[] MakeKey(String password)
        {
            var key = Encoding.ASCII.GetBytes(password);
            Array.Resize(ref key, 8);

            for (int i = 0; i < 8; i++)
                key[i] = (byte)(((key[i] & 0x01) << 7) |
                                 ((key[i] & 0x02) << 5) |
                                 ((key[i] & 0x04) << 3) |
                                 ((key[i] & 0x08) << 1) |
                                 ((key[i] & 0x10) >> 1) |
                                 ((key[i] & 0x20) >> 3) |
                                 ((key[i] & 0x40) >> 5) |
                                 ((key[i] & 0x80) >> 7));

            return key;
        }


        private byte[] EncryptKey(byte[] key, byte[] challenge)
        {
            using (var des = new DESCryptoServiceProvider()
            {
                Padding = PaddingMode.None,
                Mode = CipherMode.ECB
            })
            using (var enc = des.CreateEncryptor(key, null))
            {
                var response = new byte[16];
                enc.TransformBlock(challenge, 0, challenge.Length, response, 0);
                return response;
            }
        }

        public override void Authenticate(RfbSerializer serializer)
        {    
            var challenge = serializer.ReadBytes(16);
            var b = EncryptKey(_PasswordBytes, challenge);
            serializer.WriteBytes(b);
        }
    }
}
