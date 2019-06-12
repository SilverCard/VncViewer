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
        private TcpClient tcpClient;
        private RfbSerializer serializer;
        private byte[] securityTypes;
        private int timeout;
        private VncState vncState;

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
        private RfbDecoder rfbDecoder;

        /// <summary>
        /// Pixel format of the Framebuffer.
        /// </summary>
        private PixelFormat pixelFormat;

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

        private Task receiveUpdatesTask;
        private CancellationTokenSource cts;

        private object _FullScreenRefreshLock = new object();
        private bool _FullScreenRefresh = false;      
 
        public VncClient(byte bitsPerPixel = 16, byte depth = 8)
        {
            tcpClient = new TcpClient()
            {
                NoDelay = true,
                SendTimeout = Timeout,
                ReceiveTimeout = Timeout
            };
            securityTypes = Array.Empty<byte>();

            pixelFormat = new PixelFormat(bitsPerPixel, depth);
            Timeout = 30*1000;
            vncState = VncState.NotConnected;
        }

        private void SetState(VncState newState)
        {
            var o = VncState;
            vncState = newState;
            OnStateChanged?.Invoke(this, new VncStateChangedEventArgs(o, newState));
        }

        public void Connect(string host, int port = 5900)
        {
            SetState(VncState.Connecting);

            tcpClient.Connect(host, port);
            serializer = new RfbSerializer(tcpClient.GetStream());

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
            serializer.WriteByte(1);
        }

        private void ServerInit()
        {
            var msg = serializer.ReadServerInitMessage();

            pixelFormat.IsBigEndian = msg.PixelFormat.IsBigEndian;          
            Framebuffer = new Framebuffer(msg.FramebufferWidth, msg.FramebufferHeight, pixelFormat)
            {
                Name = msg.Name
            };

            serializer.WriteMessage(new SetEncodingsMessage(RfbDecoder.SupportedDecoders));
            serializer.WriteMessage(new SetPixelFormatMessage(Framebuffer.PixelFormat));
        }

        public void RequestScreenUpdate(bool refreshFullScreen)
        {
            serializer.WriteMessage(new FramebufferUpdateRequestMessage()
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
            var r = rfbDecoder.UpdateFramebuffer(token);
            OnFramebufferUpdate?.Invoke(this, new FramebufferUpdateEventArgs(Framebuffer, r));    
        }

        private void UpdateColourMapEntries()
        {
            var map = serializer.ReadColorMapEntries();
            Framebuffer.UpdateColorMap(map);
        }

        public void ReceiveUpdates()
        {
            cts = new CancellationTokenSource();
            receiveUpdatesTask = Task.Run(() => ReceiveUpdates(cts.Token));
        }

        public void StopUpdates()
        {
            cts.Cancel();
        }

        public void Disconnect()
        {
            SetState(VncState.Disconnecting);
            if (cts?.IsCancellationRequested == false)
            {
                StopUpdates();
            }
            receiveUpdatesTask?.Wait();
            tcpClient.Close();
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
            tcpClient.ReceiveTimeout = 0;
            var msgType = serializer.ReadServerMessageType();
            tcpClient.ReceiveTimeout = Timeout;

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

            rfbDecoder?.Dispose();
            rfbDecoder = new RfbDecoder(Framebuffer, serializer);

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
                var type = serializer.ReadUInt32();
                securityTypes = new byte[] { (byte)type };
            }
            else
            {
                var n = serializer.ReadByte();
                if (n == 0) throw new Exception();

                securityTypes = serializer.ReadBytes(n);
            }
        }

        private void HandshakeProtocolVersion()
        {
            ServerVersion = serializer.ReadVersion();

            if (ServerVersion == RfbVersions.v3_3)
                ClientVersion = ServerVersion;
            else
                ClientVersion = RfbVersions.v3_8;

            serializer.WriteVersion(ClientVersion);
        }

        public void Authenticate(RfbAuthenticator a)
        {
            if (a == null) throw new ArgumentNullException(nameof(a));            

            SetState(VncState.Authenticating);

            if (!IsSecurityTypeSupported(a.SecurityType)) throw new NotSupportedException();

            serializer.WriteByte(a.SecurityType);
            a.Authenticate(serializer);
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

            serializer.WriteByte(type);

            if (ServerVersion >= RfbVersions.v3_8)
                GetSecurityResult();

            SetState(VncState.Authenticated);
        }

        private void GetSecurityResult()
        {
            var securityResult = serializer.ReadUInt32();

            if (securityResult >= 1 && ServerVersion >= RfbVersions.v3_8)
            {
                var reason = serializer.ReadString();
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

        public IEnumerable<byte> SecurityTypes => securityTypes == null ? Enumerable.Empty<byte>() : securityTypes;
        public Boolean IsSecurityTypeSupported(byte b) => SecurityTypes.Contains(b);
        public VncState VncState => vncState;

        /// <summary>
        /// Timeout of the socket.
        /// </summary>
        public int Timeout
        {
            get => timeout;
            set 
            {
                timeout = value;
                if(tcpClient != null)
                {
                    tcpClient.ReceiveTimeout = timeout;
                    tcpClient.SendTimeout = timeout;
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
                    tcpClient?.Dispose();
                    rfbDecoder?.Dispose();
                    cts?.Dispose();
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
