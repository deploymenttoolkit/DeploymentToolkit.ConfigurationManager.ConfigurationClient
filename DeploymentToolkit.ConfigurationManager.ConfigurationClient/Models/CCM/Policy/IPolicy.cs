using System.Collections.ObjectModel;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.CCM.Policy
{
    public enum PolicyTarget
    {
        User,
        Machine
    }

    public enum ConfigState
    {
        Actual,
        Requested
    }

    public interface IPolicy
    {
        public string DisplayName { get; set; }
        public ObservableCollection<IPolicy> Children { get; set; }

        public bool Default { get; }
        public PolicyTarget? PolicyTarget { get; }
        public ConfigState? ConfigState { get; }
        public string? SID { get; }
    }
}
