using FluentResults;
using System.Collections.Generic;
using System.ComponentModel;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.WMI;

public interface IWindowsManagementInstrumentationConnection : INotifyPropertyChanged
{
    public bool IsConnected { get; }
    public bool IsLocalConnection { get; }

    public Result Connect(string host, string? username = null, string? password = null, bool encrypted = true);
    public Result Disconnect();

    public T? GetInstance<T>(IWindowsManagementInstrumentationInstance instance) where T : new();
    public T UpdateInstance<T>(IWindowsManagementInstrumentationInstance instance) where T : class, IWindowsManagementInstrumentationInstance, new();
    public T PutInstance<T>(IWindowsManagementInstrumentationInstance instance, params string[] updatedProperties) where T : class, IWindowsManagementInstrumentationInstance, new();

    public T? GetStaticInstance<T>(IWindowsManagementInstrumentationStaticInstance instance) where T : new();
    public T? GetStaticInstance<T>(string managementClass, string? managementScope = null) where T : new();

    public IEnumerable<T>? GetInstances<T>(IWindowsManagementInstrumentationInstance instance) where T : new();
    public IEnumerable<T>? GetInstances<T>(string managementClass, string? managementScope = null) where T : new();

    public IEnumerable<DynamicWMIClass> GetInstances(IWindowsManagementInstrumentationInstance instance);
    public IEnumerable<DynamicWMIClass> GetInstances(string managementClass, string? managementScope = null);

    public IEnumerable<string>? GetChildNamespaces(IWindowsManagementInstrumentationStaticInstance instance);
    public IEnumerable<string>? GetChildClasses(IWindowsManagementInstrumentationStaticInstance instance);

    public T? InvokeStaticMethod<T>(IWindowsManagementInstrumentationStaticInstance instance, string method, Dictionary<string, object>? parameters = null) where T : IMethodResult, new();
    public T? InvokeMethod<T>(IWindowsManagementInstrumentationInstance instance, string method, Dictionary<string, object>? parameters = null) where T : IMethodResult, new();
}
