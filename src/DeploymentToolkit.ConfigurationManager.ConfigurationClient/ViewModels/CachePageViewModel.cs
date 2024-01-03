using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Services;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Views;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Views.Dialogs;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.ObjectModel;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.CCM.SoftMgmtAgent;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.ViewModels;

public partial class CachePageViewModel : ObservableObject
{
#if DEBUG
    public Visibility DebugVisibility { get; set; } = Visibility.Visible;
#else
public Visibility DebugVisibility { get; set; } = Visibility.Collapsed;
#endif

    [ObservableProperty]
    private bool _isLoading = true;

    public CachePage Page { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EnableContent))]
    private bool _isAdministrator;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EnableContent))]
    private bool _hasCache = true;

    public bool EnableContent => IsAdministrator && HasCache;

    [ObservableProperty]
    private uint _cacheSize;

    [ObservableProperty]
    private ObservableCollection<CacheInfoEx> _cacheElements = new();

    private readonly ILogger<CachePageViewModel> _logger;
    private readonly UACService _uacService;
    private readonly IConfigurationManagerClientService _clientService;

    public CachePageViewModel(ILogger<CachePageViewModel> logger, UACService uacService, IConfigurationManagerClientService clientService)
    {
        _logger = logger;
        _uacService = uacService;
        _clientService = clientService;

        IsAdministrator = _uacService.IsElevated;

        if(!IsAdministrator)
        {
            IsLoading = false;
            return;
        }

        Task.Factory.StartNew(RefreshCache);
    }

    private void RefreshCache()
    {
        try
        {
            App.Current.DispatcherQueue.TryEnqueue(() =>
            {
                IsLoading = true;
                CacheElements.Clear();
            });

            var clientConfig = _clientService.GetClientCacheConfig();
            if (clientConfig != null)
            {
                App.Current.DispatcherQueue.TryEnqueue(() =>
                {
                    CacheSize = clientConfig.Size;
                });
            }

            var cacheItems = _clientService.GetClientCache();
            if (cacheItems == null)
            {
                return;
            }

            foreach (var element in cacheItems)
            {
                App.Current.DispatcherQueue.TryEnqueue(() =>
                {
                    CacheElements.Add(element);
                });
            }
        }
        catch(Exception ex)
        {
            _logger.LogWarning(ex, "Failed to update Cache");
            HasCache = false;
        }
        finally
        {
            App.Current.DispatcherQueue.TryEnqueue(() =>
            {
                IsLoading = false;
            });
        }
    }

    [RelayCommand]
    private void DeleteFromCache(string cacheElementId)
    {
        // TODO: Implement
        //_clientService.DeleteFromCache(cacheElementId);

        App.Current.DispatcherQueue.TryEnqueue(() =>
        {
            RefreshCache();
        });
    }

    [RelayCommand]
    private void RestartButton()
    {
        _uacService.RestartAsAdmin();
    }

    [RelayCommand]
    private async void ChangeLocation()
    {
        //var openPicker = new FolderPicker()
        //{
        //    SuggestedStartLocation = PickerLocationId.ComputerFolder,
        //    ViewMode = PickerViewMode.List
        //};
        //// Set options for your folder picker
        //openPicker.FileTypeFilter.Add("*");

        //// Retrieve the window handle (HWND) of the current WinUI 3 window.
        //var window = App.Current.GetActiveWindow();
        //var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);

        //// Initialize the folder picker with the window handle (HWND).
        //WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);

        //// Open the picker for the user to pick a folder
        //var folder = await openPicker.PickSingleFolderAsync();
        //if (folder != null)
        //{
            
        //}
        //else
        //{
            
        //}
    }

    [RelayCommand]
    private async void DeleteFiles()
    {
        var dialog = new ContentDialog
        {
            XamlRoot = Page.XamlRoot,
            Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
            Title = "Delete Cache?",
            PrimaryButtonText = "Delete",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Close,
            IsSecondaryButtonEnabled = false,
            Content = new CacheDeleteDialog()
        };

        var result = await dialog.ShowAsync();
        if(result == ContentDialogResult.Primary)
        {
            var deletePersistent = (dialog.Content as CacheDeleteDialog)!.DeletePersistentContent;
        }
    }

    [RelayCommand]
    private async void ApplyCacheSize()
    {
        if(CacheSize <= 0)
        {
            return;
        }

        var cacheConfig = _clientService.GetClientCacheConfig();
        if(cacheConfig == null || cacheConfig.Size == CacheSize)
        {
            return;
        }

        cacheConfig.Size = CacheSize;
        cacheConfig = _clientService.PutInstance<CacheConfig>(cacheConfig, nameof(cacheConfig.Size));
        CacheSize = cacheConfig.Size;

        var dialog = new ContentDialog
        {
            XamlRoot = Page.XamlRoot,
            Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
            Title = "Restart Service?",
            PrimaryButtonText = "Restart",
            CloseButtonText = "Do not restart",
            DefaultButton = ContentDialogButton.Close,
            IsSecondaryButtonEnabled = false,
            Content = new RestartServiceDialog()
        };

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            try
            {
                await _clientService.RestartServiceAsync();
            }
            catch (TimeoutException) { }
        }
    }
}
