// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using DeploymentToolkit.ConfigurationManager.ConfigurationClient.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.Extensions.DependencyInjection;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Services;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindowViewModel ViewModel { get; set; }

        public MainWindow()
        {
            this.InitializeComponent();
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);

            App.Current.Services.GetService<NavigationService>().SetNavigationFrame(NavigationFrame);

            this.ViewModel = App.Current.Services.GetService<MainWindowViewModel>();

            this.Title = "Configuration Manager Properties";
        }
    }
}
