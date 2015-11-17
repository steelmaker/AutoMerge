using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Merge.Desktop.Extentions
{
    /// <summary>
    /// Windows extenstions. Represent additional buttons.
    /// </summary>
    internal static class WindowExtensions
    {
        private const uint WsExContexthelp = 0x00000400;
        private const uint WsMinimizebox = 0x00020000;
        private const uint WsMaximizebox = 0x00010000;
        private const int GwlStyle = -16;
        private const int GwlExstyle = -20;
        private const int SwpNosize = 0x0001;
        private const int SwpNomove = 0x0002;
        private const int SwpNozorder = 0x0004;
        private const int SwpFramechanged = 0x0020;
        private const int WmSyscommand = 0x0112;
        private const int ScContexthelp = 0xF180;
        private const int WsExDlgmodalframe = 0x0001;

        private const string TutorialFileName = "tutorial.swf";

        [DllImport("user32.dll")]
        private static extern uint GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hwnd, int index, uint newStyle);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hwnd, IntPtr hwndInsertAfter, int x, int y, int width, int height, uint flags);


        internal static void InitializeButtons(this Window window)
        {
            var hwnd = new WindowInteropHelper(window).Handle;
            var styles = GetWindowLong(hwnd, GwlStyle);
            styles &= 0xFFFFFFFF;// ^ (WsMinimizebox | WsMaximizebox);
            SetWindowLong(hwnd, GwlStyle, styles);
            styles = GetWindowLong(hwnd, GwlExstyle);
            styles |= WsExContexthelp;
            SetWindowLong(hwnd, GwlExstyle, styles);
            SetWindowPos(hwnd, IntPtr.Zero, 0, 0, 0, 0, SwpNomove | SwpNosize | SwpNozorder | SwpFramechanged);
            var fromVisual = (HwndSource)PresentationSource.FromVisual(window);
            if (fromVisual != null)
                fromVisual.AddHook(HelpHook);
        }

        internal static void RemoveIcon(this Window window)
        {
            // Get this window's handle
            var hwnd = new WindowInteropHelper(window).Handle;

            // Change the extended window style to not show a window icon
            var extendedStyle = GetWindowLong(hwnd, GwlExstyle);
            SetWindowLong(hwnd, GwlExstyle, extendedStyle | WsExDlgmodalframe);

            // Update the window's non-client area to reflect the changes
            SetWindowPos(hwnd, IntPtr.Zero, 0, 0, 0, 0, SwpNomove |
                  SwpNosize | SwpNozorder | SwpFramechanged);
        }

        private static IntPtr HelpHook(IntPtr hwnd,
            int msg,
            IntPtr wParam,
            IntPtr lParam,
            ref bool handled)
        {
            if (msg == WmSyscommand &&
                ((int)wParam & 0xFFF0) == ScContexthelp)
            {
                if (MessageBox.Show("Open help tutorial?", "Help", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    try
                    {
                        Process.Start(string.Format("Help\\{0}", TutorialFileName));
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(string.Format("Can not open file '{0}'", TutorialFileName), "Error");

                        Debug.WriteLine(ex);
                    }
                }
                handled = true;
            }

            return IntPtr.Zero;
        }
    }
}
