using CommunityToolkit.Mvvm.ComponentModel;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Extensions;
using System;
using System.Management;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models
{
    public enum MessageLevel : uint
    {
        Information,
        Warning,
        Error
    }

    public enum Verbosity : uint
    {
        Low = 10,
        Medium_Low = 20,
        Medium = 30,
        Medium_High = 40,
        High = 50
    }

    public enum ActionType : uint
    {
        Update = 1,
        Delete,
        Reboot,
        RebootCountdonwStart, 
        Logoff,
        ProgramAvailable,
        ProgramDownloadProgress,
        OptionalProgramReady,
        AssignedProgramReady,
        ProgramExecuteComplete,
        UpdateAvailable,
        UpdateDeleted,
        UpdateManadatoryInstallStart,
        UpdateInstallComplete, 
        InstallJobStart,
        InstallJobComplete,
        AppEvaluationStarted = 19,
        AppEvaluationComplete,
        AppAvailable,
        AppEnforcementStarted,
        AppEnforcementProgress,
        AppEnforcementSuccess,
        AppEnforcementFailure,
        AppRemoved
    }

    public partial class InstanceEvent : ObservableObject
    {
        [ObservableProperty]
        private string _className;
        [ObservableProperty]
        private string _targetInstancePath;
        [ObservableProperty]
        private ActionType _actionType;
        [ObservableProperty]
        private string _userSID;
        [ObservableProperty]
        uint _sessionID;
        [ObservableProperty]
        private MessageLevel _messageLevel;
        [ObservableProperty]
        private Verbosity _verbosity;
        [ObservableProperty]
        string _value;

        public InstanceEvent(ManagementBaseObject instance)
        {
            ClassName = instance.GetPropertyValue("ClassName") as string;
            TargetInstancePath = instance.GetPropertyValue("TargetInstancePath") as string;
            ActionType = EnumExtensions.ParseOrDefault<ActionType>(instance.GetPropertyValue("ActionType") as string);
            UserSID = instance.GetPropertyValue("UserSID") as string;
            SessionID = Convert.ToUInt32(instance.GetPropertyValue("SessionID"));
            MessageLevel = EnumExtensions.ParseOrDefault<MessageLevel>(instance.GetPropertyValue("MessageLevel") as string);
            Verbosity = EnumExtensions.ParseOrDefault<Verbosity>(instance.GetPropertyValue("Verbosity") as string);
            Value = instance.GetPropertyValue("Value") as string;
        }
    }
}
