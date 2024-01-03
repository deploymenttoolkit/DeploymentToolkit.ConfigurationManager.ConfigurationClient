using CommunityToolkit.Mvvm.ComponentModel;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.WMI;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.CCM.ClientSDK;

public partial class CCM_SoftwareUpdate : CCM_SoftwareBase, IWindowsManagementInstrumentationInstance
{
    public string Namespace => CCM_Constants.ClientSDKNamespace;
    public string Class => nameof(CCM_SoftwareUpdate);
    public string Key => @$"UpdateID=""{UpdateID}""";
    public bool QueryByFilter => false;

    internal SoftwareUpdatesPageViewModel ViewModel { get; set; }
    internal ObservableCollection<ReferenceProperty> Properties { get; private set; } = new();

    [ObservableProperty]
    string _updateID;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EvaluationStateText))]
    private SoftwareUpdateEvaluationState _evaluationState;

    [ObservableProperty]
    string _bulletinID;
    [ObservableProperty]
    string _articleID;
    [ObservableProperty()]
    string _url;
    [ObservableProperty]
    bool _exclusiveUpdate;
    [ObservableProperty]
    ComplianceState _complianceState;
    [ObservableProperty]
    bool _userUIExperience;
    [ObservableProperty]
    bool _notifyUser;
    [ObservableProperty]
    DateTime _restartDeadline;
    [ObservableProperty]
    DateTime _startTime;
    [ObservableProperty]
    bool _overrideServiceWindows;
    [ObservableProperty]
    bool _rebootOutsideServiceWindows;
    [ObservableProperty]
    uint _maxExecutionTime;
    [ObservableProperty]
    bool _isUpgrade;
    [ObservableProperty]
    bool _isO365Update;

    public override string EvaluationStateText
    {
        get
        {
            if (EvaluationState == SoftwareUpdateEvaluationState.ciJobStateDownloading || EvaluationState == SoftwareUpdateEvaluationState.ciJobStateInstalling)
            {
                return $"{EvaluationState} ({PercentComplete}%)";
            }
            return EvaluationState.ToString();
        }
    }

    private static readonly List<PropertyInfo> _properties = new();
    private static readonly List<string> _propertiesToSkip = new()
    {
        nameof(Namespace),
        nameof(Class),
        nameof(Key),
        nameof(QueryByFilter),

        nameof(Properties),
        nameof(ViewModel),
        nameof(EvaluationStateText)
    };

    static CCM_SoftwareUpdate()
    {
        var supType = typeof(CCM_SoftwareUpdate);
        var properties = supType.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => !_propertiesToSkip.Contains(p.Name));
        _properties.AddRange(properties);
    }

    public CCM_SoftwareUpdate()
    {
        foreach (var property in _properties)
        {
            Properties.Add(new ReferenceProperty(this, property));
        }
    }
}
