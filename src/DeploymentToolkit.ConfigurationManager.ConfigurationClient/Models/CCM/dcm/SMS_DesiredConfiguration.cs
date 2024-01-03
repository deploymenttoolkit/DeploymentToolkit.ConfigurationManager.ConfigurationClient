using CommunityToolkit.Mvvm.ComponentModel;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.WMI;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.ViewModels;
using System;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.CCM.dcm;

public enum LastComplianceStatus : uint
{
    NonCompliant,
    Compliant,
    NotApplicable,
    Unknown,
    Error,
    NotEvaluated
}

public enum PolicyType : uint
{
    DCM,
    SettingAssignment
}

public enum Status : uint
{
    Idle,
    EvaluationStarted,
    DownloadingDocuments,
    InProgress,
    Failure,
    Reporting
}

public partial class SMS_DesiredConfiguration : ObservableObject, IWindowsManagementInstrumentationInstance
{
    public string Namespace => CCM_Constants.ClientDesiredStateConfigurationNamespaace;
    public string Class => nameof(SMS_DesiredConfiguration);
    public string Key => @$"IsMachineTarget={(IsMachineTarget ? "true" : "false")},Name=""{Name}"",Version={Version}";
    public bool QueryByFilter => true;

    public ConfigurationsViewModel ViewModel { get; set; }

    [ObservableProperty]
    private string _name;

    [ObservableProperty]
    private string _displayName;

    [ObservableProperty]
    private string _version;

    [ObservableProperty]
    private bool _isMachineTarget;

    [ObservableProperty]
    private string _complianceDetails;

    [ObservableProperty]
    private LastComplianceStatus _lastComplianceStatus;

    [ObservableProperty]
    private DateTime _lastEvalTime;

    [ObservableProperty]
    private PolicyType _policyType;

    [ObservableProperty]
    private Status _status;

    public SMS_DesiredConfiguration()
    {
    }
}
