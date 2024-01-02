using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI.UI.Controls;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.ViewModels;

public partial class SettingsPageViewModel : ObservableObject, IDisposable
{
    public List<string> Themes { get; set; } = Enum.GetNames(typeof(ElementTheme)).ToList();

    [ObservableProperty]
    private string _selectedTheme = nameof(ElementTheme.Default);

    [ObservableProperty]
    private string _defaultConnectionMethod;
    public ObservableCollection<string> ConnectionMethods = new()
    {
        //"Auto",
        "WMI",
        "WinRM"
    };
    [ObservableProperty]
    private string _defaultFileConnectionMethod;
    public ObservableCollection<string> FileConnectionMethods = new()
    {
        "Windows",
        "SMBClient"
    };

    private readonly LocalSettingsService _localSettingsService;
    private readonly ThemeSelectorService _themeSelectorService;

    public SettingsPageViewModel(LocalSettingsService localSettingsService, ThemeSelectorService themeSelectorService)
    {
        _localSettingsService = localSettingsService;
        _themeSelectorService = themeSelectorService;

        _themeSelectorService.ThemeChanged += OnThemeChanged;

        DefaultConnectionMethod = _localSettingsService.UserSettings.DefaultConnectionMethod.ToString();
        DefaultFileConnectionMethod = _localSettingsService.UserSettings.DefaultFileConnectionMethod.ToString();
    }

    public void Dispose()
    {
        _themeSelectorService.ThemeChanged -= OnThemeChanged;
        GC.SuppressFinalize(this);
    }

    private void OnThemeChanged(object? sender, EventArgs? e)
    {
        App.Current.DispatcherQueue.TryEnqueue(() =>
        {
            SelectedTheme = Enum.GetName(_themeSelectorService.Theme) ?? nameof(ElementTheme.Default);
        });
    }

    [RelayCommand]
    private async Task ThemeChanged(SelectionChangedEventArgs e)
    {
        if(!Enum.TryParse<ElementTheme>(SelectedTheme, true, out var selectedTheme))
        {
            return;
        }

        await _themeSelectorService.SetThemeAsync(selectedTheme);
    }

    [RelayCommand]
    private async Task ConnectionMethodChanged()
    {
        if (!Enum.TryParse<ConnectionMethod>(DefaultConnectionMethod, true, out var selectedConnectionMethod))
        {
            return;
        }

        _localSettingsService.UserSettings.DefaultConnectionMethod = selectedConnectionMethod;
        await _localSettingsService.SaveSettingsAsync();
    }

    [RelayCommand]
    private async Task FileConnectionMethodChanged()
    {
        if (!Enum.TryParse<FileConnectionMethods>(DefaultFileConnectionMethod, true, out var selectedConnectionMethod))
        {
            return;
        }

        _localSettingsService.UserSettings.DefaultFileConnectionMethod = selectedConnectionMethod;
        await _localSettingsService.SaveSettingsAsync();
    }
}
