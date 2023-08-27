using CommunityToolkit.Mvvm.ComponentModel;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.WMI;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Management;
using System.Reflection;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.CCM.ClientSDK
{
    public partial class CCM_SoftwareUpdate : CCM_SoftwareBase, IWindowsManagementInstrumentationInstance
    {
        public string Namespace => CCM_Constants.ClientSDKNamespace;
        public string Class => nameof(CCM_SoftwareUpdate);
        public string Key => @$"UpdateID=""{UpdateID}""";

        internal SoftwareUpdatesPageViewModel ViewModel { get; private set; }
        internal ObservableCollection<BasicProperty> Properties { get; private set; } = new();

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

        private ManagementBaseObject _instance;

        static CCM_SoftwareUpdate()
        {
            var supType = typeof(CCM_SoftwareUpdate);
            var properties = supType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            _properties.AddRange(properties);
        }

        public CCM_SoftwareUpdate(SoftwareUpdatesPageViewModel viewModel, ManagementBaseObject softwareUpdate)
        {
            ViewModel = viewModel;
            _instance = softwareUpdate;

            UpdateInstance();

        }

        internal void UpdateInstance(bool refresh = false)
        {
            if (refresh)
            {
                _instance = ViewModel.GetSoftwareUpdate(_instance.Properties["UpdateID"].Value as string);
            }

            var instanceProperties = new List<string>(_instance.Properties.Count);
            foreach (var property in _instance.Properties)
            {
                instanceProperties.Add(property.Name);
            }

            Properties.Clear();

            foreach (var property in _properties)
            {
                if (!instanceProperties.Contains(property.Name))
                {
                    Debug.WriteLine($"Property {property.Name} not found on SoftwareUpdate");
                    continue;
                }

                var value = _instance.GetPropertyValue(property.Name);

                if (property.PropertyType == typeof(DateTime))
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
                else if (property.PropertyType.IsEnum)
                {
                    if (Enum.IsDefined(property.PropertyType, value))
                    {
                        property.SetValue(this, Enum.ToObject(property.PropertyType, value));
                    }
                    else
                    {
                        Debug.WriteLine($"Failed to parse {value} to {property.PropertyType}");
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
