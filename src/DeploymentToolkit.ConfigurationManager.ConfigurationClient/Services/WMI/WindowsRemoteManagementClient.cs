using CommunityToolkit.Mvvm.ComponentModel;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Extensions;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.CCM.ClientSDK;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.cimv2;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.WMI;
using FluentResults;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using System.Xml.Linq;
using WSManAutomation;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Services;

public partial class WindowsRemoteManagementClient : ObservableObject, IDisposable, IWindowsManagementInstrumentationConnection, IProcessExecuter
{
    [ObservableProperty]
    private bool _isConnected;
    
    private readonly WSMan _instance;
    private IWSManSession? _session;

    private readonly ILogger<WindowsRemoteManagementClient> _logger;

    public WindowsRemoteManagementClient(ILogger<WindowsRemoteManagementClient> logger)
    {
        _logger = logger;

        _instance = new WSMan();
    }

    public void Dispose()
    {
        IsConnected = false;

        if(_session != null)
        {
            Marshal.ReleaseComObject(_session);
        }
        if(_instance != null)
        {
            Marshal.ReleaseComObject(_instance);
        }

        GC.SuppressFinalize(this);
    }

    public Result Connect(string host, string? username = null, string? password = null, bool encrypted = true)
    {
        host = $"http://{host}:5985";

        if(_session != null)
        {
            Marshal.ReleaseComObject(_session);
        }

        var connectOptions = (IWSManConnectionOptions)_instance.CreateConnectionOptions();

        var flags = _instance.SessionFlagUTF8();
        if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
        {
            connectOptions.UserName = username;
            connectOptions.Password = password;
            flags |= _instance.SessionFlagCredUsernamePassword();
            _logger.LogTrace("Connecting with {username}", username);
        }
        else
        {
            flags |= _instance.SessionFlagAllowNegotiateImplicitCredentials();
            _logger.LogTrace("Connecting with {username}", Environment.UserName);
        }

        if(!encrypted)
        {
            flags |= _instance.SessionFlagNoEncryption();
        }

        try
        {
            _session = _instance.CreateSession(host, flags, connectOptions);

            var identity = _session.Identify();
            _logger.LogDebug("Identity: {identity}", identity);

            _logger.LogInformation("Sucessfully connected to {host}", host);

            IsConnected = true;

            return Result.Ok();
        }
        catch(UnauthorizedAccessException ex)
        {
            IsConnected = false;
            _logger.LogError(ex, "Failed to connect to WinRM on Host {host}. Access is denied", host);
            return Result.Fail(new Error("Access denied. Invalid username or password?").CausedBy(ex));
        }
        catch(COMException ex)
        {
            IsConnected = false;
            _logger.LogError(ex, "Failed to connect to WinRM on Host {host}", host);
            return Result.Fail(new Error(ex.Message).CausedBy(ex));
        }
        catch (Exception ex)
        {
            IsConnected = false;
            _logger.LogError(ex, "Failed to connect to WinRM on Host {host}", host);
            return Result.Fail(new Error(ex.Message).CausedBy(ex));
        }
        finally
        {
            Marshal.ReleaseComObject(connectOptions);
        }
    }

    public Result Disconnect()
    {
        if(_session == null)
        {
            return Result.Ok();
        }

        try
        {
            Marshal.ReleaseComObject(_session);
        }
        catch (COMException ex)
        {
            _logger.LogError(ex, "Failed to release session");
            return Result.Fail(new Error("Failed to release session").CausedBy(ex));
        }
        finally
        {
            _session = null;
            IsConnected = false;
        }
        return Result.Ok();
    }

