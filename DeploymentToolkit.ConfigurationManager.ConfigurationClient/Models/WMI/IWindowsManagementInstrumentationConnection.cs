using FluentResults;
using System.Collections.Generic;
using System.ComponentModel;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.WMI
{
    public interface IWindowsManagementInstrumentationConnection : INotifyPropertyChanged
    {
        public bool IsConnected { get; }

        public Result Connect(string host, string? username = null, string? password = null, bool encrypted = true);
        public Result Disconnect();

        public T? GetInstance<T>(IWindowsManagementInstrumentationInstance instance) where T : new();
        public T PatchInstance<T>(IWindowsManagementInstrumentationInstance instance) where T : class, IWindowsManagementInstrumentationInstance, new();

        public T? GetStaticInstance<T>(IWindowsManagementInstrumentationStaticInstance instance) where T : new();
        public T? GetStaticInstance<T>(string managementClass, string? managementScope = null) where T : new();

        public IEnumerable<T>? GetInstances<T>(IWindowsManagementInstrumentationInstance instance) where T : new();
        public IEnumerable<T>? GetInstances<T>(string managementClass, string? managementScope = null) where T : new();

        public IEnumerable<DynamicWMIClass> GetInstances(IWindowsManagementInstrumentationInstance instance);
        public IEnumerable<DynamicWMIClass> GetInstances(string managementClass, string? managementScope = null);

        public IEnumerable<string>? GetChildNamespaces(IWindowsManagementInstrumentationStaticInstance instance);
        public IEnumerable<string>? GetChildClasses(IWindowsManagementInstrumentationStaticInstance instance);

        public T? InvokeMethod<T>(IWindowsManagementInstrumentationStaticInstance instance, string method, Dictionary<string, object> parameters) where T : IMethodResult, new();
    }
}
