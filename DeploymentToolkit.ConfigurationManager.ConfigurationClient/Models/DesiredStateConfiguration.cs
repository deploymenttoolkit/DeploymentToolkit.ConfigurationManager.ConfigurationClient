using CommunityToolkit.Mvvm.ComponentModel;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.ViewModels;
using System;
using System.Management;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models
{
    public enum LastComplianceStatus
    {
        NonCompliant,
        Compliant,
        NotApplicable,
        Unknown,
        Error,
        NotEvaluated
    }

    public enum PolicyType
    {
        DCM,
        SettingAssignment
    }

    public enum Status
    {
        Idle,
        EvaluationStarted,
        DownloadingDocuments,
        InProgress,
        Failure,
        Reporting
    }

    public partial class DesiredStateConfiguration : ObservableObject
    {
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
        private DateTime _lastEvaluationTime;

        [ObservableProperty]
        private PolicyType _policyType;

        [ObservableProperty]
        private Status _status;

        public ConfigurationsViewModel ViewModel { get; set; }

        private readonly ManagementObject _instance;

        public DesiredStateConfiguration(ConfigurationsViewModel viewModel, ManagementObject instance)
        {
            ViewModel = viewModel;
            _instance = instance;
            UpdateInstance();
        }

        public void UpdateInstance()
        {
            //_instance.Get();

            Name = _instance.GetPropertyValue("Name") as string;
            DisplayName = _instance.GetPropertyValue("DisplayName") as string;
            Version = _instance.GetPropertyValue("Version") as string;
            IsMachineTarget = (bool)_instance.GetPropertyValue("IsMachineTarget");
            ComplianceDetails = _instance.GetPropertyValue("ComplianceDetails") as string;
            LastEvaluationTime = ManagementDateTimeConverter.ToDateTime(_instance.GetPropertyValue("LastEvalTime") as string);

            if (Enum.TryParse<LastComplianceStatus>(_instance.GetPropertyValue("LastComplianceStatus") as string, out var lastComplianceStatus))
            {
                LastComplianceStatus = lastComplianceStatus;
            }
            if (Enum.TryParse<PolicyType>(_instance.GetPropertyValue("PolicyType") as string, out var policyType))
            {
                PolicyType = policyType;
            }
            if (Enum.TryParse<Status>(_instance.GetPropertyValue("Status") as string, out var status))
            {
                Status = status;
            }
        }

        public uint Evaluate()
        {
            var desiredConfigurationClass = new ManagementClass(@"ROOT\ccm\dcm:SMS_DesiredConfiguration");

            var parameters = desiredConfigurationClass.GetMethodParameters("TriggerEvaluation");
            parameters["Name"] = Name;
            parameters["Version"] = Version;
            parameters["IsMachineTarget"] = IsMachineTarget;
            parameters["IsEnforced"] = true;
            parameters["PolicyType"] = (uint)PolicyType;

            var result = desiredConfigurationClass.InvokeMethod("TriggerEvaluation", parameters, null);
            return (uint)result.GetPropertyValue("ReturnValue");
        }

        public override bool Equals(object obj)
        {
            return obj is DesiredStateConfiguration configuration && Name == configuration.Name;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name);
        }
    }
}
