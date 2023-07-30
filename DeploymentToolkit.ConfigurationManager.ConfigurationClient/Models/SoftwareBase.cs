using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models
{
    public partial class SoftwareBase : ObservableObject
    {
        [ObservableProperty]
        private uint _type;
        [ObservableProperty]
        private string _name;
        [ObservableProperty]
        private string _publisher;
        [ObservableProperty]
        private string _fullName;
        [ObservableProperty]
        private uint _contentSize;
        [ObservableProperty]
        private string _description;
        [ObservableProperty]
        private DateTime _deadline;
        [ObservableProperty]
        private DateTime _nextUserScheduledTime;
        // Evaluation state differes between Configuration Items
        //[ObservableProperty]
        //private ApplicationEvaluationState _evaluationState;
        [ObservableProperty]
        private uint _percentComplete;
        [ObservableProperty]
        private uint _errorCode;
        [ObservableProperty]
        private uint _estimatedInstallTime;
    }
}
