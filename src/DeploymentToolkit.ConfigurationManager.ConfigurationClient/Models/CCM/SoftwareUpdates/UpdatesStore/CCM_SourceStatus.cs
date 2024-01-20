using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.CCM.SoftwareUpdates.UpdatesStore;

public partial class CCM_SourceStatus : ObservableObject
{
    [ObservableProperty]
    private string _sourceUniqueId;
    [ObservableProperty]
    private DateTime _scanTime;
    [ObservableProperty]
    private uint _sourceVersion;
    [ObservableProperty]
    private uint _revisionNumber;
    [ObservableProperty]
    private string _status;
    [ObservableProperty]
    private uint _sourceType;
}
