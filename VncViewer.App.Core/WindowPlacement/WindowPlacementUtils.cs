using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace VncViewer.App.Core.WindowPlacement
{
    public static class WindowPlacementUtils
    {

        [DllImport("user32.dll")]
        private static extern bool SetWindowPlacement(IntPtr hWnd, [In] ref WindowPlacementStruct lpwndpl);

        [DllImport("user32.dll")]
        private static extern bool GetWindowPlacement(IntPtr hWnd, out WindowPlacementStruct lpwndpl);

        private const int SW_SHOWNORMAL = 1;
        private const int SW_SHOWMINIMIZED = 2;

        public static void SetWpfWindowPlacement(Window window, WindowPlacementStruct wpStruct)
        {
            SetWindowPlacement(new WindowInteropHelper(window).Handle, ref wpStruct);
        }

        public static WindowPlacementStruct GetWpfWindowPlacement(Window window)
        {
            if (window == null) throw new ArgumentNullException(nameof(window));
            WindowPlacementStruct placement = new WindowPlacementStruct();
            GetWindowPlacement(new WindowInteropHelper(window).Handle, out placement);
            return placement;
        }

    }
}


