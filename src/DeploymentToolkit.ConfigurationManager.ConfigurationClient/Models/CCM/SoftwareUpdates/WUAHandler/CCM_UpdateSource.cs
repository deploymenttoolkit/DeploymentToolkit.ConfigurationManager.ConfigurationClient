using CommunityToolkit.Mvvm.ComponentModel;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.WMI;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.CCM.SoftwareUpdates.WUAHandler;

public partial class CCM_UpdateSource : ObservableObject, IWindowsManagementInstrumentationInstance
{
    public string Namespace => CCM_Constants.SoftwareUpdates.WUAHandler;
    public string Class => nameof(CCM_UpdateSource);
    public string Key => $@"UniqueId=""{UniqueId}""";
    public bool QueryByFilter => false;

    [ObservableProperty]
    private string _uniqueId;
    [ObservableProperty]
    private uint _contentType;
    [ObservableProperty]
    private string _contentLocation;
    [ObservableProperty]
    private uint _contentVersion;
    [ObservableProperty]
    private string _serviceId;
    [ObservableProperty]
    private uint _supersedenceMode;
}
