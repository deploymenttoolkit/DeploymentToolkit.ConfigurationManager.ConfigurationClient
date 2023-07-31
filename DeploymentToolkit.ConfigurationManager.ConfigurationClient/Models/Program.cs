using CommunityToolkit.Mvvm.ComponentModel;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Reflection;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models
{
    public enum RepeatRunBehavior : uint
    {
        RerunAlways,
        RerunIfFail,
        RerunIfSuccess,
        RerunNever
    }
    public partial class Program : SoftwareBase
    {
        internal ProgramPageViewModel ViewModel { get; private set; }

        public ObservableCollection<BasicProperty> Properties { get; private set; } = new();

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
        private string[] _categories;
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
        private Program[] _dependencies;
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

        private readonly ManagementBaseObject _instance;

        static Program()
        {
            var programType = typeof(Program);
            var properties = programType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            _properties.AddRange(properties);
        }
        
        public Program(ProgramPageViewModel viewModel, ManagementBaseObject program)
        {
            ViewModel = viewModel;
            _instance = program;

            UpdateInstance();
        }
        private void UpdateInstance()
        {
            var instanceProperties = new List<string>(_instance.Properties.Count);
            foreach(var property in _instance.Properties)
            {
                instanceProperties.Add(property.Name);
            }

            foreach(var property in _properties)
            {
                if (!instanceProperties.Contains(property.Name))
                {
                    Debug.WriteLine($"Property {property.Name} not found on Program");
                    continue;
                }

                var value = _instance.GetPropertyValue(property.Name);
                
                if(property.PropertyType == typeof(DateTime))
                {
                    if (value == null)
                    {
                        property.SetValue(this, DateTime.MinValue);
                    }
                    else
                    {
                        property.SetValue(this, ManagementDateTimeConverter.ToDateTime(value as string));
                    }
                }
                else if(property.PropertyType.IsEnum)
                {
                    if (Enum.IsDefined(property.PropertyType, value))
                    {
                        if (value is string)
                        {
                            property.SetValue(this, Enum.Parse(property.PropertyType, value as string));
                        }
                        else
                        {
                            property.SetValue(this, Enum.ToObject(property.PropertyType, value));
                        }
                    }
                    else
                    {
                        Debug.WriteLine($"Failed to parse {value} to {property.PropertyType}");
                    }
                }
                else if(property.PropertyType.IsArray)
                {
                    if(value == null)
                    {
                        continue;
                    }

                    if(property.Name == nameof(Dependencies))
                    {
                        var programs = value as IEnumerable<ManagementBaseObject>;
                        Dependencies = new Program[programs.Count()];
                        for(var i = 0; i <  programs.Count(); i++)
                        {
                            Dependencies[i] = new Program(ViewModel, programs.ElementAt(i));
                        }
                    }
                    else
                    {
                        property.SetValue(this, value);
                    }
                }
                else
                {
                    property.SetValue(this, value);
                }

                Properties.Add(new BasicProperty(property.Name, property.GetValue(this)));
            }
        }
    }
}
