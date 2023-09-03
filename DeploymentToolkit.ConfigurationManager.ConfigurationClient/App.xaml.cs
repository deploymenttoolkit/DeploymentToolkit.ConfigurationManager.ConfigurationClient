// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using DeploymentToolkit.ConfigurationManager.ConfigurationClient.HostedServices;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.WMI;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Services;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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

        private readonly IHost HostedService;

        public IServiceProvider Services => HostedService.Services;

        public App()
        {
            HostedService = Host
                .CreateDefaultBuilder()
                .ConfigureServices(services =>
                {
                    services.AddSingleton<NavigationService>();

                    services.AddSingleton<UACService>();

                    services.AddSingleton<WindowsRemoteManagementClient>();
                    services.AddSingleton<WindowsManagementInstrumentationClient>();

                    services.AddSingleton<ClientConnectionManager>();

                    services.AddTransient<IConfigurationManagerClientService, ConfigurationManagerClientService>();

                    services.AddSingleton<ClientEventsService>();

                    services.AddTransient<ClientEventsPageViewModel>();
                    services.AddTransient<MainWindowViewModel>();
                    services.AddTransient<ComponentsPageViewModel>();
                    services.AddTransient<CachePageViewModel>();
                    services.AddTransient<BITSPageViewModel>();
                    services.AddTransient<ActionsPageViewModel>();
                    services.AddTransient<ConfigurationsViewModel>();
                    services.AddTransient<PolicyPageViewModel>();
                    services.AddTransient<LogPageViewModel>();
                    services.AddTransient<DeviceRegistrationViewModel>();

                    services.AddSingleton<SettingsPageViewModel>();

                    services.AddSingleton<WinRMDebugPageViewModel>();

                    services.AddTransient<ApplicationsPageViewModel>();
                    services.AddTransient<ProgramPageViewModel>();
                    services.AddTransient<SoftwareUpdatesPageViewModel>();

                    services.AddTransient<GeneralPageViewModel>();

                    services.AddHostedService<ClientEventsHostedService>();
                })
                .ConfigureLogging((context, logger) =>
                {
                    logger.ClearProviders();

                    logger
                        .AddDebug()
#if DEBUG
                        .SetMinimumLevel(LogLevel.Trace);
#endif
                })
                .Build();

            HostedService.RunAsync();

            this.InitializeComponent();
        }

        ~App()
        {
            HostedService.StopAsync().ConfigureAwait(false).GetAwaiter().GetResult();
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
    }
}
