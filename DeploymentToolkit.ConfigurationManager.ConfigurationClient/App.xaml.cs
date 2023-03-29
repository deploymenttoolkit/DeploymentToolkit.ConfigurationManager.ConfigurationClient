// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Services;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        public static new App Current => (App)Application.Current;
        public DispatcherQueue DispatcherQueue { get; private set; }

        public IServiceProvider Services { get; }

        public App()
        {
            Services = ConfigureServices();

            this.InitializeComponent();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            DispatcherQueue = DispatcherQueue.GetForCurrentThread();

            m_window = new MainWindow();
            m_window.Activate();
        }

        private Window m_window;

        internal Window GetActiveWindow()
        {
            return m_window;
        }

        private static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            services.AddSingleton<UACService>();
            services.AddSingleton<NavigationService>();
            services.AddSingleton<ConfigurationManagerClientService>();

            services.AddTransient<MainWindowViewModel>();
            services.AddTransient<ComponentsPageViewModel>();
            services.AddTransient<CachePageViewModel>();
            services.AddTransient<BITSPageViewModel>();
            services.AddTransient<ActionsPageViewModel>();
            services.AddTransient<ConfigurationsViewModel>();
            services.AddTransient<PolicyPageViewModel>();
            services.AddTransient<LogPageViewModel>();

            services.AddTransient<GeneralPageViewModel>();

            return services.BuildServiceProvider();
        }
    }
}
