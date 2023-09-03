using CommunityToolkit.Mvvm.ComponentModel;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.WMI;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.CCM.ClientSDK
{
    public enum RepeatRunBehavior : uint
    {
        RerunAlways,
        RerunIfFail,
        RerunIfSuccess,
        RerunNever
    }
    public partial class CCM_Program : CCM_SoftwareBase, IWindowsManagementInstrumentationInstance
    {
        public string Namespace => CCM_Constants.ClientSDKNamespace;
        public string Class => nameof(CCM_Program);
        public string Key => @$"PackageID=""{PackageID}"",ProgramID=""{ProgramID}""";

        internal ProgramPageViewModel ViewModel { get; set; }

        public ObservableCollection<ReferenceProperty> Properties { get; private set; } = new();

        [ObservableProperty]
        private string _packageID;
        [ObservableProperty]
        private string _packageName;
        [ObservableProperty]
        private string _programID;
        [ObservableProperty]
        private string _version;

        [ObservableProperty]
        private string _diskSpaceRequired;
        [ObservableProperty]
        private uint _duration;
        [ObservableProperty]
        private uint _completionAction;
        [ObservableProperty]
        private DateTime _activationTime;
        [ObservableProperty]
        private DateTime _expirationTime;
        [ObservableProperty]
        private ObservableCollection<string> _categories;
        [ObservableProperty]
        private bool _requiresUserInput;
        [ObservableProperty]
        private string _packageLanguage;
        [ObservableProperty]
        private string _lastRunStatus;
        [ObservableProperty]
        private DateTime _lastRunTime;
        [ObservableProperty]
        private uint _lastExitCode;
        [ObservableProperty]
        private bool _runDependent;
        [ObservableProperty]
        private string _dependentProgramID;
        [ObservableProperty]
        private string _dependentPackageID;
        [ObservableProperty]
        private bool _runAtLogon;
        [ObservableProperty]
        private bool _runAtLogoff;
        [ObservableProperty]
        private bool _published;
        [ObservableProperty]
        private bool _advertisedDirectly;
        [ObservableProperty]
        private bool _taskSequence;
        [ObservableProperty]
        private bool _highImpact;
        [ObservableProperty]
        private string _customHighImpactWarning;
        [ObservableProperty]
        private string _customHighImpactHeadline;
        [ObservableProperty]
        private string _customHighImpactWarningTop;
        [ObservableProperty]
        private string _customHighImpactWarningInstall;
        [ObservableProperty]
        private string _localizedTaskSequenceName;
        [ObservableProperty]
        private string _localizedTaskSequenceDescription;
        [ObservableProperty]
        private bool _highImpactTaskSequence;
        [ObservableProperty]
        private bool _customHighImpactSet;
        [ObservableProperty]
        private bool _restartRequired;
        [ObservableProperty]
        private uint _estimatedDownloadSizeMB;
        [ObservableProperty]
        private uint _estimatedRunTimeMinutes;
        [ObservableProperty]
        private string _icon;
        [ObservableProperty]
        private uint _level;
        [ObservableProperty]
        private ObservableCollection<CCM_Program> _dependencies;
        [ObservableProperty]
        private RepeatRunBehavior _repeatRunBehavior;
        [ObservableProperty]
        private bool _forceDependencyToRun;
        [ObservableProperty]
        private bool _notifyUser;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(EvaluationStateText))]
        // Now ApplicationEvaluationState is probably wrong for a Program but I can't seem to find a documentation for it ...
        private ApplicationEvaluationState _evaluationState;

        public override string EvaluationStateText
        {
            get
            {
                if (EvaluationState == ApplicationEvaluationState.DownloadingContent)
                {
                    return $"{EvaluationState} ({PercentComplete})";
                }
                return EvaluationState.ToString();
            }
        }

        private static readonly List<PropertyInfo> _properties = new();
        private static readonly List<string> _propertiesToSkip = new()
        {
            nameof(Properties),
            nameof(ViewModel),
            nameof(EvaluationStateText),
            nameof(Dependencies)
        };

        static CCM_Program()
        {
            var programType = typeof(CCM_Program);
            var properties = programType.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => !_propertiesToSkip.Contains(p.Name));
            _properties.AddRange(properties);
        }

        public CCM_Program()
        {
            foreach(var property in _properties)
            {
                Properties.Add(new ReferenceProperty(this, property));
            }
        }
    }
}
