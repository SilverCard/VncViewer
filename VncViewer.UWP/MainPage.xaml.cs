using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using VncViewerLib;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace VncViewer.UWP
{    

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private VncClient _VncClient;
        private WriteableBitmap _VncImage;

        public MainPage()
        {
            this.InitializeComponent();
        }

        private void UpdateRectangle(Stream stream, Framebuffer frambebuffer, Rectangle r )
        {
            byte[] buffer = new byte[r.Width * 4];

            for (int y = 0; y < r.Height; y++)
            {
                int p = 4 * (r.X + (r.Y + y) * frambebuffer.Width);
                Buffer.BlockCopy(frambebuffer.PixelData, p, buffer, 0, buffer.Length);
                stream.Position = p;
                stream.Write(buffer, 0, buffer.Length);
            }
        }

        private async void _VncClient_OnFramebufferUpdate(VncClient sender, FramebufferUpdateEventArgs e)
        {
            Stream stream = null;

            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                stream = _VncImage.PixelBuffer.AsStream();
            });

            foreach (var item in e.Rectangles)
            {
                UpdateRectangle(stream, e.Framebuffer, item);
            }

            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                stream?.Dispose();
                _VncImage.Invalidate();
            });
        }

        private async Task<Config> ReadConfigAsync()
        {
            var packageFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            var sampleFile = await packageFolder.GetFileAsync("Config.json");
            var str = await FileIO.ReadTextAsync(sampleFile);
            return JsonConvert.DeserializeObject<Config>(str);
        }


        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {

            var config = await ReadConfigAsync();
            
             _VncClient = new VncClient(config.BitsPerPixel, config.Depth);
            _VncClient.Connect(config.Host, config.Port);

            var auth = new VncAuthenticator(config.Password);
            _VncClient.Authenticate(auth);
            _VncClient.Initialize();

            _VncImage = new WriteableBitmap(_VncClient.Framebuffer.Width, _VncClient.Framebuffer.Height);
            VncImage.Source = _VncImage;

            _VncClient.OnFramebufferUpdate += _VncClient_OnFramebufferUpdate;
            _VncClient.ReceiveUpdates();
        }
    }
}
