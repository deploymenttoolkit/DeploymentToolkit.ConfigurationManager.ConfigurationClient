using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI.UI.Controls.TextToolbarSymbols;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.CCM;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.CCM.ClientSDK;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.WMI;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Services;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using Vanara.Extensions.Reflection;
using WSManAutomation;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.ViewModels
{
    public partial class WinRMDebugPageViewModel : ObservableObject
    {
        [ObservableProperty]
        private string? _debugResponse;

        [ObservableProperty]
        private string? _debugQuery;
        [ObservableProperty]
        private string? _methodName;
        [ObservableProperty]
        private string? _parameters;
        [ObservableProperty]
        private int _selectedIndex;

        [ObservableProperty]
        private string? _instanceDebugResponse;
        [ObservableProperty]
        private int _instanceSelectedIndex;

        [ObservableProperty]
        private string? _instancesDebugResponse;
        [ObservableProperty]
        private int _instancesSelectedIndex;

        public ObservableCollection<Type> StaticInstances { get; } = new();
        public CollectionViewSource StaticInstancesView { get; } = new();
        public ObservableCollection<Type> Instances { get; } = new();
        public CollectionViewSource InstancesView { get; } = new();

        enum SelectedQuery
        {
            Get,
            Enumerate,
            Invoke
        }

        private readonly ILogger<WinRMDebugPageViewModel> _logger;
        private readonly WindowsRemoteManagementClient _remoteManagementClient;

        private const string _wmiUriPrefix = "http://schemas.microsoft.com/wbem/wsman/1/wmi/";

        public WinRMDebugPageViewModel(ILogger<WinRMDebugPageViewModel> logger, WindowsRemoteManagementClient windowsRemoteManagementClient)
        {
            _logger = logger;
            _remoteManagementClient = windowsRemoteManagementClient;
            if (!_remoteManagementClient.IsConnected)
            {
                _remoteManagementClient.Connect("127.0.0.1");
            }

            var staticInstances = Assembly.GetExecutingAssembly().GetTypes().Where(t => !t.IsAbstract && t.GetInterface(nameof(IWindowsManagementInstrumentationStaticInstance)) != null);
            foreach (var instance in staticInstances)
            {
                StaticInstances.Add(instance);
            }
            StaticInstancesView.Source = StaticInstances.Select(t => t.Name);

            var instances = Assembly.GetExecutingAssembly().GetTypes().Where(t => !t.IsAbstract && t.GetInterface(nameof(IWindowsManagementInstrumentationInstance)) != null);
            foreach (var instance in instances)
            {
                Instances.Add(instance);
            }
            InstancesView.Source = Instances.Select(t => t.Name);
        }

        [RelayCommand]
        private void ExecuteInstance()
        {
            var selectedType = StaticInstances[InstanceSelectedIndex];
            DebugInstance(selectedType);
        }

        [RelayCommand]
        private void ExecuteInstances()
        {
            var selectedType = Instances[InstancesSelectedIndex];
            DebugInstance(selectedType);
        }

        private void DebugInstance(Type selectedType)
        {
            try
            {
                var tempInstance = Activator.CreateInstance(selectedType);

                var instance = typeof(WindowsRemoteManagementClient)
                    .GetMethod("GetInstances")!
                    .MakeGenericMethod(selectedType)
                    .Invoke(
                        _remoteManagementClient,
                        new object[] {
                        selectedType.GetProperty("Class")!.GetValue(tempInstance)!,
                        selectedType.GetProperty("Namespace")!.GetValue(tempInstance)!
                        }
                    );

                if (instance != null)
                {
                    InstanceDebugResponse = System.Text.Json.JsonSerializer.Serialize(instance, new System.Text.Json.JsonSerializerOptions()
                    {
                        WriteIndented = true
                    });
                }
            }
            catch (Exception ex)
            {
                InstanceDebugResponse = ex.Message;
                InstanceDebugResponse += Environment.NewLine;
                InstanceDebugResponse += ex.StackTrace;
            }
        }

        [RelayCommand]
        private void ExecuteQuery()
        {
            if(string.IsNullOrEmpty(DebugQuery))
            {
                return;
            }

            if(!DebugQuery.StartsWith(_wmiUriPrefix))
            {
                DebugQuery = $"{_wmiUriPrefix}{DebugQuery}";
            }

            try
            {
                var queryType = (SelectedQuery)SelectedIndex;
                string? methodName = null;
                Dictionary<string, object>? parameters = null;

                if(queryType == SelectedQuery.Invoke)
                {
                    if(string.IsNullOrEmpty(MethodName))
                    {
                        return;
                    }

                    methodName = MethodName;

                    if(!string.IsNullOrEmpty(Parameters))
                    {
                        parameters = new();

                        var pairs = Parameters.Split(',');
                        foreach(var pair in pairs)
                        {
                            var split = pair.Split('=');
                            parameters.Add(split[0], split[1]);
                        }
                    }
                }

                var response = queryType switch
                {
                    SelectedQuery.Get => _remoteManagementClient.ExecuteQuery(DebugQuery),
                    SelectedQuery.Enumerate => EnumerateResponse(_remoteManagementClient.Enumerate(DebugQuery)),
                    SelectedQuery.Invoke => _remoteManagementClient.InvokeMethod(DebugQuery, methodName!, parameters),
                    _ => throw new NotImplementedException()
                };

                if (response != null)
                {
                    var document = XDocument.Parse(response);
                    DebugResponse = document.ToString();
                }
                else
                {
                    DebugResponse = "EMPTY RESPONSE";
                }
            }
            catch(Exception ex)
            {
                DebugResponse = ex.Message;
                DebugResponse += Environment.NewLine;
                DebugResponse += ex.StackTrace;
            }
        }

        private string EnumerateResponse(IWSManEnumerator? response)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append("<Enumeration>");
            if (response != null)
            {
                while (!response.AtEndOfStream)
                {
                    var item = response.ReadItem();
                    var document = XDocument.Parse(item);
                    stringBuilder.Append(document.ToString());
                }
            }
            stringBuilder.Append("</Enumeration>");
            return stringBuilder.ToString();
        }
    }
}
