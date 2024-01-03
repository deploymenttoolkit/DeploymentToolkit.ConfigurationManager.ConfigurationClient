using System;
using CommunityToolkit.Mvvm.ComponentModel;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.WMI;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.CCM.Policy;
public partial class CCM_ComponentClientConfig : ObservableObject, IWindowsManagementInstrumentationInstance
{
    public string Namespace => @$"{CCM_Constants.ClientPolicyNamespace}\Machine\ActualConfig";
    public string Class => nameof(CCM_ComponentClientConfig);
    public string Key => throw new NotImplementedException();
    public bool QueryByFilter => false;

    [ObservableProperty]
    private string _componentName;
    [ObservableProperty]
    private bool _enabled;
}
