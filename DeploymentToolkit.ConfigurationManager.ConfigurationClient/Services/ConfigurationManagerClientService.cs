using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.CCM;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.CCM.ClientSDK;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.CCM.dcm;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.CCM.Policy;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.CCM.SoftMgmtAgent;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.WMI;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Web.Syndication;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Services
{
    public class ConfigurationManagerClientService : IConfigurationManagerClientService
    {
        private readonly IWindowsManagementInstrumentationConnection _remoteManagementClient;

        private static readonly Lazy<CCM_ClientAction> _defaultClientAction = new(() => new CCM_ClientAction(false, PolicyTarget.Machine, ConfigState.Actual));

        private static readonly Lazy<CCM_InstalledComponent> _defaultInstalledComponent = new(() => new CCM_InstalledComponent());

        private static readonly Lazy<CacheInfoEx> _defaultCacheInfo = new(() => new CacheInfoEx());
        private static readonly Lazy<CacheConfig> _defaultCacheConfig = new(() => new CacheConfig() { ConfigKey = "Cache" });

        private static readonly Lazy<CCM_Client> _defaultClient = new(() => new CCM_Client());
        private static readonly Lazy<SMS_Client> _defaultSMSClient = new(() => new SMS_Client());
        private static readonly Lazy<SMS_LookupMP> _defaultSMSLookupMP = new(() => new SMS_LookupMP());
        private static readonly Lazy<CCM_ClientIdentificationInformation> _defaultClientIdentification = new(() => new CCM_ClientIdentificationInformation());
        private static readonly Lazy<CCM_ClientSiteMode> _defaultSiteMode = new(() => new CCM_ClientSiteMode());
        private static readonly Lazy<CCM_ClientUpgradeStatus> _defaultUpgradeStatus = new(() => new CCM_ClientUpgradeStatus());
        private static readonly Lazy<ClientInfo> _defaultClientInfo = new(() => new ClientInfo());

        private static readonly Lazy<PolicyNamespace> _defaultPolicy = new(() => new PolicyNamespace("Policy", null));

        private static readonly Lazy<CCM_Program> _defaultProgram = new(() => new CCM_Program());

        private static readonly Lazy<CCM_Application> _defaultApplication = new(() => new CCM_Application());

        private static readonly Lazy<CCM_SoftwareUpdate> _defaultSoftwareUpdate = new(() => new CCM_SoftwareUpdate());

        private static readonly Lazy<SMS_DesiredConfiguration> _defaultDesiredConfiguration = new(() => new SMS_DesiredConfiguration());

        public ConfigurationManagerClientService(ClientConnectionManager clientConnectionManager)
        {
            _remoteManagementClient = clientConnectionManager.Connection;
        }

        public T UpdateInstance<T>(IWindowsManagementInstrumentationInstance instance) where T : class, IWindowsManagementInstrumentationInstance, new()
        {
            return _remoteManagementClient.PatchInstance<T>(instance);
        }

        public IEnumerable<CCM_ClientAction>? GetClientActions()
        {
            return _remoteManagementClient.GetInstances<CCM_ClientAction>(_defaultClientAction.Value.Class, _defaultClientAction.Value.Namespace);
        }

        public uint PerformClientAction(CCM_ClientAction clientAction)
        {
            var result = _remoteManagementClient.InvokeMethod<ReturnCodeOnlyResult>(_defaultSMSClient.Value, "TriggerSchedule", new()
            {
                { "sScheduleID", clientAction.ActionID }
            });
            return result?.ReturnValue ?? uint.MaxValue;
        }

        public IEnumerable<CCM_InstalledComponent>? GetInstalledComponents()
        {
            return _remoteManagementClient.GetInstances<CCM_InstalledComponent>(_defaultInstalledComponent.Value.Class, _defaultInstalledComponent.Value.Namespace);
        }

        public IEnumerable<CacheInfoEx>? GetClientCache()
        {
            return _remoteManagementClient.GetInstances<CacheInfoEx>(_defaultCacheInfo.Value.Class, _defaultCacheInfo.Value.Namespace);
        }

        public CacheConfig? GetClientCacheConfig()
        {
            return _remoteManagementClient.GetInstance<CacheConfig>(_defaultCacheConfig.Value);
        }

        public CCM_Client? GetClient()
        {
            return _remoteManagementClient.GetStaticInstance<CCM_Client>(_defaultClient.Value);
        }

        public SMS_Client? GetSMSClient()
        {
            return _remoteManagementClient.GetStaticInstance<SMS_Client>(_defaultSMSClient.Value);
        }

        public SMS_LookupMP? GetSMSLookupMP()
        {
            return _remoteManagementClient.GetInstances<SMS_LookupMP>(_defaultSMSLookupMP.Value)?.FirstOrDefault();
        }

        public CCM_ClientIdentificationInformation? GetClientIdentificationInformation()
        {
            return _remoteManagementClient.GetStaticInstance<CCM_ClientIdentificationInformation>(_defaultClientIdentification.Value);
        }

        public CCM_ClientSiteMode? GetClientSiteMode()
        {
            return _remoteManagementClient.GetStaticInstance<CCM_ClientSiteMode>(_defaultSiteMode.Value);
        }

        public CCM_ClientUpgradeStatus? GetClientUpgradeStatus()
        {
            return _remoteManagementClient.GetStaticInstance<CCM_ClientUpgradeStatus>(_defaultUpgradeStatus.Value);
        }

        public ClientInfo? GetClientInfo()
        {
            return _remoteManagementClient.GetStaticInstance<ClientInfo>(_defaultClientInfo.Value);
        }

        public IEnumerable<PolicyNamespace> GetPolicy()
        {
            var classes = _remoteManagementClient.GetChildNamespaces(_defaultPolicy.Value);
            if (classes == null)
            {
                yield break;
            }

            foreach(var child in classes)
            {
                var isDefault = child == "DefaultMachine" || child == "DefaultUser";
                var policyTarget = child == "Machine" ? PolicyTarget.Machine : PolicyTarget.User;
                var sid = !isDefault && policyTarget == PolicyTarget.User ? child : null;

                var childNamespace = new PolicyNamespace(child, null, isDefault, policyTarget, null, sid);

                var childClasses = _remoteManagementClient.GetChildNamespaces(childNamespace);
                if(childClasses != null)
                {
                    foreach(var childClass in childClasses)
                    {
                        var configClass = new PolicyNamespace(
                            childClass, null,
                            childNamespace.Default,
                            childNamespace.PolicyTarget,
                            childClass == "ActualConfig" ? ConfigState.Actual : ConfigState.Requested,
                            childNamespace.SID
                        );

                        childNamespace.Children.Add(configClass);
                    }
                }

                yield return childNamespace;
            }
        }

        public IEnumerable<PolicyClass> GetPolicyClasses(PolicyNamespace policy)
        {
            var classes = _remoteManagementClient.GetChildClasses(policy);
            if(classes == null)
            {
                yield break;
            }

            foreach(var child in classes)
            {
                yield return new PolicyClass(
                    child,
                    policy.Default,
                    policy.PolicyTarget,
                    policy.ConfigState,
                    policy.SID
                );
            }
        }

        public IEnumerable<PolicyInstance> GetPolicyInstances(PolicyClass policyClass)
        {
            var instanes = _remoteManagementClient.GetInstances(policyClass);
            if(instanes == null)
            {
                yield break;
            }

            foreach(var instance in instanes)
            {
                var newInstance = new PolicyInstance(
                    instance.Namespace,
                    instance.Class,
                    instance.Class,
                    policyClass.Default,
                    policyClass.PolicyTarget,
                    policyClass.ConfigState,
                    policyClass.SID
                );
                foreach(var property in instance.Properties)
                {
                    newInstance.Properties.Add(property);
                }
                yield return newInstance;
            }
        }

        public IEnumerable<CCM_Program> GetPrograms()
        {
            var instances = _remoteManagementClient.GetInstances<CCM_Program>(_defaultProgram.Value);
            if(instances == null)
            {
                yield break;
            }

            foreach(var instance in instances)
            {
                yield return instance;
            }
        }

        public IEnumerable<CCM_Application> GetApplications()
        {
            var instances = _remoteManagementClient.GetInstances<CCM_Application>(_defaultApplication.Value);
            if (instances == null)
            {
                yield break;
            }

            foreach (var instance in instances)
            {
                yield return instance;
            }
        }

        public uint InstallApplication(CCM_Application application, Priority priority, bool reboot = false) => InvokeApplicationMethod("Install", application, priority, reboot);
        public uint RepairApplication(CCM_Application application, Priority priority, bool reboot = false) => InvokeApplicationMethod("Install", application, priority, reboot);
        public uint UninstallApplication(CCM_Application application, Priority priority, bool reboot = false) => InvokeApplicationMethod("Install", application, priority, reboot);

        private uint InvokeApplicationMethod(string method, CCM_Application application, Priority priority, bool reboot)
        {
            var result = _remoteManagementClient.InvokeMethod<InvokeApplicationMethodResult>(application, method, new Dictionary<string, object>()
            {
                { "Id", application.Id },
                { "Revision", application.Revision },
                { "IsMachineTarget", application.IsMachineTarget },
                { "EnforcePreference", (uint)application.EnforcePreference },
                { "Priority", priority },
                { "IsRebootIfNeeded", reboot }
            });

            return result?.ReturnValue ?? uint.MaxValue;
        }

        public IEnumerable<CCM_SoftwareUpdate> GetSoftwareUpdates()
        {
            var instances = _remoteManagementClient.GetInstances<CCM_SoftwareUpdate>(_defaultSoftwareUpdate.Value);
            if (instances == null)
            {
                yield break;
            }

            foreach (var instance in instances)
            {
                yield return instance;
            }
        }

        public IEnumerable<SMS_DesiredConfiguration> GetDesiredStateConfiguration()
        {
            var instances = _remoteManagementClient.GetInstances<SMS_DesiredConfiguration>(_defaultDesiredConfiguration.Value);
            if (instances == null)
            {
                yield break;
            }

            foreach (var instance in instances)
            {
                yield return instance;
            }
        }

        public uint EvaluateDesiredStateConfiguration(SMS_DesiredConfiguration configuration)
        {
            var result = _remoteManagementClient.InvokeMethod<InvokeDesiredConfigurationEvaluationResult>(configuration, "TriggerEvaluation", new Dictionary<string, object>()
            {
                { "Name", configuration.Name },
                { "Version", configuration.Version },
                { "IsMachineTarget", configuration.IsMachineTarget },
                { "IsEnforced", true },
                { "PolicyType", (uint)configuration.PolicyType }
            });

            return result?.ReturnValue ?? uint.MaxValue;
        }
    }
}
