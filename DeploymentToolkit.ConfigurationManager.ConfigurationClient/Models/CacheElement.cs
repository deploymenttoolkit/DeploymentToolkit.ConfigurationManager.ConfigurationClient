using DeploymentToolkit.ConfigurationManager.ConfigurationClient.ViewModels;
using System;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models
{
    public class CacheElement
    {
        public CachePageViewModel ViewModel { get; set; }

        public string CacheElementId { get; set; }

        public string Location { get; set; }
        public string ContentId { get; set; }
        public string ContentVersion { get; set; }
        public int ContentSize { get; set; }
        public DateTime LastReferenceTime { get; set; }
        public int ReferenceCount { get; set; }
    }
}
