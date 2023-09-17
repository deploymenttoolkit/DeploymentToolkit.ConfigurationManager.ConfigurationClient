// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Helpers;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.HostedServices;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.WMI;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Services;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System;
using WinUIEx;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient;

public partial class App : Application
{
    public static new App Current => (App)Application.Current;
    public DispatcherQueue DispatcherQueue { get; private set; }

    private IHost HostedService;

    public IServiceProvider Services => HostedService.Services;

    public App()
    {
        this.InitializeComponent();
    }

    internal void Build()
    {
        HostedService = Host
            .CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddSingleton<LocalSettingsService>();

                services.AddSingleton<NavigationService>();
                services.AddSingleton<ThemeSelectorService>();

                services.AddSingleton<UACService>();

                services.AddSingleton<WindowsRemoteManagementClient>();
                services.AddSingleton<WindowsManagementInstrumentationClient>();

                services.AddSingleton<ClientConnectionManager>();

                services.AddSingleton<LocalFileExplorer>();
                services.AddSingleton<NetworkFileExplorer>();

                services.AddTransient<IConfigurationManagerClientService, ConfigurationManagerClientService>();

                services.AddSingleton<LocalProcessExecuter>();

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

#if DEBUG
                services.AddSingleton<WinRMDebugPageViewModel>();
                services.AddSingleton<SMBDebugPageViewModel>();
                services.AddSingleton<ProcessDebugPageViewModel>();
#endif

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

        if (RuntimeHelper.IsPackaged)
        {
            WindowManager.PersistenceStorage = HostedService.Services.GetRequiredService<LocalSettingsService>().UserSettings.WinUIExPersistence;
        }
    }

    ~App()
    {
        HostedService.StopAsync().ConfigureAwait(false).GetAwaiter().GetResult();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        DispatcherQueue = DispatcherQueue.GetForCurrentThread();

        var splashScreen = new SplashScreen(typeof(MainWindow));
        splashScreen.Completed += (s, e) =>
        {
            m_window = e as MainWindow ?? throw new Exception("Failed to load");
            m_window.Closed += (s, e) =>
            {
                HostedService.Services.GetRequiredService<LocalSettingsService>()!.SaveSettingsAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            };
            AppTitlebar = (m_window as MainWindow)!.GetAppTitleBar();

            var themeService = Current.Services.GetService<ThemeSelectorService>()!;
            if (themeService.Theme != ElementTheme.Default)
            {
                themeService.SetThemeAsync(themeService.Theme).ConfigureAwait(false).GetAwaiter().GetResult();
            }
        };
    }

    private WindowEx m_window;

    internal WindowEx GetActiveWindow()
    {
        return m_window;
    }

    internal static UIElement? AppTitlebar { get; set; }
}
