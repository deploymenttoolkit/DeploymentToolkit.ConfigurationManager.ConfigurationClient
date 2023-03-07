using CommunityToolkit.Mvvm.ComponentModel;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.ViewModels;
using System;
using Vanara.IO;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models
{
    public partial class BITSJob : ObservableObject
    {
        public BackgroundCopyJob Job { get; private set; }

        public BITSPageViewModel ViewModel { get; private set; }

        public BITSJob(BITSPageViewModel viewModel, BackgroundCopyJob job)
        {
            Job = job;
            Id = job.ID;
            ViewModel = viewModel;
        }

        [ObservableProperty]
        private bool _cannotBeCancelled;

        [ObservableProperty]
        private Guid _id;

        [ObservableProperty]
        private string _displayName;
        [ObservableProperty]
        private string _description;
        [ObservableProperty]
        private byte _percentComplete;

        [ObservableProperty]
        private DateTime _creationTime;
        [ObservableProperty]
        private DateTime _modificationTime;

        [ObservableProperty]
        private BackgroundCopyJobType _jobType;

        [ObservableProperty]
        private BackgroundCopyException _lastError;


        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if(obj is not BITSJob job) return false;
            return Id == job.Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
