using System;
using System.Drawing;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Input;
using System.Collections.Generic;
using System.Windows;
using System.Threading.Tasks;
using VncViewerLib;

namespace VncViewerLib.WPF
{

    [ToolboxBitmap(typeof(VncViewerControl), "Resources.vncviewer.ico")]
    public partial class VncViewerControl : UserControl
    {        
        
        private VncClient _VncClient;                                    

        public const String CursorName = "VncCursor";
        public Cursor VncCursor { get; private set; }
        
        public string Hostname { get => _VncClient?.Framebuffer?.Name; }
        
        public WriteableBitmap FramebufferBitmap { get; private set; }
        private WritableBitmapWriter _WritableBitmapWriter;

        public int RefreshInterval { get; private set; }

        #region Events

        public event EventHandler OnConnected;
        public event EventHandler OnDisconnected;

        #endregion

        public VncViewerControl() : base()
        {
            InitializeComponent(); 
            Cursor = ((TextBlock)this.Resources[CursorName]).Cursor;
        }      
        
        public async Task ConnectAsync(String host, int port, byte bitsPerPixel, byte depth)
        {            
            ShowLabelText($"Connecting to VNC host {host}:{port} please wait... ");

            _VncClient?.Dispose();
            _VncClient = new VncClient(bitsPerPixel, depth)
            {
                RefreshInterval = RefreshInterval
            };
            _VncClient.OnDisconnect += _VncClient_OnDisconnect;
            _VncClient.OnFramebufferUpdate += _VncClient_OnFramebufferUpdate;

            try
            {
                await Task.Run(() =>
                {
                    _VncClient.Connect(host, port);
                });
            }
            catch (Exception ex)
            {
                ShowLabelText($"Failed to connect: {ex.Message}.");
            }

            OnConnected?.Invoke(this, EventArgs.Empty);
        }

        public async Task VncAuthenticate(String password)
        {
            ShowLabelText($"Authenticating...");

            var a = new VncAuthenticator(password);

            try
            {
                await Task.Run(() => _VncClient.Authenticate(a));
            }
            catch (VncSecurityException ex)
            {
                ShowLabelText($"Authentication failed: {ex.Reason}.");
            }

        }

        private void _VncClient_OnDisconnect(VncClient sender, DisconnectEventArgs e)
        {
            OnDisconnected?.Invoke(this, EventArgs.Empty);
        }                
        
        public async Task InitializeAsync()
        {
            ShowLabelText("Initializing...");
            await Task.Run(() => _VncClient.Initialize());

            FramebufferBitmap = WritableBitmapWriter.BuildWriteableBitmap(_VncClient.Framebuffer.Width, _VncClient.Framebuffer.Height);
            VncImage.Source = FramebufferBitmap;
            _WritableBitmapWriter = new WritableBitmapWriter(FramebufferBitmap);
            _VncClient.ReceiveUpdates();
        }

        private void _VncClient_OnFramebufferUpdate(VncClient sender, FramebufferUpdateEventArgs e)
        {
            _WritableBitmapWriter.UpdateFromFramebuffer(e.Framebuffer, e.Rectangles);
        }    
               

        public Task DisconnectAsync()
        {
            return Task.Run(() => _VncClient.Disconnect());
        }

        protected void VncClientConnectionLost(object sender, EventArgs e)
        {
            ShowLabelText("Disconnected.");
        }        

        public void ShowLabelText(String text)
        {
            Label.Content = text;
            Label.Visibility = Visibility.Visible;
        }

        public void HideLabel()
        {
            Label.Visibility = Visibility.Hidden;
        }
        
    }
}
