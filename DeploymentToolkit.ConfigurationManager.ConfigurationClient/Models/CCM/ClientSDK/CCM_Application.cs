using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using System.Collections.ObjectModel;
using System;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.WMI;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.CCM.ClientSDK
{
    public enum ApplicationEvaluationState : uint
    {
        Unknown,
        Enforced,
        NotRequired,
        ApplicationForEnforcement,
        EnforcementFailed,
        Evaluating,
        DownloadingContent,
        WaitingforDependenciesDownload,
        WaitingforServiceWindow,
        WaitingforReboot,
        WaitingToEnforce,
        EnforcingDependencies,
        Enforcing,
        SoftRebootPending,
        HardRebootPending,
        PendingUpdate,
        EvaluationFailed,
        WaitingUserReconnect,
        WaitingforUserLogoff,
        WaitingforUserLogon,
        InProgressWaitingRetry,
        WaitingforPresModeOff,
        AdvanceDownloadingContent,
        AdvanceDependenciesDownload,
        DownloadFailed,
        AdvanceDownloadFailed,
        DownloadSuccess,
        PostEnforceEvaluation
    }

    public enum ApplicabilityState : uint
    {
        Unknown,
        Applicable,
        NotApplicable
    }

    public enum InstallState : uint
    {
        NotInstalled,
        Unknown,
        Error,
        Installed,
        NotEvaluated,
        NotUpdated,
        NotConfigured
    }

    public enum ResolvedState : uint
    {
        None,
        NotInstalled,
        Installed,
        Unknown,
        Any
    }

    public enum ConfigureState : uint
    {
        NotNeeded,
        NotConfigured,
        Configured
    }

    public enum EnforcePreference : uint
    {
        Immediate,
        NonBusinessHours,
        AdminSchedule
    }

    public enum SupersessionState : uint
    {
        Unknown,
        None,
        Superseded,
        Superseding
    }

    public enum ApplicationType : uint
    {
        Application,
        ApplicationGroup
    }

    public partial class CCM_Application : CCM_SoftwareBase, IWindowsManagementInstrumentationInstance
    {
        public string Namespace => CCM_Constants.ClientSDKNamespace;
        public string Class => nameof(CCM_Application);
        public string Key => @$"Id=""{Id}"",Revision=""{Revision}"",IsMachineTarget={(IsMachineTarget ? "true" : "false")}";

        [ObservableProperty]
        private string _id;
        [ObservableProperty]
        private string _revision;
        [ObservableProperty]
        private ApplicationType _applicationType;
        [ObservableProperty]
        private ObservableCollection<Application> _applications = new();
        [ObservableProperty]
        private Visibility _embeddedApplicationVisibility = Visibility.Collapsed;

        [ObservableProperty]
        private string _name;
        [ObservableProperty]
        private string _description;

        [ObservableProperty]
        private bool _isMachineTarget;
        [ObservableProperty]
        private bool _overrideServiceWindow;
        [ObservableProperty]
        private bool _rebootOutsideServiceWindow;

        [ObservableProperty]
        private bool _installable;
        [ObservableProperty]
        private bool _uninstallable;
        [ObservableProperty]
        private bool _repairable;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(EvaluationStateText))]
        private ApplicationEvaluationState _evaluationState;
        [ObservableProperty]
        private ApplicabilityState _applicabilityState;
        [ObservableProperty]
        private InstallState _installState;
        [ObservableProperty]
        private ResolvedState _resolvedState;
        [ObservableProperty]
        private ConfigureState _configureState;
        [ObservableProperty]
        private EnforcePreference _enforcePreference;
        [ObservableProperty]
        private SupersessionState _supersessionState;

        [ObservableProperty]
        private string _softwareVersion;
        [ObservableProperty]
        private string _informativeUrl;
        [ObservableProperty]
        private string _infoUrlText;
        [ObservableProperty]
        private string _icon;
        [ObservableProperty]
        private string _privacyUri;
        [ObservableProperty]
        private DateTime _releaseDate;
        [ObservableProperty]
        private string _fileTypes;
        [ObservableProperty]
        private DateTime _lastInstallTime;
        [ObservableProperty]
        private DateTime _startTime;
        [ObservableProperty]
        private bool _notifyUser;
        [ObservableProperty]
        private bool _userUIExperience;
        [ObservableProperty]
        private bool _isPreflightOnly;
        [ObservableProperty]
        private bool _highImpactDeployment;

        public override string EvaluationStateText
        {
            get
            {
                if (EvaluationState == ApplicationEvaluationState.DownloadingContent)
                {
                    return $"{EvaluationState} ({PercentComplete}%)";
                }
                return EvaluationState.ToString();
            }
        }
    }
}
