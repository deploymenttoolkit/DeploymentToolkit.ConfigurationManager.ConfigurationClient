// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using DeploymentToolkit.ConfigurationManager.ConfigurationClient.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.Extensions.DependencyInjection;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Services;
using WinUIEx;
using Microsoft.UI.Windowing;
using Microsoft.UI;
using System;
using WinRT.Interop;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainWindow : WindowEx
{
    public MainWindowViewModel ViewModel { get; set; }

    private readonly AppWindow _appWindow;

    public UIElement GetAppTitleBar()
    {
        return AppTitleBar;
    }

    public MainWindow()
    {
        ViewModel = App.Current.Services.GetService<MainWindowViewModel>()!;

        this.InitializeComponent();

        ViewModel.Notification = Notification;

        if (AppWindowTitleBar.IsCustomizationSupported())
        {
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);
            AppTitleBar.Loaded += AppTitleBar_Loaded;
            AppTitleBar.SizeChanged += AppTitleBar_SizeChanged;
        }
        else
        {
            AppTitleBar.Visibility = Visibility.Collapsed;
        }

        _appWindow = GetAppWindowForCurrentWindow();

        var navigationService = App.Current.Services.GetService<NavigationService>()!;
        navigationService.SetNavigationFrame(NavigationFrame);
        navigationService.Navigate((NavigationViewItem)MainNavigationView.MenuItems.First(p => (string)((NavigationViewItemBase)p).Tag == "ConnectPage"));
        ViewModel.SelectedItem = navigationService.GetCurrentNavigationViewItem()!;

        Title = "Configuration Manager Properties";
    }

    private void AppTitleBar_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if(AppWindowTitleBar.IsCustomizationSupported())
        {
            SetDragRegionForCustomTitleBar();
        }
    }
    private void AppTitleBar_Loaded(object sender, RoutedEventArgs e)
    {
        if (AppWindowTitleBar.IsCustomizationSupported())
        {
            SetDragRegionForCustomTitleBar();
        }
    }

    private AppWindow GetAppWindowForCurrentWindow()
    {
        var hWnd = WindowNative.GetWindowHandle(this);
        var wndId = Win32Interop.GetWindowIdFromWindow(hWnd);
        return AppWindow.GetFromWindowId(wndId);
    }

    [DllImport("Shcore.dll", SetLastError = true)]
    internal static extern int GetDpiForMonitor(IntPtr hmonitor, Monitor_DPI_Type dpiType, out uint dpiX, out uint dpiY);

    internal enum Monitor_DPI_Type : int
    {
        MDT_Effective_DPI = 0,
        MDT_Angular_DPI = 1,
        MDT_Raw_DPI = 2,
        MDT_Default = MDT_Effective_DPI
    }

    private double GetScaleAdjustment()
    {
        var hWnd = WindowNative.GetWindowHandle(this);
        var wndId = Win32Interop.GetWindowIdFromWindow(hWnd);
        var displayArea = DisplayArea.GetFromWindowId(wndId, DisplayAreaFallback.Primary);
        var hMonitor = Win32Interop.GetMonitorFromDisplayId(displayArea.DisplayId);

        // Get DPI.
        var result = GetDpiForMonitor(hMonitor, Monitor_DPI_Type.MDT_Default, out var dpiX, out var _);
        if (result != 0)
        {
            throw new Exception("Could not get DPI for monitor.");
        }

        var scaleFactorPercent = (uint)(((long)dpiX * 100 + (96 >> 1)) / 96);
        return scaleFactorPercent / 100.0;
    }

    private void SetDragRegionForCustomTitleBar()
    {
        if (AppWindowTitleBar.IsCustomizationSupported())
        {
            var scaleAdjustment = GetScaleAdjustment();

            RightPaddingColumn.Width = new GridLength(_appWindow.TitleBar.RightInset / scaleAdjustment);
            LeftPaddingColumn.Width = new GridLength(_appWindow.TitleBar.LeftInset / scaleAdjustment);

            List<Windows.Graphics.RectInt32> dragRectsList = new();

            Windows.Graphics.RectInt32 dragRectL;
            dragRectL.X = (int)((LeftPaddingColumn.ActualWidth) * scaleAdjustment);
            dragRectL.Y = 0;
            dragRectL.Height = (int)(AppTitleBar.ActualHeight * scaleAdjustment);
            dragRectL.Width = (int)((IconColumn.ActualWidth
                                    + TitleColumn.ActualWidth
                                    + LeftDragColumn.ActualWidth) * scaleAdjustment);
            dragRectsList.Add(dragRectL);

            Windows.Graphics.RectInt32 dragRectR;
            //dragRectR.X = (int)((LeftPaddingColumn.ActualWidth
            //                    + IconColumn.ActualWidth
            //                    + TitleTextBlock.ActualWidth
            //                    + LeftDragColumn.ActualWidth
            //                    + ButtonColumn.ActualWidth) * scaleAdjustment);
            dragRectR.X = (int)(dragRectL.Width + (ButtonColumn.ActualWidth * scaleAdjustment));
            dragRectR.Y = 0;
            dragRectR.Height = (int)(AppTitleBar.ActualHeight * scaleAdjustment);
            dragRectR.Width = (int)(RightDragColumn.ActualWidth * scaleAdjustment);
            dragRectsList.Add(dragRectR);

            var dragRects = dragRectsList.ToArray();

            _appWindow.TitleBar.SetDragRectangles(dragRects);
        }
    }
}
