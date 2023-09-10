using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Services;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Views;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Views.Dialogs;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using Windows.Storage.Pickers;
using System.Collections.ObjectModel;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.CCM.SoftMgmtAgent;
using System.Linq;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.ViewModels
{
    public partial class CachePageViewModel : ObservableObject
    {
        public CachePage Page { get; set; }

        [ObservableProperty]
        private bool _isAdministrator;

        [ObservableProperty]
        private bool _enableContent = true;

        [ObservableProperty]
        private Visibility _errorMessageVisibility = Visibility.Collapsed;

        [ObservableProperty]
        private uint _cacheSize;

        [ObservableProperty]
        private ObservableCollection<CacheInfoEx> _cacheElements = new();
        
        private readonly UACService _uacService;
        private readonly IConfigurationManagerClientService _clientService;

        public CachePageViewModel(UACService uacService, IConfigurationManagerClientService clientService)
        {
            this._uacService = uacService;
            this._clientService = clientService;

            IsAdministrator = _uacService.IsElevated;

            if(!IsAdministrator)
            {
                _errorMessageVisibility = Visibility.Visible;
                EnableContent = false;
                return;
            }

            var clientConfig = clientService.GetClientCacheConfig();
            if(clientConfig != null )
            {
                CacheSize = clientConfig.Size;
            }

            RefreshCache();
        }

        private void RefreshCache()
        {
            CacheElements.Clear();
            var cacheItems = _clientService.GetClientCache();
            if(cacheItems == null)
            {
                return;
            }

            foreach (var element in cacheItems)
            {
                CacheElements.Add(element);
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
}
