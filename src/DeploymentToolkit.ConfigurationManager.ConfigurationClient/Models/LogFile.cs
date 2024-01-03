using CommunityToolkit.Mvvm.ComponentModel;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.ViewModels;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models
{
    public partial class LogFile : ObservableObject
    {
        [ObservableProperty]
        private string _name;

        [ObservableProperty]
        private DateTime? _created;
        [ObservableProperty]
        private DateTime? _lastModified;

        [ObservableProperty]
        private IEnumerable<LogFile> _files;

        [ObservableProperty]
        private string _path;

        public LogPageViewModel ViewModel { get; set; }

        public LogFile(LogPageViewModel viewModel, string name, string path)
        {
            ViewModel = viewModel;
            _name = name;
            _path = path;
        }

        public override bool Equals(object obj)
        {
            if(obj is not LogFile logFile)
            {
                return false;
            }
            return logFile.Path == this.Path;
        }

        public override int GetHashCode()
        {
            return this.Path.GetHashCode();
        }
    }
}
