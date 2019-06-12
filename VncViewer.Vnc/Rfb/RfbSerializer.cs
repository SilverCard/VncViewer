using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace VncViewer.Vnc
{
    public class RfbSerializer
    {
        public Stream BaseStream { get; private set; }
        private Encoding _Encoding;

        public RfbSerializer(Stream baseStream)
        {
            BaseStream = baseStream ?? throw new ArgumentNullException(nameof(baseStream));
            _Encoding = Encoding.UTF8;
        }       
                
        public T Deserialize<T>() where T : new()
        {
            T obj = new T();

            var memberInfos = obj.GetType().GetProperties().Where(p => Attribute.IsDefined(p, typeof(MessageMemberAttribute))).Select(p => MessageMemberInfo.FromPropertyInfo(p, obj));

            foreach (var m in memberInfos.OrderBy(x => x.MessageMemberAttribute.Index))
            {
                Object value = null;

                if (m.Type == typeof(byte))
                {
                    value = ReadByte();
                }
                else if (m.Type == typeof(byte[]))
                {
                    value = ReadBytes(m.MessageMemberAttribute.Size);
                }
                else if (m.Type == typeof(ushort))
                {
                    value = ReadUInt16();
                }
                else if (m.Type == typeof(bool))
                {
                    value = ReadBool();
                }
                else
                {
                    throw new NotImplementedException();
                }

                m.PropertyInfo.SetValue(obj, value);
            }

            return obj;
        }


        public void Serialize(Object value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));           

            var memberInfos = value.GetType().GetProperties().Where(p => Attribute.IsDefined(p, typeof(MessageMemberAttribute))).Select(p => MessageMemberInfo.FromPropertyInfo(p, value));

            foreach (var m in memberInfos.OrderBy(x => x.MessageMemberAttribute.Index))
            {
                if (m.Type == typeof(byte))
                {
                    WriteByte((byte)m.Value);
                }
                else if (m.Type == typeof(byte[]))
                {
                    WriteBytes((byte[])m.Value);
                }
                else if (m.Type == typeof(short))
                {
                    WriteInt16((short)m.Value);
                }
                else if (m.Type == typeof(int))
                {
                    WriteInt32((int)m.Value);
                }
                else if (m.Type == typeof(ushort))
                {
                    WriteUInt16((ushort)m.Value);
                }
                else if (m.Type == typeof(uint))
                {
                    WriteUInt32((uint)m.Value);
                }
                else if (m.Type == typeof(int[]))
                {
                    WriteInt32Array((int[])m.Value);
                }
                else if (m.Type == typeof(bool))
                {
                    WriteBool((bool)m.Value);
                }
                else
                {
                    Serialize(m.Value);
                }

            }

         
        }

        #region Readers

        private Boolean ReadBool()
        {
            var b = ReadByte();
            return b != 0;
        }

        public ServerInitMessage ReadServerInitMessage()
        {
            return new ServerInitMessage()
            {
                FramebufferWidth = ReadUInt16(),
                FramebufferHeight = ReadUInt16(),
                PixelFormat = Deserialize<PixelFormat>(),
                Name = ReadString()
            };
        }

        public ColorMapEntries ReadColorMapEntries()
        {
            ReadByte();
            var m = new ColorMapEntries();
            m.FirstColour = ReadUInt16();

            var n = ReadUInt16();

            m.Map = new ushort[n, 3];

            for (int i = 0; i < n; i++)
            {
                m.Map[i, 0] = (byte)(ReadUInt16() * byte.MaxValue / ushort.MaxValue); // R
                m.Map[i, 1] = (byte)(ReadUInt16() * byte.MaxValue / ushort.MaxValue); // G
                m.Map[i, 2] = (byte)(ReadUInt16() * byte.MaxValue / ushort.MaxValue); // B
            }

            return m;
        }

        public Rectangle ReadRectangle()
        {
            return new Rectangle()
            {
                X = ReadUInt16(),
                Y = ReadUInt16(),
                Width = ReadUInt16(),
                Height = ReadUInt16()
            };
        }

        public RfbVersion ReadVersion()
        {
            return RfbVersion.FromString(ReadString(12));
        }

        public ServerClientMessageType ReadServerMessageType()
        {
            return (ServerClientMessageType)ReadByte();
        }

        public String ReadString()
        {
            int len = (int)ReadUInt32();
            var str = _Encoding.GetString(ReadBytes(len));
            return str;
        }

        public String ReadString(int len)
        {
            var str = _Encoding.GetString(ReadBytes(len));
            return str;
        }

        public byte ReadByte()
        {
            var r = BaseStream.ReadByte();
            if (r == -1) throw new Exception();
            return (byte)r;
        }

        public int ReadInt32()
        {
            var b = ReadBytes(4);
            if (BitConverter.IsLittleEndian) Array.Reverse(b);
            return BitConverter.ToInt32(b, 0);
        }

        public uint ReadUInt32()
        {
            var b = ReadBytes(4);
            if (BitConverter.IsLittleEndian) Array.Reverse(b);
            return BitConverter.ToUInt32(b, 0);
        }

        public ushort ReadUInt16()
        {
            var b = ReadBytes(2);
            if (BitConverter.IsLittleEndian) Array.Reverse(b);
            return BitConverter.ToUInt16(b, 0);
        }

        public byte[] ReadBytes(int size)
        {
            if (size <= 0) throw new ArgumentOutOfRangeException(nameof(size));

            Byte[] b = new byte[size];
            int i = 0, r;

            while (i < size)
            {
                r = BaseStream.Read(b, i, size - i);

                if (r == 0) break;
                i += r;
            }

            if (i != size) throw new InvalidOperationException($"Response have an unexpected size {i} bytes, expected {size}.");
            return b;
        }


        #endregion

        #region Writers

        public void WriteVersion(RfbVersion rfbVersion)
        {
            var clientVersionBytes = Encoding.UTF8.GetBytes(rfbVersion.ToString());
            WriteBytes(clientVersionBytes);
        }

        public void WriteBytes(byte[] b) => BaseStream.Write(b, 0, b.Length);
        public void WriteByte(byte b) => BaseStream.WriteByte(b);

        public void WriteMessage(MessageBase message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            Serialize(message);
        }

        private void WriteBool(bool value) => WriteByte((byte)(value ? 1 : 0));

        private void WriteInt32(Int32 n)
        {
            var b = BitConverter.GetBytes(n);
            if (BitConverter.IsLittleEndian) Array.Reverse(b);
            WriteBytes(b);
        }

        private void WriteInt16(Int16 n)
        {
            var b = BitConverter.GetBytes(n);
            if (BitConverter.IsLittleEndian) Array.Reverse(b);
            WriteBytes(b);
        }

        private void WriteUInt32(UInt32 n)
        {
            var b = BitConverter.GetBytes(n);
            if (BitConverter.IsLittleEndian) Array.Reverse(b);
            WriteBytes(b);
        }

        private void WriteUInt16(UInt16 n)
        {
            var b = BitConverter.GetBytes(n);
            if (BitConverter.IsLittleEndian) Array.Reverse(b);
            WriteBytes(b);
        }


        private void WriteInt32Array(int[] value)
        {
            foreach (var v in value)
                WriteInt32(v);
        }

        #endregion
    }
}