    public bool TryExecute(string filePath, string? arguments, out string output, int timeout = 30000)
    {
        output = string.Empty;

        try
        {
            var outputFile = $@"C:\Windows\Temp\dtk_{Guid.NewGuid()}.txt";

            var startupInformation = new Win32_ProcessStartup()
            {
                ShowWindow = 0
            };

            var result = InvokeStaticMethod<InvokeWin32ProcessCreateResult>(new Win32_Process(), "Create", new()
            {
                { "CommandLine", $@"CMD.EXE /S /C "" ""{filePath}"" {arguments}  > ""{outputFile}"" """ },
                //{ "ProcessStartupInformation", startupInformation }
            });

            if (result == null)
            {
                _logger.LogWarning("Failed to start process {filePath}", filePath);
                return false;
            }

            _logger.LogDebug("Started Process with id {Id}", result.ProcessId);

            var process = new Win32_Process()
            {
                Handle = result.ProcessId.ToString()
            };

            for (var i = 0; i < timeout; i += 1000)
            {
                try
                {
                    process = UpdateInstance<Win32_Process>(process);
                    _logger.LogDebug("Checking if process {processId} is still alive", result.ProcessId);
                    Thread.Sleep(1000);
                }
                catch (COMException ex)
                {
                    process = null;
                    if (ex.HResult != -2144108544) // Resource not found
                    {
                        _logger.LogWarning("Exception: {ex}", ex);
                    }
                    break;
                }
            }

            if (process != null)
            {
                // Timeout
                _logger.LogWarning("Process with id {processId} timed out", result.ProcessId);
                return false;
            }

            var networkFileExplorer = App.Current.Services.GetService<ClientConnectionManager>()!.FileExplorerConnection;
            output = networkFileExplorer.GetFileContent(outputFile).Result;
            networkFileExplorer.RemoveFile(outputFile);

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogWarning(ex, "Failed to execute process");
            return false;
        }
    }

    internal string? ExecuteQuery(string uri)
    {
        _logger.LogTrace("Executing {uri}", uri);

        if(!IsConnected)
        {
            return string.Empty;
        }

        return _session!.Get(uri);
    }

    internal IWSManEnumerator? Enumerate(string uri, string filter = "")
    {
        if (!IsConnected)
        {
            return null;
        }

        if(!string.IsNullOrEmpty(filter))
        {
            return _session!.Enumerate(uri, filter, @"http://schemas.microsoft.com/wbem/wsman/1/WQL");
        }
        
        return _session!.Enumerate(uri);
    }

    internal string? InvokeMethod(string uri, string methodName, Dictionary<string, object>? parameters = null)
    {
        if(!IsConnected)
        {
            return null;
        }

        var requestXml = $@"<p:{methodName}_INPUT xmlns:p=""{uri}"">";
        var currentCharacter = (int)'a';
        if (parameters != null)
        {
            foreach (var parameter in parameters)
            {
                if (parameter.Value is IWSManAdvancedParameter advancedParameter)
                {
                    requestXml += $@"<p:{parameter.Key}>{advancedParameter.ToXml(((char)currentCharacter++).ToString())}</p:{parameter.Key}>";
                    if(currentCharacter == 'p')
                    {
                        currentCharacter++;
                    }
                }
                else
                {
                    requestXml += $@"<p:{parameter.Key}>{SecurityElement.Escape(parameter.Value.ToString())}</p:{parameter.Key}>";
                }
            }
        }
        requestXml += $@"</p:{methodName}_INPUT>";

        try
        {
            return _session!.Invoke(methodName, uri, requestXml);
        }
        catch(COMException ex)
        {
            _logger.LogError(ex, "Failed to invoke method {method} on {uri}", methodName, uri);
            return null;
        }
    }

    internal string? Put(string uri, Dictionary<string, object> properties)
    {
        if(!IsConnected)
        {
            return null;
        }

        var className = uri.Split('/').Last();
        if(className.Contains('?'))
        {
            className = className.Split('?')[0];
        }

        var classPath = uri;
        if(classPath.Contains('?'))
        {
            classPath = classPath.Split('?')[0];
        }

        var putXml = $@"<p:{className} xmlns:p=""{classPath}"">";
        foreach(var property in properties)
        {
            putXml += $@"<p:{property.Key}>{SecurityElement.Escape(property.Value.ToString())}</p:{property.Key}>";
        }
        putXml += $@"</p:{className}>";

        try
        {
            return _session!.Put(uri, putXml);
        }
        catch(COMException ex)
        {
            _logger.LogError(ex, "Failed to patch instance on {uri}", uri);
            return null;
        }
    }
    const string _windowsRemoteManagementSchema = "http://schemas.microsoft.com/wbem/wsman/1/wmi";
    const string _defaultConfigurationManagerScope = "ROOT/ccm";

