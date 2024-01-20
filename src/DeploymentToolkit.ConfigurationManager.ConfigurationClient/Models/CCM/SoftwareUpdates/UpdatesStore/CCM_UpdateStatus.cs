using CommunityToolkit.Mvvm.ComponentModel;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.WMI;
using System;
using System.Collections.ObjectModel;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.CCM.SoftwareUpdates.UpdatesStore;

public partial class CCM_UpdateStatus : ObservableObject, IWindowsManagementInstrumentationInstance
{
    public string Namespace => CCM_Constants.SoftwareUpdates.UpdatesStore;
    public string Class => nameof(CCM_UpdateStatus);
    public string Key => $@"UniqueId=""019f2620-19db-48c1-ae23-5ee3bcac293d""";
    public bool QueryByFilter => false;

    [ObservableProperty]
    private string _uniqueId;
    [ObservableProperty]
    private string _title;
    [ObservableProperty]
    private string _bulletin;
    [ObservableProperty]
    private string _article;
    [ObservableProperty]
    private string _language;
    [ObservableProperty]
    private string _productID;
    [ObservableProperty]
    private string _updateClassification;
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
    [ObservableProperty]
    private bool _excludeForStateReporting;

    public ObservableCollection<CCM_SourceStatus> Sources;
}
