using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.ViewModels
{
    public partial class DeviceRegistrationViewModel : ObservableObject
    {
        private static Regex _headerRegex = new Regex(@"^\|\s(.*)\|$");
        private static Regex _propertyRegex = new Regex(@"^\s*(.*)\s:\s(.*)$");

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

        public DeviceRegistrationViewModel()
        {
            UpdateProperties();
        }

        [RelayCommand]
        private async void UpdateProperties()
        {
            App.Current.DispatcherQueue.TryEnqueue(() =>
            {
                IsLoading = true;
                ContentVisibility = Visibility.Collapsed;
            });

            var process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = "dsregcmd.exe",
                    Arguments = "/status /verbose",
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true
                }
            };

            process.Start();
            await process.WaitForExitAsync();

            App.Current.DispatcherQueue.TryEnqueue(() =>
            {
                ProcessOutput = string.Empty;
                var currentGroup = string.Empty;
                var line = process.StandardOutput.ReadLine();
                do
                {
                    ProcessOutput += line + "\r\n";

                    var headerMatch = _headerRegex.Match(line);
                    if (headerMatch.Success)
                    {
                        currentGroup = headerMatch.Groups[1].Value.Trim();
                        line = process.StandardOutput.ReadLine();
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
                            line = process.StandardOutput.ReadLine();
                            continue;
                        }
                        property.Value = propertyMatch.Groups[2].Value.Trim();
                    }

                    line = process.StandardOutput.ReadLine();
                }
                while (line != null);

                PropertiesViewSource.Source = Properties.GroupBy(p => p.Group);
                LastUpdated = DateTime.Now;
                IsLoading = false;
                ContentVisibility = Visibility.Visible;
            });
        }
    }
}