    private static string BuildUri(string managementClass, string? managementScope = null, string? key = null)
    {
        if(managementClass.Contains("\\") || managementClass.Contains("/"))
        {
            throw new FormatException($"Invalid class {managementClass}");
        }

        if(managementScope != null)
        {
            managementScope = managementScope.Replace("\\", "/");
        }
        else
        {
            managementScope = _defaultConfigurationManagerScope;
        }

        if (key == null)
        {
            return $"{_windowsRemoteManagementSchema}/{managementScope}/{managementClass}";
        }

        var webQuery = key
            .Replace("\"", "")
            .Replace(",", "+");

        return $"{_windowsRemoteManagementSchema}/{managementScope}/{managementClass}?{webQuery}";
    }

    public T? ParseXmlElement<T>(string? xml, T? instance = default) where T : new()
    {
        if (xml == null || string.IsNullOrWhiteSpace(xml))
        {
            return default;
        }

        var newInstance = instance ?? new T();
        try
        {
            var properties = typeof(T).GetProperties();
            var document = XDocument.Parse(xml);

            if(document.Root == null)
            {
                return newInstance;
            }

            foreach(var element in document.Root.Elements())
            {
                var property = properties.FirstOrDefault(p => p.Name ==  element.Name.LocalName);
                if(property == null)
                {
                    _logger.LogWarning("Element {name} not found on target {type}", element.Name.LocalName, typeof(T).FullName);
                    continue;
                }

                _logger.LogTrace("Parsing {name} as {type} with '{value}'", element.Name.LocalName, property.PropertyType.Name, element.Value);

                object value;
                var isEmpty = string.IsNullOrEmpty(element.Value);
                if (property.PropertyType == typeof(DateTime))
                {
                    if (isEmpty || element.Value == "0000-00-00T00:00:00Z")
                    {
                        value = DateTime.MinValue;
                    }
                    else
                    {
                        value = DateTime.Parse(element.Value);
                    }
                }
                else if(property.PropertyType.IsEnum)
                {
                    var enumValue = element.Value;
                    if(enumValue.Contains(' '))
                    {
                        enumValue = enumValue.Replace(" ", "");
                    }

                    if (Enum.TryParse(property.PropertyType, enumValue, out var parsed))
                    {
                        value = parsed!;
                    }
                    else
                    {
                        _logger.LogError("Failed to parse {value} to {type}", element.Value, property.PropertyType.Name);
                        value = Enum.Parse(property.PropertyType, "0");
                    }
                }
                else if(property.PropertyType.IsObservableCollection())
                {
                    var currentValue = property.GetValue(newInstance);
                    if (isEmpty && currentValue == null)
                    {
                        value = Activator.CreateInstance(property.PropertyType)!;
                    }
                    else if(!isEmpty)
                    {
                        if (currentValue == null)
                        {
                            value = Activator.CreateInstance(property.PropertyType)!;
                        }
                        else
                        {
                            value = currentValue;
                        }

                        var itemType = property.PropertyType.GetGenericArguments()[0];
                        if (itemType.IsPrimitive || itemType == typeof(string))
                        {
                            dynamic converted = Convert.ChangeType(element.Value, itemType);
                            (value as dynamic).Add(converted);
                        }
                        else
                        {
                            // TODO: This should be handeled externally
                            switch (element.Name.LocalName)
                            {
                                case "AppDTs":
                                    (value as dynamic).Add(ParseXmlElement<CCM_Application>(element.ToString()));
                                    break;

                                case "Dependencies":
                                    (value as dynamic).Add(ParseXmlElement<CCM_Program>(element.ToString()));
                                    break;

                                default:
                                    throw new NotImplementedException("SubType not implemented");
                            }
                        }
                    }

                    _logger.LogDebug("Added to collection {name}", element.Name.LocalName);
                    continue;
                }
                else if(isEmpty && property.PropertyType.IsNumericType())
                {
                    value = Convert.ChangeType("0", property.PropertyType);
                }
                else if(isEmpty && property.PropertyType == typeof(bool))
                {
                    value = Convert.ChangeType("false", property.PropertyType);
                }
                else
                {
                    value = Convert.ChangeType(element.Value, property.PropertyType);
                }

                _logger.LogDebug("Setting {name} to '{value}'", element.Name.LocalName, value.ToString());
                property.SetValue(newInstance, value);
            }

            return newInstance;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Failed to parse");
            _logger.LogDebug("Element: {element}", xml);
            return default;
        }
    }

