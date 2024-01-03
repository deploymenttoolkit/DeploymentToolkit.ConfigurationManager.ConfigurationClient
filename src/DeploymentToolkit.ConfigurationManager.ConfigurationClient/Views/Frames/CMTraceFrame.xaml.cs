using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Vanara.PInvoke;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Views.Frames
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CMTraceFrame : Page, IDisposable
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
        [DllImport("user32.dll")]
        private static extern uint SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);
        [DllImport("user32.dll")]
        private static extern uint GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
        [DllImport("user32.dll")]
        private static extern IntPtr SetFocus(IntPtr hWnd);
        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, SetWindowPosFlags uFlags);

        private static int GWL_STYLE = -16;
        private static int GWL_EXSTYLE = -20;

        private static uint WS_CHILD = 0x40000000;
        private static uint WS_POPUP = 0x80000000;
        private static uint WS_CAPTION = 0x00C00000;
        private static uint WS_THICKFRAME = 0x00040000;
        private static uint WS_VISIBLE = 0x10000000;

        private static uint WS_EX_DLGMODALFRAME = 0x00000001;
        private static uint WS_EX_WINDOWEDGE = 0x00000100;
        private static uint WS_EX_CLIENTEDGE = 0x00000200;
        private static uint WS_EX_STATICEDGE = 0x00020000;

        [Flags]
        private enum SetWindowPosFlags : uint
        {
            SWP_ASYNCWINDOWPOS = 0x4000,
            SWP_DEFERERASE = 0x2000,
            SWP_DRAWFRAME = 0x0020,
            SWP_FRAMECHANGED = 0x0020,
            SWP_HIDEWINDOW = 0x0080,
            SWP_NOACTIVATE = 0x0010,
            SWP_NOCOPYBITS = 0x0100,
            SWP_NOMOVE = 0x0002,
            SWP_NOOWNERZORDER = 0x0200,
            SWP_NOREDRAW = 0x0008,
            SWP_NOREPOSITION = 0x0200,
            SWP_NOSENDCHANGING = 0x0400,
            SWP_NOSIZE = 0x0001,
            SWP_NOZORDER = 0x0004,
            SWP_SHOWWINDOW = 0x0040
        }

        private readonly Process _cmTraceProcess;

        public CMTraceFrame(string cmTracePath, string logFile)
        {
            this.InitializeComponent();

            _cmTraceProcess = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = cmTracePath,
                    Arguments = logFile,
                    WindowStyle = ProcessWindowStyle.Maximized
                }
            };
            _cmTraceProcess.Start();
            _cmTraceProcess.WaitForInputIdle();

            var currentWindow = WinRT.Interop.WindowNative.GetWindowHandle(App.Current.GetActiveWindow());
            var insertAfter = new IntPtr(-1);

            var dwSyleToRemove = WS_POPUP | WS_CAPTION | WS_THICKFRAME;
            var dwExStyleToRemove = WS_EX_DLGMODALFRAME | WS_EX_WINDOWEDGE | WS_EX_CLIENTEDGE | WS_EX_STATICEDGE;

            var style = GetWindowLong(_cmTraceProcess.MainWindowHandle, GWL_STYLE);
            var exStyle = GetWindowLong(_cmTraceProcess.MainWindowHandle, GWL_EXSTYLE);
            style &= ~dwSyleToRemove;
            exStyle &= ~dwExStyleToRemove;
            SetWindowLong(_cmTraceProcess.MainWindowHandle, GWL_STYLE, style | WS_VISIBLE);
            SetWindowLong(_cmTraceProcess.MainWindowHandle, GWL_EXSTYLE, exStyle);

            //SetWindowPos(_cmTraceProcess.MainWindowHandle, insertAfter, 0, 0, 0, 0, SetWindowPosFlags.SWP_NOMOVE | SetWindowPosFlags.SWP_NOSIZE);

            SetParent(_cmTraceProcess.MainWindowHandle, currentWindow);

            SetWindowPos(_cmTraceProcess.MainWindowHandle, insertAfter, 0, 0, 1000, 1000, SetWindowPosFlags.SWP_NOZORDER);
        }

        public void Dispose()
        {
            _cmTraceProcess?.Kill();
            _cmTraceProcess?.Dispose();

            GC.SuppressFinalize(this);
        }
    }
}
