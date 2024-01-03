using System;
using System.Threading.Tasks;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Helpers;
using Microsoft.UI.Xaml;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Services;

public class ThemeSelectorService
{
    public ElementTheme Theme { get; set; } = ElementTheme.Default;

    public event EventHandler? ThemeChanged;

    private readonly LocalSettingsService _localSettingsService;

    public ThemeSelectorService(LocalSettingsService settingsService)
    {
        _localSettingsService = settingsService;

        Theme = _localSettingsService.UserSettings.Theme;
    }

    public async Task SetThemeAsync(ElementTheme theme)
    {
        Theme = theme;
        _localSettingsService.UserSettings.Theme = theme;

        await SetRequestedThemeAsync();
        await _localSettingsService.SaveSettingsAsync();

        ThemeChanged?.Invoke(this, EventArgs.Empty);
    }

    private async Task SetRequestedThemeAsync()
    {
        if (App.Current.GetActiveWindow().Content is FrameworkElement rootElement)
        {
            rootElement.RequestedTheme = Theme;

            TitleBarHelper.UpdateTitleBar(Theme);
        }

        await Task.CompletedTask;
    }
}