    private string WinRMNamespaceToWMIClass(string namespaceName)
    {
        if(!namespaceName.Contains(_windowsRemoteManagementSchema))
        {
            throw new ArgumentException(null, nameof(namespaceName));
        }

        var className = namespaceName.Replace(_windowsRemoteManagementSchema, "");
        className = className.Substring(1, className.Length - 2);
        className = className.Substring(0, className.LastIndexOf("/"));
        className = className.Replace("/", "\\");

        return className;
    }

    public DynamicWMIClass? ParseDynamicXmlElement(string? xml)
    {
        if (xml == null || string.IsNullOrWhiteSpace(xml))
        {
            return default;
        }

        try
        {
            var document = XDocument.Parse(xml);

            if (document.Root == null)
            {
                return default;
            }

            var newElement = new DynamicWMIClass(WinRMNamespaceToWMIClass(document.Root.Name.NamespaceName), document.Root.Name.LocalName);

            foreach (var element in document.Root.Elements())
            {
                newElement.Properties.Add(new WindowsManagementInstrumentationProperty(element.Name.LocalName, element.Value));
            }

            return newElement;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse");
            _logger.LogDebug("Element: {element}", xml);
            return default;
        }
    }

    public T? GetInstance<T>(IWindowsManagementInstrumentationInstance instance) where T : new()
    {
        try
        {
            var uri = BuildUri(instance.Class, instance.Namespace, instance.Key);
            _logger.LogTrace("Getting instance of {uri}", uri);
            var response = ExecuteQuery(uri);
            return ParseXmlElement<T>(response);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Failed to get instance of {namespace}->{name}->{key}", instance.Namespace, instance.Class, instance.Key);
            return default;
        }
    }

    public T UpdateInstance<T>(IWindowsManagementInstrumentationInstance instance) where T : class, IWindowsManagementInstrumentationInstance, new()
    {
        if(instance == null)
        {
            throw new ArgumentNullException(nameof(instance));
        }

        if (instance.QueryByFilter)
        {
            var uri = BuildUri("*", instance.Namespace);
            _logger.LogTrace("Getting instance of {uri} by filter (slow)", uri);

            var query = $"SELECT * FROM {instance.Class} WHERE ";
            var split = instance.Key.Split(",");
            foreach(var keyValuePair in split)
            {
                query += $"{keyValuePair} AND ";
            }
            query = query.Substring(0, query.Length - 5);

            _logger.LogTrace("Build query '{query}'", query);

            var enumeration = Enumerate(uri, query);
            if(enumeration == null || enumeration.AtEndOfStream)
            {
                _logger.LogWarning("Failed to find instance");
                return (instance as T)!;
            }

            return ParseXmlElement(enumeration.ReadItem(), instance as T)!;
        }
        else
        {
            var uri = BuildUri(instance.Class, instance.Namespace, instance.Key);
            _logger.LogTrace("Getting instance of {uri}", uri);
            var response = ExecuteQuery(uri);
            return ParseXmlElement(response, instance as T)!;
        }
    }

