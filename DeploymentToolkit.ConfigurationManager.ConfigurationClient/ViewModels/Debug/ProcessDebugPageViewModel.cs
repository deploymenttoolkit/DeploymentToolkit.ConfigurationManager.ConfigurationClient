﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Services;
using System;
using System.Threading.Tasks;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.ViewModels
{
    public partial class ProcessDebugPageViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool _isLoading = false;

        [ObservableProperty]
        private string _filePath;
        [ObservableProperty]
        private string _arguments;

        [ObservableProperty]
        private string _output;

        [ObservableProperty]
        private string _hostname;
        [ObservableProperty]
        private string _username = $"{Environment.GetEnvironmentVariable("Username")}@{Environment.GetEnvironmentVariable("USERDNSDOMAIN")}";
        [ObservableProperty]
        private string _password;

        [ObservableProperty]
        private int _selectedIndex;

        private readonly LocalProcessExecuter _localProcessExecuter;
        private readonly WindowsManagementInstrumentationClient _windowsManagementInstrumentationClient;

        public ProcessDebugPageViewModel(LocalProcessExecuter localProcessExecuter, WindowsManagementInstrumentationClient windowsManagementInstrumentationClient)
        {
            _localProcessExecuter = localProcessExecuter;
            _windowsManagementInstrumentationClient = windowsManagementInstrumentationClient;
        }

        private IProcessExecuter GetClient()
        {
            return SelectedIndex == 0 ? _localProcessExecuter : SelectedIndex == 1 ? _windowsManagementInstrumentationClient : throw new NotImplementedException();
        }

        [RelayCommand]
        private void Execute()
        {
            if(string.IsNullOrEmpty(FilePath))
            {
                return;
            }

            IsLoading = true;

            Task.Factory.StartNew(() =>
            {
                string? arguments = null;
                if (!string.IsNullOrEmpty(Arguments))
                {
                    arguments = Arguments;
                }

                try
                {
                    if (GetClient().TryExecute(FilePath, arguments, out var output))
                    {
                        App.Current.DispatcherQueue.TryEnqueue(() =>
                        {
                            Output = output;
                            if (string.IsNullOrEmpty(Output))
                            {
                                Output = "Empty output";
                            }
                            IsLoading = false;
                        });
                        return;
                    }
                }
                catch (Exception ex)
                {
                    App.Current.DispatcherQueue.TryEnqueue(() =>
                    {
                        Output = ex.ToString();
                    });
                }

                App.Current.DispatcherQueue.TryEnqueue(() =>
                {
                    Output = "Failed to start process or wait for it to exit";
                    IsLoading = false;
                });
            });
        }
    }
}
