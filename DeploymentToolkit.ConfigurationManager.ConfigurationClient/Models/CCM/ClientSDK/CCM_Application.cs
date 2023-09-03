using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using System.Collections.ObjectModel;
using System;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.WMI;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.ViewModels;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Xml.Serialization;

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
        Any,
        Available // Not sure if this is right
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

        public ApplicationsPageViewModel ViewModel { get; set; }

        public ObservableCollection<ReferenceProperty> Properties { get; private set; } = new();

        [ObservableProperty]
        private string _id;
        [ObservableProperty]
        private string _revision;
        [ObservableProperty]
        private ApplicationType _applicationType;
        [ObservableProperty]
        private ObservableCollection<CCM_Application> _appDTs = new();

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
        [NotifyPropertyChangedFor(nameof(Installable))]
        [NotifyPropertyChangedFor(nameof(Uninstallable))]
        [NotifyPropertyChangedFor(nameof(Repairable))]
        private ObservableCollection<string> _allowedActions = new();

        public bool Installable => AllowedActions.Contains("Install");
        public bool Uninstallable => AllowedActions.Contains("Uninstall");
        public bool Repairable => AllowedActions.Contains("Repair");

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
        private DateTime _lastEvalTime;
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

        private static readonly List<PropertyInfo> _properties = new();
        private static readonly List<string> _propertiesToSkip = new()
        {
            nameof(Properties),
            nameof(ViewModel),
            nameof(AppDTs),
            nameof(EvaluationStateText),
            nameof(Installable),
            nameof(Uninstallable),
            nameof(Repairable)
        };

        static CCM_Application()
        {
            var programType = typeof(CCM_Application);
            var properties = programType.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => !_propertiesToSkip.Contains(p.Name));
            _properties.AddRange(properties);
        }

        public CCM_Application()
        {
            foreach (var property in _properties)
            {
                Properties.Add(new ReferenceProperty(this, property));
            }
        }
    }
}
