using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI.UI.Controls;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.ViewModels;

public partial class SettingsPageViewModel : ObservableObject, IDisposable
{
    public List<string> Themes { get; set; } = Enum.GetNames(typeof(ElementTheme)).ToList();

    [ObservableProperty]
    private string _selectedTheme = nameof(ElementTheme.Default);

    private readonly ThemeSelectorService _themeSelectorService;

    public SettingsPageViewModel(ThemeSelectorService themeSelectorService)
    {
        _themeSelectorService = themeSelectorService;

        _themeSelectorService.ThemeChanged += OnThemeChanged;
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
}
