using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.ViewModels;

public partial class DeviceRegistrationViewModel : ObservableObject
{
    private static readonly Regex _headerRegex = new(@"^\|\s(.*)\|$");
    private static readonly Regex _propertyRegex = new(@"^\s*(.*)\s:\s(.*)$");

    public ObservableCollection<ClientProperty> Properties { get; set; } = new();
    public CollectionViewSource PropertiesViewSource { get; } = new CollectionViewSource()
    {
        IsSourceGrouped = true
    };

    [ObservableProperty]
    private string _processOutput;

    [ObservableProperty]
    private DateTime _lastUpdated;

    [ObservableProperty]
    private bool _isLoading = true;

    [ObservableProperty]
    private Visibility _contentVisibility = Visibility.Collapsed;

    private readonly ClientConnectionManager _clientConnectionManager;

    public DeviceRegistrationViewModel(ClientConnectionManager clientConnectionManager)
    {
        _clientConnectionManager = clientConnectionManager;

        Task.Factory.StartNew(() => UpdateProperties());
    }

    [RelayCommand]
    private void UpdateProperties()
    {
        App.Current.DispatcherQueue.TryEnqueue(() =>
        {
            IsLoading = true;
            ContentVisibility = Visibility.Collapsed;
        });

        if(!_clientConnectionManager.ProcessExecuter.TryExecute(@"C:\Windows\System32\dsregcmd.exe", "/status /verbose", out var output))
        {
            ProcessOutput = "DeviceRegistrationPage/Error_FailedToLaunchProcess".GetLocalized();
            return;
        }

        var lines = output.Split(Environment.NewLine);

        App.Current.DispatcherQueue.TryEnqueue(() =>
        {
            ProcessOutput = string.Empty;
            var currentGroup = string.Empty;
            for(var i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                ProcessOutput += line + Environment.NewLine;

                var headerMatch = _headerRegex.Match(line);
                if (headerMatch.Success)
                {
                    currentGroup = headerMatch.Groups[1].Value.Trim();
                    continue;
                }

                var propertyMatch = _propertyRegex.Match(line);
                if (propertyMatch.Success)
                {
                    var name = propertyMatch.Groups[1].Value.Trim();
                    var property = Properties.FirstOrDefault(p => p.Name == name);
                    if (property == null)
                    {
                        Properties.Add(new ClientProperty()
                        {
                            Group = currentGroup,
                            Name = name,
                            Value = propertyMatch.Groups[2].Value.Trim()
                        });
                        continue;
                    }
                    property.Value = propertyMatch.Groups[2].Value.Trim();
                }
            }

            PropertiesViewSource.Source = Properties.GroupBy(p => p.Group);
            LastUpdated = DateTime.Now;
            IsLoading = false;
            ContentVisibility = Visibility.Visible;
        });
    }
}
