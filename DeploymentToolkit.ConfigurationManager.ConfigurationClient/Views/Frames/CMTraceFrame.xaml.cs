using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Views.Frames
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CMTraceFrame : Page, IDisposable
    {
        [DllImport("user32.dll")]
        private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

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
            SetParent(_cmTraceProcess.MainWindowHandle, currentWindow);
        }

        public void Dispose()
        {
            _cmTraceProcess?.Kill();
            _cmTraceProcess?.Dispose();

            GC.SuppressFinalize(this);
        }
    }
}
