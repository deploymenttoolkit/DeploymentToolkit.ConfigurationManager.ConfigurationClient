using CommunityToolkit.Mvvm.ComponentModel;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Extensions;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.CCM.ClientSDK;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.ViewModels;
using Microsoft.UI.Xaml;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management;
using static System.Net.Mime.MediaTypeNames;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models
{
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

    public partial class Application : CCM_SoftwareBase
    {
        public ApplicationsPageViewModel ViewModel { get; private set; }

        public ObservableCollection<BasicProperty> Properties { get; private set; } = new();

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

        private ManagementBaseObject _instance;
        private readonly bool _isPartOfApplicationGroup;

        public Application(ApplicationsPageViewModel viewModel, ManagementBaseObject application, bool isPartOfApplicationGroup = false)
        {
            ViewModel = viewModel;

            _instance = application;
            _isPartOfApplicationGroup = isPartOfApplicationGroup;

            UpdateInstance();
        }

        internal void UpdateInstance(bool refresh = false)
        {
            if(refresh)
            {
                _instance = ViewModel.GetApplication(_instance.Properties["Id"].Value as string, _instance.Properties["Revision"].Value as string, Convert.ToBoolean(_instance.Properties["IsMachineTarget"].Value));
            }

            Properties.Clear();

            Id = _instance.GetPropertyValue("Id") as string;
            Revision = _instance.GetPropertyValue("Revision") as string;
            ApplicationType = Id.Contains("ApplicationGroup_") ? ApplicationType.ApplicationGroup : ApplicationType.Application;
            if (ApplicationType == ApplicationType.ApplicationGroup)
            {
                var applications = _instance.GetPropertyValue("AppDTs") as ManagementBaseObject[];
                foreach (var application in applications)
                {
                    Applications.Add(new Application(ViewModel, application, true));
                }
                EmbeddedApplicationVisibility = Visibility.Visible;
            }

            Name = _instance.GetPropertyValue("Name") as string;
            Description = _instance.GetPropertyValue("Description") as string;

            if (!_isPartOfApplicationGroup)
            {
                IsMachineTarget = Convert.ToBoolean(_instance.GetPropertyValue("IsMachineTarget"));
                OverrideServiceWindow = Convert.ToBoolean(_instance.GetPropertyValue("OverrideServiceWindow"));
                RebootOutsideServiceWindow = Convert.ToBoolean(_instance.GetPropertyValue("RebootOutsideServiceWindow"));
            }

            var allowedActions = _instance.GetPropertyValue("AllowedActions") as string[];
            Installable = allowedActions.Contains("Install");
            Uninstallable = allowedActions.Contains("Uninstall");
            Repairable = allowedActions.Contains("Repair");

            EvaluationState = EnumExtensions.ParseOrDefault<ApplicationEvaluationState>(_instance.GetPropertyValue("EvaluationState").ToString());
            ApplicabilityState = EnumExtensions.ParseOrDefault<ApplicabilityState>(_instance.GetPropertyValue("ApplicabilityState") as string);
            InstallState = EnumExtensions.ParseOrDefault<InstallState>(_instance.GetPropertyValue("InstallState") as string);
            ResolvedState = EnumExtensions.ParseOrDefault<ResolvedState>(_instance.GetPropertyValue("ResolvedState") as string);
            ConfigureState = EnumExtensions.ParseOrDefault<ConfigureState>(_instance.GetPropertyValue("ConfigureState") as string);
            if (!_isPartOfApplicationGroup)
            {
                EnforcePreference = EnumExtensions.ParseOrDefault<EnforcePreference>(_instance.GetPropertyValue("EnforcePreference").ToString());
                SupersessionState = EnumExtensions.ParseOrDefault<SupersessionState>(_instance.GetPropertyValue("SupersessionState") as string);


                SoftwareVersion = _instance.GetPropertyValue("SoftwareVersion") as string;
                InformativeUrl = _instance.GetPropertyValue("InformativeUrl") as string;
                InfoUrlText = _instance.GetPropertyValue("InfoUrlText") as string;
                Icon = _instance.GetPropertyValue("Icon") as string;
                PrivacyUri = _instance.GetPropertyValue("PrivacyUri") as string;
                var releaseDate = _instance.GetPropertyValue("ReleaseDate") as string;
                if (releaseDate != "00000000000000.000000+000")
                {
                    ReleaseDate = ManagementDateTimeConverter.ToDateTime(_instance.GetPropertyValue("ReleaseDate") as string);
                }
                FileTypes = _instance.GetPropertyValue("FileTypes") as string;
                LastInstallTime = ManagementDateTimeConverter.ToDateTime(_instance.GetPropertyValue("LastInstallTime") as string);
                StartTime = ManagementDateTimeConverter.ToDateTime(_instance.GetPropertyValue("StartTime") as string);
                NotifyUser = Convert.ToBoolean(_instance.GetPropertyValue("NotifyUser"));
                UserUIExperience = Convert.ToBoolean(_instance.GetPropertyValue("UserUIExperience"));
                IsPreflightOnly = Convert.ToBoolean(_instance.GetPropertyValue("IsPreflightOnly"));
                HighImpactDeployment = Convert.ToBoolean(_instance.GetPropertyValue("HighImpactDeployment"));
            }

            // Properties to be viewed in details
            Properties.Add(new BasicProperty(nameof(Id), Id));
            Properties.Add(new BasicProperty(nameof(Revision), Revision));
            Properties.Add(new BasicProperty(nameof(ConfigureState), ConfigureState));
            Properties.Add(new BasicProperty(nameof(EvaluationState), EvaluationState));
            if (!_isPartOfApplicationGroup)
            {
                Properties.Add(new BasicProperty(nameof(EnforcePreference), EnforcePreference));
                Properties.Add(new BasicProperty(nameof(SupersessionState), SupersessionState));
                Properties.Add(new BasicProperty(nameof(StartTime), StartTime));
                Properties.Add(new BasicProperty(nameof(NotifyUser), NotifyUser));
                Properties.Add(new BasicProperty(nameof(UserUIExperience), UserUIExperience));
                Properties.Add(new BasicProperty(nameof(IsPreflightOnly), IsPreflightOnly));
                Properties.Add(new BasicProperty(nameof(HighImpactDeployment), HighImpactDeployment));
            }
        }
    }
}
