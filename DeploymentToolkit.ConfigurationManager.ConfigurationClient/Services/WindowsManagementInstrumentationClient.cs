using CommunityToolkit.Mvvm.ComponentModel;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.CCM;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.WMI;
using FluentResults;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml.Controls.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Services
{
    public partial class WindowsManagementInstrumentationClient : ObservableObject, IDisposable, IWindowsManagementInstrumentationConnection
    {
        [ObservableProperty]
        private bool _isConnected;

        private readonly ILogger<WindowsManagementInstrumentationClient> _logger;

        private ManagementScope? _clientManagementScope;

        private string? _host;
        private ConnectionOptions _connectionOptions = new();

        public WindowsManagementInstrumentationClient(ILogger<WindowsManagementInstrumentationClient> logger)
        {
            _logger = logger;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        private ManagementScope GetManagementScope(string? scope)
        {
            if(scope == null)
            {
                if (_clientManagementScope != null)
                {
                    return _clientManagementScope;
                }
                return GetManagementScope(CCM_Constants.ClientNamespace);
            }

            var scopeName = scope;
            if(_host != null)
            {
                scopeName = @$"\\{_host}\{scope}";
            }

            _logger.LogDebug("Getting scope {scope}", scope);

            var managementScope = new ManagementScope(scope, _connectionOptions);
            managementScope.Connect();
            return managementScope;
        }

        public Result Connect(string host, string? username = null, string? password = null, bool encrypted = true)
        {
            if(host == "127.0.0.1" || host == "::1" || host == "localhost" || host == Environment.GetEnvironmentVariable("ComputerName"))
            {
                _host = null;
            }
            else
            {
                _host = host;
            }

            if(username != null && password != null)
            {
                _connectionOptions.Username = username;
                _connectionOptions.Password = password;
            }
            else
            {
                // Needs to be set to null (not empty string) so no credentials are attempted during connect
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                _connectionOptions.Username = null;
                _connectionOptions.Password = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            }

            try
            {
                _clientManagementScope = GetManagementScope(CCM_Constants.ClientNamespace);
                IsConnected = true;
                return Result.Ok();
            }
            catch(ManagementException ex)
            {
                _logger.LogError(ex, "Failed to connect to WMI on {host}", host);
                IsConnected = false;
                return Result.Fail(new Error($"Failed to connect to WMI on computer {host ?? "localhost"}").CausedBy(ex));
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to {host}", host);
                IsConnected = false;
                return Result.Fail(new Error($"Failed to connect to computer {host ?? "localhost"}").CausedBy(ex));
            }
        }

        public Result Disconnect()
        {
            IsConnected = false;
            _clientManagementScope = null;
            return Result.Ok();
        }

        private T? ConvertManagementObject<T>(ManagementBaseObject managementObject) where T : new()
        {
            if(managementObject == null)
            {
                return default;
            }

            var newElement = new T();
            var properties = typeof(T).GetProperties();

            foreach(var wmiProperty in managementObject.Properties)
            {
                var property = properties.FirstOrDefault(p => p.Name == wmiProperty.Name);
                if (property == null)
                {
                    _logger.LogWarning("Property {name} not found on target {type}", wmiProperty.Name, typeof(T).FullName);
                    continue;
                }

                _logger.LogTrace("Parsing {name} as {type} with '{value}'", wmiProperty.Name, property.PropertyType.Name, wmiProperty.Value);

                if(wmiProperty.Value != null && wmiProperty.Value.GetType() == property.PropertyType)
                {
                    property.SetValue(newElement, wmiProperty.Value);
                }
                else if (wmiProperty.Value != null && property.PropertyType.IsEnum)
                {
                    if (Enum.IsDefined(property.PropertyType, wmiProperty.Value))
                    {
                        property.SetValue(newElement, Enum.ToObject(property.PropertyType, wmiProperty.Value));
                    }
                    else
                    {
                        _logger.LogWarning("Failed to parse {value} to {propertyType}", wmiProperty.Value, property.PropertyType.Name);
                    }
                }
                else if(property.PropertyType == typeof(DateTime))
                {
                    if (wmiProperty.Value == null || wmiProperty.Value.ToString() == "00000000000000.000000+000")
                    {
                        property.SetValue(newElement, DateTime.MinValue);
                    }
                    else
                    {
                        property.SetValue(newElement, ManagementDateTimeConverter.ToDateTime(wmiProperty.Value as string));
                    }
                }
                else if(wmiProperty.Value != null)
                {
                    property.SetValue(newElement, wmiProperty.Value);
                }

                _logger.LogDebug("Set {name} to '{value}'", wmiProperty.Name, property.GetValue(newElement));
            }

            return newElement;
        }

        public T? GetInstance<T>(IWindowsManagementInstrumentationInstance instance) where T : new()
        {
            var managementScope = GetManagementScope(instance.Namespace);
            var managementPath = new ManagementPath($"{managementScope.Path}:{instance.Class}.{instance.Key}");
            var managementObject = new ManagementObject(managementPath);
            managementObject.Get();
            return ConvertManagementObject<T>(managementObject);
        }

        public T? GetStaticInstance<T>(IWindowsManagementInstrumentationStaticInstance instance) where T : new()
        {
            return GetStaticInstance<T>(instance.Class, instance.Namespace);
        }

        public T? GetStaticInstance<T>(string managementClass, string? managementScope = null) where T : new()
        {
            var mScope = GetManagementScope(managementScope);
            var mPath = new ManagementPath($"{mScope.Path}:{managementClass}=@");
            var mObject = new ManagementObject(mPath);
            mObject.Get();
            return ConvertManagementObject<T>(mObject);
        }

        public IEnumerable<T>? GetInstances<T>(IWindowsManagementInstrumentationInstance instance) where T : new()
        {
            return GetInstances<T>(instance.Class, instance.Namespace);
        }

        public IEnumerable<T>? GetInstances<T>(string managementClass, string? managementScope = null) where T : new()
        {
            var mScope = GetManagementScope(managementScope);
            var searcher = new ManagementObjectSearcher(mScope, new ObjectQuery($"SELECT * FROM {managementClass}"));
            
            foreach(var result in searcher.Get())
            {
                yield return ConvertManagementObject<T>(result)!;
            }
        }

        public IEnumerable<DynamicWMIClass> GetInstances(IWindowsManagementInstrumentationInstance instance)
        {
            return GetInstances(instance.Class, instance.Namespace);
        }

        public IEnumerable<DynamicWMIClass> GetInstances(string managementClass, string? managementScope = null)
        {
            var mScope = GetManagementScope(managementScope);
            var searcher = new ManagementObjectSearcher(mScope, new ObjectQuery($"SELECT * FROM {managementClass}"));

            foreach(var result in searcher.Get())
            {
                var newElement = new DynamicWMIClass(result.ClassPath.Path, result.ClassPath.ClassName);
                foreach(var property in result.Properties)
                {
                    newElement.Properties.Add(new WindowsManagementInstrumentationProperty(property.Name, property.Value));
                }

                yield return newElement;
            }
        }

        public IEnumerable<string>? GetChildNamespaces(IWindowsManagementInstrumentationStaticInstance instance)
        {
            var mScope = GetManagementScope(instance.Namespace);
            var searcher = new ManagementObjectSearcher(mScope, new ObjectQuery($"SELECT * FROM __NAMESPACE"));
            foreach(var child in searcher.Get())
            {
                yield return child.GetPropertyValue("Name") as string ?? throw new InvalidOperationException();
            }
        }

        public IEnumerable<string>? GetChildClasses(IWindowsManagementInstrumentationStaticInstance instance)
        {
            var mScope = GetManagementScope(instance.Namespace);
            var searcher = new ManagementObjectSearcher(mScope, new ObjectQuery($"SELECT * FROM meta_class"));
            foreach (var child in searcher.Get())
            {
                // Skip system classes
                if (child.ClassPath.Path.Contains("__") || child.ClassPath.Path.Contains("CIM") || child.ClassPath.Path.Contains("MSFT"))
                {
                    continue;
                }

                yield return child.ClassPath.ClassName;
            }
        }
    }
}
