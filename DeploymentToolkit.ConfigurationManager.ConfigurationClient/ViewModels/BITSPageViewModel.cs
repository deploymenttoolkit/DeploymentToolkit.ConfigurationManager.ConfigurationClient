using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI.UI.Controls.TextToolbarSymbols;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Services;
using Microsoft.UI.Xaml;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Timers;
using Vanara.IO;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.ViewModels
{
    public partial class BITSPageViewModel : ObservableObject
    {
        [ObservableProperty]
        private Visibility _errorMessageVisibility = Visibility.Collapsed;
        [ObservableProperty]
        private bool _enableContent = true;

        [ObservableProperty]
        private DateTime _lastUpdated;

        private ObservableCollection<BITSJob> _bitsJobs = new();
        public ObservableCollection<BITSJob> BITSJobs => _bitsJobs;

        private System.Collections.Generic.IEnumerable<Guid> _jobs;
        private Timer _jobUpdateTimer;

        private UACService _uacService;

        public BITSPageViewModel(UACService uacService)
        {
            _uacService = uacService;

            if(!_uacService.IsElevated)
            {
                ErrorMessageVisibility = Visibility.Visible;
                EnableContent = false;
                return;
            }

            _jobUpdateTimer = new Timer(5000);
            _jobUpdateTimer.Elapsed += UpdateJobsTimerElapsed;
            _jobUpdateTimer.AutoReset = true;
            _jobUpdateTimer.Start();

            UpdateJobList();
        }

        ~BITSPageViewModel()
        {
            _jobUpdateTimer?.Dispose();
        }

        private void UpdateJobsTimerElapsed(object sender, ElapsedEventArgs e)
        {
            var newJobs = BackgroundCopyManager.Jobs;

            if (_jobs.Any(j => !newJobs.Contains(j)) || newJobs.Any(j => !_jobs.Contains(j.ID)))
            {
                App.Current.DispatcherQueue.TryEnqueue(() =>
                {
                    UpdateJobList();
                });
            }
        }

        [RelayCommand]
        private void TryUpdateJobs()
        {
            App.Current.DispatcherQueue.TryEnqueue(() =>
            {
                UpdateJobList();
            });
        }

        private void UpdateJobList()
        {
            var jobs = BackgroundCopyManager.Jobs;
            foreach (BackgroundCopyJob job in jobs)
            {
                BITSJob bitsJob = BITSJobs.FirstOrDefault(j => j.Id == job.ID);
                if(bitsJob == null)
                {
                    bitsJob = new BITSJob(this, job);
                    BITSJobs.Add(bitsJob);

                    job.Modified += JobModified;
                }

                bitsJob.DisplayName = job.DisplayName;
                bitsJob.Description = job.Description;
                bitsJob.CreationTime = job.CreationTime;
                bitsJob.ModificationTime = job.ModificationTime;
                bitsJob.JobType = job.JobType;
                bitsJob.LastError = job.LastError;
                bitsJob.PercentComplete = job.Progress.PercentComplete;

                bitsJob.CannotBeCancelled = job.State != BackgroundCopyJobState.Cancelled && job.State != BackgroundCopyJobState.Transferred;
            }

            var deletedJobs = BITSJobs.Where(j => !jobs.Contains(j.Id)).ToList();
            foreach (var job in deletedJobs)
            {
                BITSJobs.Remove(job);
            }

            _jobs = jobs.Select(j => j.ID).ToList();
            LastUpdated = DateTime.Now;
        }

        private void JobModified(object sender, BackgroundCopyJobEventArgs e)
        {
            var job = e.Job;
            var bitsJob = BITSJobs.FirstOrDefault(j => j.Id == job.ID);
            if(bitsJob == null) { return; }

            App.Current.DispatcherQueue.TryEnqueue(() =>
            {
                bitsJob.DisplayName = job.DisplayName;
                bitsJob.Description = job.Description;
                bitsJob.ModificationTime = job.ModificationTime;
                bitsJob.JobType = job.JobType;
                bitsJob.LastError = job.LastError;
                bitsJob.PercentComplete = job.Progress.PercentComplete;
            });
        }

        [RelayCommand]
        private void RestartButton()
        {
            _uacService.RestartAsAdmin();
        }


        [RelayCommand]
        private void CancelJob(Guid jobId)
        {
            if(BackgroundCopyManager.Jobs.Contains(jobId))
            {
                BackgroundCopyManager.Jobs[jobId].Cancel();
            }    

            App.Current.DispatcherQueue.TryEnqueue(() =>
            {
                UpdateJobList();
            });
        }
    }
}