    public T PutInstance<T>(IWindowsManagementInstrumentationInstance instance, params string[] updatedProperties) where T : class, IWindowsManagementInstrumentationInstance, new()
    {
        if(instance == null)
        {
            throw new ArgumentNullException(nameof(instance));
        }

        try
        {
            var uri = BuildUri(instance.Class, instance.Namespace, instance.Key);
            _logger.LogTrace("Patching instance of {uri} with properties {properties}", uri, string.Join(',', updatedProperties));

            var properties = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => updatedProperties.Contains(p.Name));
            if(properties.Count() != updatedProperties.Length)
            {
                throw new InvalidOperationException($"Expected {updatedProperties.Length} but got {properties.Count()}");
            }

            var propertiesToUpdate = new Dictionary<string, object>();
            foreach(var property in properties)
            {
                var value = property.GetValue(instance);
                propertiesToUpdate.Add(property.Name, value!);
            }

            var response = Put(uri, propertiesToUpdate);
            return ParseXmlElement(response, instance as T)!;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Failed to patch instance of {namespace}->{name}->{key}", instance.Namespace, instance.Class, instance.Key);
            return (instance as T)!;
        }
    }

    public T? GetStaticInstance<T>(IWindowsManagementInstrumentationStaticInstance instance) where T : new()
    {
        return GetStaticInstance<T>(instance.Class, instance.Namespace);
    }

    public T? GetStaticInstance<T>(string managementClass, string? managementScope = null) where T : new()
    {
        var uri = BuildUri(managementClass, managementScope);
        try
        {
            var response = ExecuteQuery(uri);
            _logger.LogTrace("Getting static instance of {uri}", uri);
            return ParseXmlElement<T>(response);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Failed to execute '{uri}'", uri);
            return default;
        }
    }

    public IEnumerable<T>? GetInstances<T>(IWindowsManagementInstrumentationInstance instance) where T : new()
    {
        return GetInstances<T>(instance.Class, instance.Namespace);
    }

    public IEnumerable<T>? GetInstances<T>(string managementClass, string? managementScope = null) where T : new()
    {
        var uri = BuildUri(managementClass, managementScope);
        _logger.LogTrace("Getting enumeration of {uri}", uri);
        var enumerator = Enumerate(uri);
        if(enumerator == null)
        {
            yield break;
        }

        while(!enumerator.AtEndOfStream)
        {
            yield return ParseXmlElement<T>(enumerator.ReadItem())!;
        }
    }

    public IEnumerable<DynamicWMIClass> GetInstances(IWindowsManagementInstrumentationInstance instance)
    {
        return GetInstances(instance.Class, instance.Namespace);
    }

    public IEnumerable<DynamicWMIClass> GetInstances(string managementClass, string? managementScope = null)
    {
        var uri = BuildUri(managementClass, managementScope);
        _logger.LogTrace("Getting dynamic enumeration of {uri}", uri);
        var enumerator = Enumerate(uri);
        if (enumerator == null)
        {
            yield break;
        }

        while (!enumerator.AtEndOfStream)
        {
            yield return ParseDynamicXmlElement(enumerator.ReadItem())!;
        }
    }

    public IEnumerable<string>? GetChildNamespaces(IWindowsManagementInstrumentationStaticInstance instance)
    {
        var uri = BuildUri("__NAMESPACE", instance.Namespace);
        _logger.LogTrace("Getting subnamespaces of {uri}", uri);

        var enumeration = Enumerate(uri);
        if(enumeration == null)
        {
            yield break;
        }

        while(!enumeration.AtEndOfStream)
        {
            var document = XDocument.Parse(enumeration.ReadItem());
            if (document == null)
            {
                yield break;
            }

            yield return document.Elements().First().Value;
        }
    }

    public IEnumerable<string>? GetChildClasses(IWindowsManagementInstrumentationStaticInstance instance)
    {
        var uri = BuildUri("*", instance.Namespace);
        _logger.LogTrace("Getting subclasses of {uri}", uri);

        var enumeration = Enumerate(uri, "select * from meta_class");
        if (enumeration == null)
        {
            yield break;
        }

        while (!enumeration.AtEndOfStream)
        {
            var document = XDocument.Parse(enumeration.ReadItem());
            if (document == null)
            {
                yield break;
            }

            var className = document.Elements().First().Name.LocalName;
            if(className.StartsWith("__") || className.StartsWith("MSFT") || className.StartsWith("CIM"))
            {
                continue;
            }

            yield return className;
        }
    }

    public T? InvokeStaticMethod<T>(IWindowsManagementInstrumentationStaticInstance instance, string method, Dictionary<string, object>? parameters = null) where T : IMethodResult, new()
    {
        var uri = BuildUri(instance.Class, instance.Namespace);

        var result = InvokeMethod(uri, method, parameters);
        return ParseXmlElement<T>(result);
    }

    public T? InvokeMethod<T>(IWindowsManagementInstrumentationInstance instance, string method, Dictionary<string, object>? parameters = null) where T : IMethodResult, new()
    {
        var uri = BuildUri(instance.Class, instance.Namespace, instance.Key);

        var result = InvokeMethod(uri, method, parameters);
        return ParseXmlElement<T>(result);
    }
}
