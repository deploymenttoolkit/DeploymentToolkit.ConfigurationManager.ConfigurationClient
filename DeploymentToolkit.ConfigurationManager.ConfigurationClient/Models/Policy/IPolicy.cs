using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.Policy
{
    public interface IPolicy
    {
        public string DisplayName { get; set; }
        public ObservableCollection<IPolicy> Children { get; set; }
    }
}
