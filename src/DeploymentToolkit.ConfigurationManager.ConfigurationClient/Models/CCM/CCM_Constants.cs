﻿namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.CCM;

internal static class CCM_Constants
{
    internal const string ClientNamespace = @"ROOT\ccm";
    internal const string ClientSDKNamespace = @"ROOT\ccm\ClientSDK";
    internal const string ClientSoftMgmtAgentNamespace = @"ROOT\ccm\SoftMgmtAgent";
    internal const string ClientPolicyNamespace = @"ROOT\ccm\Policy";
    internal const string ClientDesiredStateConfigurationNamespaace = @"ROOT\ccm\dcm";
    internal const string ClientEventsNamespace = @"root\CCM\Events";

    internal static class SoftwareUpdates
    {
        internal const string UpdatesStore = @"ROOT\ccm\SoftwareUpdates\UpdatesStore";
        internal const string WUAHandler = @"ROOT\ccm\SoftwareUpdates\WUAHandler";
    }
}
