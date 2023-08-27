using CCMEXEC;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Extensions;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.WMI;
using FluentResults;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text.Encodings.Web;
using System.Xml.Linq;
using WSManAutomation;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Services
{
    public class WindowsRemoteManagementClient : IDisposable, IWindowsManagementInstrumentationConnection
    {
        public bool IsConnected => _session != null;
        
        private readonly WSMan _instance;
        private IWSManSession _session;

        private readonly ILogger<WindowsRemoteManagementClient> _logger;

        public WindowsRemoteManagementClient(ILogger<WindowsRemoteManagementClient> logger)
        {
            _logger = logger;

            _instance = new WSMan();
        }

        public void Dispose()
        {
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

                return Result.Ok();
            }
            catch(UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Failed to connect to WinRM on Host {host}. Access is denied", host);
                return Result.Fail(new Error("Access denied. Invalid username or password?").CausedBy(ex));
            }
            catch(COMException ex)
            {
                _logger.LogError(ex, "Failed to connect to WinRM on Host {host}", host);
                return Result.Fail(new Error(ex.Message).CausedBy(ex));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to WinRM on Host {host}", host);
                return Result.Fail(new Error(ex.Message).CausedBy(ex));
            }
            finally
            {
                Marshal.ReleaseComObject(connectOptions);
            }
        }

        internal string? ExecuteQuery(string uri)
        {
            if(_session == null)
            {
                return string.Empty;
            }

            return _session.Get(uri);
        }

        internal IWSManEnumerator? Enumerate(string uri, string filter = "")
        {
            if (_session == null)
            {
                return null;
            }

            if(!string.IsNullOrEmpty(filter))
            {
                return _session.Enumerate(uri, filter, @"http://schemas.microsoft.com/wbem/wsman/1/WQL");
            }
            
            return _session.Enumerate(uri);
        }

        internal string? InvokeMethod(string uri, string methodName, Dictionary<string, object>? parameters = null)
        {
            if(_session == null)
            {
                return null;
            }

            var requestXml = $@"<p:{methodName}_INPUT xmlns:p=""{uri}"">";
            if (parameters != null)
            {
                foreach (var parameter in parameters)
                {
                    requestXml += $@"<p:{parameter.Key}>{SecurityElement.Escape(parameter.Value.ToString())}</p:{parameter.Key}>";
                }
            }
            requestXml += $@"</p:{methodName}_INPUT>";

            return _session.Invoke(methodName, uri, requestXml);
        }

        const string _windowsRemoteManagementSchema = "http://schemas.microsoft.com/wbem/wsman/1/wmi";

        const string _defaultConfigurationManagerScope = "ROOT/ccm";

        private static string BuildUri(string managementClass, string? managementScope = null, string? key = null)
        {
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

            var webQuery = UrlEncoder.Default.Encode(key.Replace("\"", ""));
            return $"{_windowsRemoteManagementSchema}/{managementScope}/{managementClass}?{webQuery}";
        }

        public T? ParseXmlElement<T>(string? xml) where T : new()
        {
            if (xml == null || string.IsNullOrWhiteSpace(xml))
            {
                return default;
            }

            var newElement = new T();
            try
            {
                var properties = typeof(T).GetProperties();
                var document = XDocument.Parse(xml);

                if(document.Root == null)
                {
                    return newElement;
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
                        if (isEmpty)
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
                        if(Enum.TryParse(property.PropertyType, element.Value, out var parsed))
                        {
                            value = parsed!;
                        }
                        else
                        {
                            _logger.LogError("Failed to parse {value} to {type}", element.Value, property.PropertyType.Name);
                            value = Enum.Parse(property.PropertyType, "0");
                        }
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
                    property.SetValue(newElement, value);
                }

                return newElement;
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
                var uri = BuildUri(instance.Namespace, instance.Class, instance.Key);
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

        //public T? InvokeMethod<T>(string managementClass, string? managementScope = null) where T : new()
        //{
        //    var uri = BuildUri(managementClass, managementScope);
        //}
    }
}
