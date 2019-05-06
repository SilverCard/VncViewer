using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace VncViewer.Vnc
{
    public class VncClient : IDisposable
    {
        private TcpClient _TcpClient;
        private RfbSerializer _Serializer;
        private byte[] _SecurityTypes;
        private int _Timeout;
        private VncState _VncState;

        /// <summary>
        /// RFB version of the server.
        /// </summary>
        public RfbVersion ServerVersion { get; private set; }

        /// <summary>
        /// RFB version of this client.
        /// </summary>
        public RfbVersion ClientVersion { get; private set; }

        /// <summary>
        /// Representation of the remote framebuffer.
        /// </summary>
        public Framebuffer Framebuffer { get; private set; }

        /// <summary>
        /// Decoder of the Framebuffer update rectangles.
        /// </summary>
        private RfbDecoder _RfbDecoder;

        /// <summary>
        /// Pixel format of the Framebuffer.
        /// </summary>
        private PixelFormat _PixelFormat;

        /// <summary>
        /// Raised when the framebuffer is updated.
        /// </summary>
        public event FrameupdateUpdateEventHandler OnFramebufferUpdate;

        /// <summary>
        /// Raised when the client is disconnected.
        /// </summary>
        public event DisconnectEventHandler OnDisconnect;

        /// <summary>
        /// Raised when the client change its state.
        /// </summary>
        public event VncStateChangedEventHandler OnStateChanged;

        private Task _ReceiveUpdatesTask;
        private CancellationTokenSource _Cts;

        private object _FullScreenRefreshLock = new object();
        private bool _FullScreenRefresh = false;      
 
        public VncClient(byte bitsPerPixel = 16, byte depth = 8)
        {
            _TcpClient = new TcpClient()
            {
                NoDelay = true,
                SendTimeout = Timeout,
                ReceiveTimeout = Timeout
            };
            _SecurityTypes = Array.Empty<byte>();

            _PixelFormat = new PixelFormat(bitsPerPixel, depth);
            Timeout = 1500;
            _VncState = VncState.NotConnected;
        }

        private void SetState(VncState newState)
        {
            var o = VncState;
            _VncState = newState;
            OnStateChanged?.Invoke(this, new VncStateChangedEventArgs(o, newState));
        }

        public void Connect(string host, int port = 5900)
        {
            SetState(VncState.Connecting);

            _TcpClient.Connect(host, port);
            _Serializer = new RfbSerializer(_TcpClient.GetStream());

            HandshakeProtocolVersion();
            GetSecurityTypes();

            SetState(VncState.Connected);
        }

        public void FullScreenRefresh()
        {
            lock (_FullScreenRefreshLock)
            {
                _FullScreenRefresh = true;
            }
        }

        private void ClientInit()
        {
            _Serializer.WriteByte(1);
        }

        private void ServerInit()
        {
            var msg = _Serializer.ReadServerInitMessage();

            _PixelFormat.IsBigEndian = msg.PixelFormat.IsBigEndian;          
            Framebuffer = new Framebuffer(msg.FramebufferWidth, msg.FramebufferHeight, _PixelFormat)
            {
                Name = msg.Name
            };

            _Serializer.WriteMessage(new SetEncodingsMessage(RfbDecoder.SupportedDecoders));
            _Serializer.WriteMessage(new SetPixelFormatMessage(Framebuffer.PixelFormat));
        }

        public void RequestScreenUpdate(bool refreshFullScreen)
        {
            _Serializer.WriteMessage(new FramebufferUpdateRequestMessage()
            {
                X = 0,
                Y = 0,
                Width = Framebuffer.Width,
                Height = Framebuffer.Height,
                Incremental = !refreshFullScreen
            });
        }


        public void RequestScreenUpdate()
        {
            bool r = _FullScreenRefresh;
            RequestScreenUpdate(r);

            if(r)
            {
                lock(_FullScreenRefreshLock)
                {
                    _FullScreenRefresh = false;
                }
            }            
        }

        private void ReceiveFramebufferUpdate(CancellationToken token)
        {           
            var r = _RfbDecoder.UpdateFramebuffer(token);
            OnFramebufferUpdate?.Invoke(this, new FramebufferUpdateEventArgs(Framebuffer, r));    
        }

        private void UpdateColourMapEntries()
        {
            var map = _Serializer.ReadColorMapEntries();
            Framebuffer.UpdateColorMap(map);
        }

        public void ReceiveUpdates()
        {
            _Cts = new CancellationTokenSource();
            _ReceiveUpdatesTask = Task.Run(() => ReceiveUpdates(_Cts.Token));
        }

        public void StopUpdates()
        {
            _Cts.Cancel();
        }

        public void Disconnect()
        {
            SetState(VncState.Disconnecting);
            if (_Cts?.IsCancellationRequested == false)
            {
                StopUpdates();
            }
            _ReceiveUpdatesTask?.Wait();
            _TcpClient.Close();
            SetState(VncState.Disconnected);
        }

        public void Initialize()
        {
            if (VncState != VncState.Authenticated) throw new VncException("The client must be authenticated.");

            SetState(VncState.Initializing);
            ClientInit();
            ServerInit();
            SetState(VncState.Initialized);
        }

        private void HandleMessage(CancellationToken token)
        {
            _TcpClient.ReceiveTimeout = 0;
            var msgType = _Serializer.ReadServerMessageType();
            _TcpClient.ReceiveTimeout = Timeout;

            switch (msgType)
            {
                case ServerClientMessageType.FramebufferUpdate:
                    ReceiveFramebufferUpdate(token);
                    break;

                case ServerClientMessageType.SetColourMapEntries:
                    UpdateColourMapEntries();
                    break;

                case ServerClientMessageType.Bell:
                    throw new NotImplementedException();

                case ServerClientMessageType.ServerCutText:
                    throw new NotImplementedException();

                case ServerClientMessageType.ResizeFrameBuffer:
                    throw new NotImplementedException();

                case ServerClientMessageType.KeyFrameUpdate:
                    throw new NotImplementedException();

                case ServerClientMessageType.FileTransfer:
                    throw new NotImplementedException();

                case ServerClientMessageType.TextChat:
                    throw new NotImplementedException();

                case ServerClientMessageType.KeepAlive:
                    throw new NotImplementedException();

                default:
                    throw new NotImplementedException();

            }
        }


        private void ReceiveUpdates(CancellationToken token)
        {
            if (VncState != VncState.Initialized) throw new VncException("The client must be initialized.");

            _RfbDecoder?.Dispose();
            _RfbDecoder = new RfbDecoder(Framebuffer, _Serializer);

            SetState(VncState.ReceivingMessages);

            try
            {
                RequestScreenUpdate(true);

                while (!token.IsCancellationRequested)
                {
                    HandleMessage(token);                                   
                    RequestScreenUpdate();
                }
            }
            catch (Exception e)
            {
                OnDisconnect?.Invoke(this, new DisconnectEventArgs(e));
            }
            finally
            {
            }
        }

        private void GetSecurityTypes()
        {
            if (ServerVersion == RfbVersions.v3_3)
            {
                var type = _Serializer.ReadUInt32();
                _SecurityTypes = new byte[] { (byte)type };
            }
            else
            {
                var n = _Serializer.ReadByte();
                if (n == 0) throw new Exception();

                _SecurityTypes = _Serializer.ReadBytes(n);
            }
        }

        private void HandshakeProtocolVersion()
        {
            ServerVersion = _Serializer.ReadVersion();

            if (ServerVersion == RfbVersions.v3_3)
                ClientVersion = ServerVersion;
            else
                ClientVersion = RfbVersions.v3_8;

            _Serializer.WriteVersion(ClientVersion);
        }

        public void Authenticate(RfbAuthenticator a)
        {
            SetState(VncState.Authenticating);

            if (!IsSecurityTypeSupported(a.SecurityType)) throw new NotSupportedException();

            _Serializer.WriteByte(a.SecurityType);
            a.Authenticate(_Serializer);
            GetSecurityResult();

            SetState(VncState.Authenticated);
        }

        /// <summary>
        /// Authenticate without any security mechanism.
        /// </summary>
        public void Authenticate()
        {
            SetState(VncState.Authenticating);

            byte type = 0;
            if (!IsSecurityTypeSupported(type)) throw new NotSupportedException();

            _Serializer.WriteByte(type);

            if (ServerVersion >= RfbVersions.v3_8)
                GetSecurityResult();

            SetState(VncState.Authenticated);
        }

        private void GetSecurityResult()
        {
            var securityResult = _Serializer.ReadUInt32();

            if (securityResult >= 1 && ServerVersion >= RfbVersions.v3_8)
            {
                var reason = _Serializer.ReadString();
                throw new VncSecurityException($"Failed to authenticate: {reason}.", securityResult, reason);
            }
        }

        //public void Dispose()
        //{
        //    _TcpClient?.Dispose();
        //}

        /// <summary>
        /// Check if the server supports Vnc Authentication.
        /// </summary>
        public Boolean SupportsVncAuthentication => SecurityTypes.Contains((byte)2);

        public IEnumerable<byte> SecurityTypes => _SecurityTypes == null ? Enumerable.Empty<byte>() : _SecurityTypes;
        public Boolean IsSecurityTypeSupported(byte b) => SecurityTypes.Contains(b);
        public VncState VncState => _VncState;

        /// <summary>
        /// Timeout of the socket.
        /// </summary>
        public int Timeout
        {
            get => _Timeout;
            set 
            {
                _Timeout = value;
                if(_TcpClient != null)
                {
                    _TcpClient.ReceiveTimeout = _Timeout;
                    _TcpClient.SendTimeout = _Timeout;
                }
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _TcpClient?.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion


    }
}
